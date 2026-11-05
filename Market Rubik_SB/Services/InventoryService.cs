using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public InventoryService(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<MovimientoInventario> RegistrarMovimientoAsync(
            int productoId, 
            int? sucursalId, 
            string tipoMovimiento, 
            int cantidad, 
            string referencia, 
            string? usuarioId = null, 
            string? observacion = null)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                throw new KeyNotFoundException($"El producto con ID {productoId} no fue encontrado.");
            }

            int stockAnterior = producto.Stock;
            int stockPosterior = stockAnterior;

            if (tipoMovimiento.Equals("Compra", StringComparison.OrdinalIgnoreCase) ||
                tipoMovimiento.Equals("Devolucion", StringComparison.OrdinalIgnoreCase) ||
                tipoMovimiento.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
            {
                stockPosterior += cantidad;
            }
            else if (tipoMovimiento.Equals("Venta", StringComparison.OrdinalIgnoreCase) ||
                     tipoMovimiento.Equals("Salida", StringComparison.OrdinalIgnoreCase) ||
                     tipoMovimiento.Equals("AjusteNegativo", StringComparison.OrdinalIgnoreCase))
            {
                if (stockAnterior < cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Stock disponible: {stockAnterior}, solicitado: {cantidad}.");
                }
                stockPosterior -= cantidad;
            }
            else if (tipoMovimiento.Equals("Ajuste", StringComparison.OrdinalIgnoreCase))
            {
                stockPosterior = cantidad;
            }

            producto.Stock = stockPosterior;
            producto.FechaActualizacion = DateTime.UtcNow;

            var movimiento = new MovimientoInventario
            {
                ProductoId = productoId,
                SucursalId = sucursalId,
                TipoMovimiento = tipoMovimiento,
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = stockPosterior,
                Referencia = referencia,
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = usuarioId,
                Observacion = observacion
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();

            await _auditService.RegistrarAccionAsync("Inventario", "Movimiento", $"Movimiento {tipoMovimiento} de {cantidad} unidades para {producto.Nombre}. Ref: {referencia}", usuarioId);

            return movimiento;
        }

        public async Task<bool> ProcesarVentaStockAsync(int ventaId, IEnumerable<DetalleVenta> detalles, string? usuarioId = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detalle in detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto == null || producto.Stock < detalle.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    int stockAnterior = producto.Stock;
                    producto.Stock -= detalle.Cantidad;
                    producto.FechaActualizacion = DateTime.UtcNow;

                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = detalle.ProductoId,
                        TipoMovimiento = "Venta",
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockPosterior = producto.Stock,
                        Referencia = $"Venta #{ventaId}",
                        FechaMovimiento = DateTime.UtcNow,
                        UsuarioId = usuarioId,
                        Observacion = $"Descuento de stock por Venta #{ventaId}"
                    };

                    _context.MovimientosInventario.Add(movimiento);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService.RegistrarAccionAsync("Inventario", "DescuentoVenta", $"Descuento transaccional de stock completado para Venta #{ventaId}", usuarioId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReintegrarStockVentaCanceladaAsync(int ventaId, IEnumerable<DetalleVenta> detalles, string? usuarioId = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detalle in detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        int stockAnterior = producto.Stock;
                        producto.Stock += detalle.Cantidad;
                        producto.FechaActualizacion = DateTime.UtcNow;

                        var movimiento = new MovimientoInventario
                        {
                            ProductoId = detalle.ProductoId,
                            TipoMovimiento = "Devolucion",
                            Cantidad = detalle.Cantidad,
                            StockAnterior = stockAnterior,
                            StockPosterior = producto.Stock,
                            Referencia = $"Cancelacion Venta #{ventaId}",
                            FechaMovimiento = DateTime.UtcNow,
                            UsuarioId = usuarioId,
                            Observacion = $"Reintegro de stock por Venta cancelada #{ventaId}"
                        };

                        _context.MovimientosInventario.Add(movimiento);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService.RegistrarAccionAsync("Inventario", "ReintegroVenta", $"Reintegro de stock completado por Venta #{ventaId} cancelada", usuarioId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(IEnumerable<MovimientoInventario> Items, int TotalCount)> ObtenerKardexAsync(int? productoId, int page = 1, int pageSize = 20)
        {
            var query = _context.MovimientosInventario
                .Include(m => m.Producto)
                .Include(m => m.Sucursal)
                .AsNoTracking()
                .AsQueryable();

            if (productoId.HasValue && productoId.Value > 0)
            {
                query = query.Where(m => m.ProductoId == productoId.Value);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(m => m.FechaMovimiento)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> AumentarStockAsync(int productoId, int cantidad, int? sucursalId = null, string? usuarioId = null, string? observacion = null)
        {
            if (cantidad <= 0) return false;
            await RegistrarMovimientoAsync(productoId, sucursalId, "Ingreso", cantidad, "Aumento Manual", usuarioId, observacion ?? "Aumento manual de stock");
            return true;
        }
    }
}
