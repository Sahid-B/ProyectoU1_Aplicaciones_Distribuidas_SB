using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MiniMarketApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error del proveedor externo: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error al cargar la información del inicio de sesión externo.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                ErrorMessage = "No se pudo obtener el correo electrónico desde Google.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // 1. Verificar si el usuario ya existe por email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Determinar un nombre de usuario a partir del correo
                var username = email.Split('@')[0];
                // Asegurarse de que el nombre de usuario sea único
                var existingUserByUsername = await _userManager.FindByNameAsync(username);
                if (existingUserByUsername != null)
                {
                    username = $"{username}_{DateTime.UtcNow.Ticks % 10000}";
                }

                // Crear nueva cuenta
                user = new IdentityUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    ErrorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                // Asignar rol Cliente por defecto
                if (!await _roleManager.RoleExistsAsync("Cliente"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                }
                await _userManager.AddToRoleAsync(user, "Cliente");
            }
            else
            {
                // Si el usuario existe pero no tiene el correo confirmado, lo confirmamos ya que viene de Google
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
            }

            // 2. Verificar si el login ya está asociado a este usuario
            var logins = await _userManager.GetLoginsAsync(user);
            var alreadyAssociated = logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey);

            if (!alreadyAssociated)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    ErrorMessage = $"Error al asociar la cuenta de Google: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }

            // 3. Iniciar sesión respetando 2FA
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario autenticado con Google y sesión iniciada.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            
            // Fallback si por alguna razón falla el login externo a pesar de estar asociado
            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            _logger.LogInformation("Usuario autenticado con Google (Fallback) y sesión iniciada.");
            return LocalRedirect(returnUrl);
        }

        [TempData]
        public string? ErrorMessage { get; set; }
    }
}
