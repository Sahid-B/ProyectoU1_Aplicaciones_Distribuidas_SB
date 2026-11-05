using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace MiniMarketApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly UrlEncoder _urlEncoder;
        private readonly IAuditService _auditService;

        public AccountService(
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            UrlEncoder urlEncoder,
            IAuditService auditService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _urlEncoder = urlEncoder;
            _auditService = auditService;
        }

        public async Task<IdentityResult> RegistrarUsuarioAsync(string email, string password, string role = "Cliente")
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = false };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                await _auditService.RegistrarAccionAsync("Seguridad", "RegistroUsuario", $"Usuario registrado: {email} con rol {role}");
            }

            return result;
        }

        public async Task<bool> ConfirmarEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

            if (result.Succeeded)
            {
                await _auditService.RegistrarAccionAsync("Seguridad", "ConfirmarEmail", $"Correo confirmado para usuario: {user.Email}");
            }

            return result.Succeeded;
        }

        public async Task EnviarCorreoConfirmacionAsync(IdentityUser user, string callbackUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var messageHtml = $"Por favor confirma tu cuenta en Market Rubik haciendo clic en el siguiente enlace: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Confirmar mi Cuenta</a>";
            await _emailService.SendEmailAsync(user.Email!, "Confirmación de Cuenta - Market Rubik", messageHtml);
        }

        public async Task EnviarCorreoRecuperacionAsync(string email, string callbackUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return;
            }

            var messageHtml = $"Para restablecer la contraseña de tu cuenta en Market Rubik, haz clic en: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Restablecer Contraseña</a>";
            await _emailService.SendEmailAsync(email, "Restablecer Contraseña - Market Rubik", messageHtml);
            await _auditService.RegistrarAccionAsync("Seguridad", "RecuperarPassword", $"Correo de recuperación de contraseña enviado a: {email}");
        }

        public async Task<IdentityResult> ResetearPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (result.Succeeded)
            {
                await _auditService.RegistrarAccionAsync("Seguridad", "ResetearPassword", $"Contraseña restablecida exitosamente para: {email}");
            }

            return result;
        }

        public async Task<(string SecretKey, string QrCodeUri)> GenerarConfiguracionMfaAsync(IdentityUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            string formattedKey = FormatKey(unformattedKey!);
            string qrCodeUri = GenerateQrCodeUri(user.Email!, unformattedKey!);

            return (formattedKey, qrCodeUri);
        }

        public async Task<bool> HabilitarMfaAsync(IdentityUser user, string code)
        {
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!is2faTokenValid)
            {
                return false;
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _auditService.RegistrarAccionAsync("Seguridad", "HabilitarMFA", $"Autenticación multifactor TOTP habilitada para: {user.Email}");
            return true;
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                _urlEncoder.Encode("Market Rubik"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
