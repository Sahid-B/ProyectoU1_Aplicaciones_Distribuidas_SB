using System.Threading.Tasks;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public interface IPaymentService
    {
        Task<TransaccionPago> IniciarPagoAsync(int ventaId, string provider);
        Task<TransaccionPago?> ConfirmarPagoPayPalAsync(string token);
        Task CancelarPagoPayPalAsync(string token);
        Task MarcarComoPagadoDemostracionAsync(int transaccionId, string? usuarioId = null);
        Task<TransaccionPago?> ObtenerTransaccionPorIdAsync(int id);
    }
}
