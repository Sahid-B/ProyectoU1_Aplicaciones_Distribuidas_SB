using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MiniMarketApp.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegistrarUsuarioAsync(string email, string password, string role = "Cliente");
        Task<bool> ConfirmarEmailAsync(string userId, string code);
        Task EnviarCorreoConfirmacionAsync(IdentityUser user, string callbackUrl);
        Task EnviarCorreoRecuperacionAsync(string email, string callbackUrl);
        Task<IdentityResult> ResetearPasswordAsync(string email, string token, string newPassword);
        Task<(string SecretKey, string QrCodeUri)> GenerarConfiguracionMfaAsync(IdentityUser user);
        Task<bool> HabilitarMfaAsync(IdentityUser user, string code);
    }
}
