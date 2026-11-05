using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MiniMarketApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre de usuario es requerido.")]
            [StringLength(50, ErrorMessage = "El nombre de usuario debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 3)]
            [Display(Name = "Nombre de Usuario")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo electrónico es requerido.")]
            [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
            [Display(Name = "Correo Electrónico")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es requerida.")]
            [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Verificar si ya existe un usuario registrado con este Username o Email
                var existingUser = await _userManager.FindByNameAsync(Input.Username) 
                                   ?? await _userManager.FindByEmailAsync(Input.Email);

                if (existingUser != null)
                {
                    // Si el usuario existe pero aún no ha confirmado su correo electrónico
                    if (!existingUser.EmailConfirmed)
                    {
                        var resendCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                        HttpContext.Session.SetString("VerificationCode", resendCode);
                        HttpContext.Session.SetString("VerificationUserId", existingUser.Id);
                        HttpContext.Session.SetString("VerificationEmail", existingUser.Email ?? string.Empty);

                        await _emailSender.SendEmailAsync(
                            existingUser.Email ?? string.Empty,
                            "Código de Verificación - Supermarket Rubik",
                            $"Tu código de verificación de 6 dígitos para activar tu cuenta es: <strong>{resendCode}</strong>");

                        return RedirectToPage("./RegisterConfirmation", new { userId = existingUser.Id, email = existingUser.Email, code = resendCode });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"El usuario '{Input.Username}' o correo '{Input.Email}' ya está registrado. Por favor inicia sesión.");
                        return Page();
                    }
                }

                var user = new IdentityUser 
                { 
                    UserName = Input.Username, 
                    Email = Input.Email, 
                    EmailConfirmed = false 
                };
                
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creó una nueva cuenta con contraseña.");
                    
                    // Asignar rol Cliente
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // Generar código de verificación aleatorio de 6 dígitos
                    var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                    // Guardar en sesión
                    HttpContext.Session.SetString("VerificationCode", code);
                    HttpContext.Session.SetString("VerificationUserId", user.Id);
                    HttpContext.Session.SetString("VerificationEmail", user.Email ?? string.Empty);

                    // Enviar correo electrónico con el código
                    await _emailSender.SendEmailAsync(
                        user.Email ?? string.Empty, 
                        "Código de Verificación - Supermarket Rubik", 
                        $"Tu código de verificación de 6 dígitos para activar tu cuenta es: <strong>{code}</strong>");

                    // Redireccionar incluyendo parámetros para soportar balanceo de carga en Docker Swarm
                    return RedirectToPage("./RegisterConfirmation", new { userId = user.Id, email = user.Email, code = code });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
