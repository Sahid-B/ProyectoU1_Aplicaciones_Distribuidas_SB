using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterConfirmationModel> _logger;

        public RegisterConfirmationModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            ILogger<RegisterConfirmationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Email { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Code { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El código de verificación es obligatorio.")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener exactamente 6 dígitos.")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "El código solo debe contener números.")]
            public string Code { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? userId = null, string? email = null, string? code = null)
        {
            UserId = !string.IsNullOrEmpty(userId) ? userId : HttpContext.Session.GetString("VerificationUserId");
            Email = !string.IsNullOrEmpty(email) ? email : HttpContext.Session.GetString("VerificationEmail");
            Code = !string.IsNullOrEmpty(code) ? code : HttpContext.Session.GetString("VerificationCode");

            if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("./Register");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? userId = null, string? email = null, string? code = null)
        {
            UserId = !string.IsNullOrEmpty(userId) ? userId : (!string.IsNullOrEmpty(UserId) ? UserId : HttpContext.Session.GetString("VerificationUserId"));
            Email = !string.IsNullOrEmpty(email) ? email : (!string.IsNullOrEmpty(Email) ? Email : HttpContext.Session.GetString("VerificationEmail"));
            var expectedCode = !string.IsNullOrEmpty(code) ? code : (!string.IsNullOrEmpty(Code) ? Code : HttpContext.Session.GetString("VerificationCode"));

            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(Email))
            {
                return RedirectToPage("./Register");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            IdentityUser? user = null;
            if (!string.IsNullOrEmpty(UserId))
            {
                user = await _userManager.FindByIdAsync(UserId);
            }
            if (user == null && !string.IsNullOrEmpty(Email))
            {
                user = await _userManager.FindByEmailAsync(Email);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return Page();
            }

            // Verificar el código ingresado contra el código esperado de la sesión/ruta o token TOTP de la DB
            bool isCodeValid = false;
            if (!string.IsNullOrEmpty(expectedCode) && Input.Code.Trim() == expectedCode.Trim())
            {
                isCodeValid = true;
            }
            else
            {
                // Fallback a verificación mediante token provider de Identity en la base de datos
                isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", Input.Code.Trim());
            }

            if (isCodeValid)
            {
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    HttpContext.Session.Remove("VerificationCode");
                    HttpContext.Session.Remove("VerificationUserId");
                    HttpContext.Session.Remove("VerificationEmail");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("Usuario {Email} confirmó su correo exitosamente.", user.Email);
                    return RedirectToPage("/Index", new { area = "" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "El código de 6 dígitos ingresado es incorrecto o ha expirado.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendAsync(string? userId = null, string? email = null)
        {
            UserId = !string.IsNullOrEmpty(userId) ? userId : (!string.IsNullOrEmpty(UserId) ? UserId : HttpContext.Session.GetString("VerificationUserId"));
            Email = !string.IsNullOrEmpty(email) ? email : (!string.IsNullOrEmpty(Email) ? Email : HttpContext.Session.GetString("VerificationEmail"));

            if (string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Email))
            {
                var targetUser = await _userManager.FindByEmailAsync(Email);
                if (targetUser != null) UserId = targetUser.Id;
            }

            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("./Register");
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return RedirectToPage("./Register");
            }

            // Generar nuevo código de 6 dígitos
            var newCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            Code = newCode;
            HttpContext.Session.SetString("VerificationCode", newCode);

            // Enviar correo
            await _emailSender.SendEmailAsync(
                user.Email ?? Email ?? string.Empty, 
                "Nuevo Código de Verificación - Supermarket Rubik", 
                $"Tu nuevo código de verificación de 6 dígitos es: <strong>{newCode}</strong>");

            TempData["SuccessMessage"] = "Se ha enviado un nuevo código a tu correo electrónico.";
            return RedirectToPage(new { userId = user.Id, email = user.Email, code = newCode });
        }
    }
}
