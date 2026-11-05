using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public interface IPromotionsService
    {
        Task<decimal> CalcularDescuentoPromocionesAsync(List<DetalleVenta> detalles);
        decimal CalcularDescuentoPorPuntos(int puntos, decimal totalVenta);
        int CalcularPuntosGanados(decimal totalVenta);
    }
}
