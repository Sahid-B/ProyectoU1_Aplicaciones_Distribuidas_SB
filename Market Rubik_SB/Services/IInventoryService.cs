using System.Collections.Generic;
using System.Threading.Tasks;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public interface IInventoryService
    {
        Task<MovimientoInventario> RegistrarMovimientoAsync(int productoId, int? sucursalId, string tipoMovimiento, int cantidad, string referencia, string? usuarioId = null, string? observacion = null);
        Task<bool> ProcesarVentaStockAsync(int ventaId, IEnumerable<DetalleVenta> detalles, string? usuarioId = null);
        Task ReintegrarStockVentaCanceladaAsync(int ventaId, IEnumerable<DetalleVenta> detalles, string? usuarioId = null);
        Task<(IEnumerable<MovimientoInventario> Items, int TotalCount)> ObtenerKardexAsync(int? productoId, int page = 1, int pageSize = 20);
        Task<bool> AumentarStockAsync(int productoId, int cantidad, int? sucursalId = null, string? usuarioId = null, string? observacion = null);
    }
}
