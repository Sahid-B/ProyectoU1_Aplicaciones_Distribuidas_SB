using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.ViewModels;

namespace MiniMarketApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }



        public async Task<IActionResult> TotalVentas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Ventas.AsNoTracking().Where(v => v.Activo);

            if (fechaInicio.HasValue) query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(v => v.FechaVenta <= fechaFin.Value);

            var count = await query.CountAsync();

            var model = new TotalVentasDto
            {
                NumeroVentas = count,
                TotalVentas = count > 0 ? await query.SumAsync(v => v.Total) : 0,
                PromedioPorVenta = count > 0 ? await query.AverageAsync(v => v.Total) : 0
            };

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> VentasPorMes()
        {
            await EnsureViewsExistAsync();
            var model = await _context.Database
                .SqlQueryRaw<VentasPorMesDto>("SELECT \"Anio\", \"Mes\", \"Total\", \"Cantidad\" FROM \"vw_VentasPorMes\" ORDER BY \"Anio\" DESC, \"Mes\" DESC")
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> TopProductos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.DetallesVenta
                .AsNoTracking()
                .Include(d => d.Producto)
                .Where(d => d.Venta != null && d.Venta.Activo);

            if (fechaInicio.HasValue) query = query.Where(d => d.Venta!.FechaVenta >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(d => d.Venta!.FechaVenta <= fechaFin.Value);

            var model = await query
                .GroupBy(d => d.Producto!.Nombre)
                .Select(g => new TopProductoDto
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(g => g.CantidadVendida)
                .Take(10)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> ClientesMasCompras(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Where(v => v.Activo);

            if (fechaInicio.HasValue) query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(v => v.FechaVenta <= fechaFin.Value);

            var model = await query
                .GroupBy(v => v.Cliente!.Nombre + " " + v.Cliente.Apellido)
                .Select(g => new ClienteMasComprasDto
                {
                    Cliente = g.Key,
                    TotalComprado = g.Sum(v => v.Total)
                })
                .OrderByDescending(g => g.TotalComprado)
                .Take(10)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> ProductosBajoStock(int pagina = 1)
        {
            int tamañoPagina = 20;

            var model = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Stock < 10 && p.Activo)
                .OrderBy(p => p.Stock)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(p => new ProductoStockDto
                {
                    Nombre = p.Nombre,
                    Stock = p.Stock
                })
                .ToListAsync();

            ViewBag.Pagina = pagina;
            return View(model);
        }

        public async Task<IActionResult> VentasPorEstado(DateTime? fechaInicio, DateTime? fechaFin, string? estado)
        {
            var query = _context.Ventas
                .AsNoTracking()
                .Where(v => v.Activo);

            if (fechaInicio.HasValue) query = query.Where(v => v.FechaVenta >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(v => v.FechaVenta <= fechaFin.Value);
            if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado == estado);

            var model = await query
                .GroupBy(v => v.Estado)
                .Select(g => new VentasPorEstadoDto
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Estado = estado;
            return View(model);
        }

        public async Task<IActionResult> ProveedoresMasProductos()
        {
            var model = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Proveedor)
                .Where(p => p.Activo && p.Proveedor != null)
                .GroupBy(p => p.Proveedor!.Nombre)
                .Select(g => new ProveedorMasProductosDto
                {
                    Proveedor = g.Key,
                    CantidadProductos = g.Count()
                })
                .OrderByDescending(g => g.CantidadProductos)
                .Take(10)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> CategoriasMasVendidas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.DetallesVenta
                .AsNoTracking()
                .Include(d => d.Producto)
                .ThenInclude(p => p!.Categoria)
                .Where(d => d.Venta != null && d.Venta.Activo && d.Producto != null && d.Producto.Categoria != null);

            if (fechaInicio.HasValue) query = query.Where(d => d.Venta!.FechaVenta >= fechaInicio.Value);
            if (fechaFin.HasValue) query = query.Where(d => d.Venta!.FechaVenta <= fechaFin.Value);

            var model = await query
                .GroupBy(d => d.Producto!.Categoria!.Nombre)
                .Select(g => new CategoriaMasVendidaDto
                {
                    Categoria = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(g => g.CantidadVendida)
                .Take(10)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> PromedioVentasPorCliente()
        {
            await EnsureViewsExistAsync();
            var model = await _context.Database
                .SqlQueryRaw<PromedioVentaClienteDto>("SELECT \"Cliente\", \"PromedioVentas\" FROM \"vw_PromedioVentasCliente\"")
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> ResumenGeneral()
        {
            var unMesAtras = DateTime.UtcNow.AddMonths(-1);
            var ventasUltimoMes = _context.Ventas.AsNoTracking().Where(v => v.FechaVenta >= unMesAtras && v.Activo);

            var model = new ResumenGeneralDto
            {
                ClientesActivos = await _context.Clientes.AsNoTracking().CountAsync(c => c.Activo),
                ClientesInactivos = await _context.Clientes.AsNoTracking().CountAsync(c => !c.Activo),
                ProductosActivos = await _context.Productos.AsNoTracking().CountAsync(p => p.Activo),
                ProductosInactivos = await _context.Productos.AsNoTracking().CountAsync(p => !p.Activo),
                VentasActivas = await _context.Ventas.AsNoTracking().CountAsync(v => v.Activo),
                VentasInactivas = await _context.Ventas.AsNoTracking().CountAsync(v => !v.Activo),
                TotalVentasUltimoMes = await ventasUltimoMes.CountAsync(),
                TotalIngresosUltimoMes = await ventasUltimoMes.SumAsync(v => v.Total)
            };

            return View(model);
        }
        public async Task<IActionResult> PagosPorPasarela(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.TransaccionesPago.AsNoTracking();

            if (fechaInicio.HasValue) query = query.Where(t => t.CreatedAt >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(t => t.CreatedAt <= fechaFin.Value.ToUniversalTime());

            var model = await query
                .GroupBy(t => t.Provider)
                .Select(g => new PagosPorPasarelaDto
                {
                    Pasarela = string.IsNullOrEmpty(g.Key) ? "Desconocida" : g.Key,
                    CantidadPagos = g.Count(),
                    TotalRecaudado = g.Sum(t => (decimal)t.AmountInCents) / 100m
                })
                .OrderByDescending(g => g.TotalRecaudado)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> PagosPorEstado(DateTime? fechaInicio, DateTime? fechaFin, string? estado)
        {
            var query = _context.TransaccionesPago.AsNoTracking();

            if (fechaInicio.HasValue) query = query.Where(t => t.CreatedAt >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(t => t.CreatedAt <= fechaFin.Value.ToUniversalTime());
            if (!string.IsNullOrEmpty(estado)) query = query.Where(t => t.Status == estado);

            var model = await query
                .GroupBy(t => t.Status)
                .Select(g => new PagosPorEstadoDto
                {
                    Estado = string.IsNullOrEmpty(g.Key) ? "Desconocido" : g.Key,
                    CantidadPagos = g.Count()
                })
                .OrderByDescending(g => g.CantidadPagos)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Estado = estado;
            return View(model);
        }

        public async Task<IActionResult> MovimientosPorTipo(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.MovimientosInventario.AsNoTracking();

            if (fechaInicio.HasValue) query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(m => m.FechaMovimiento <= fechaFin.Value.ToUniversalTime());

            var model = await query
                .GroupBy(m => m.TipoMovimiento)
                .Select(g => new MovimientosPorTipoDto
                {
                    TipoMovimiento = g.Key,
                    CantidadMovimientos = g.Count()
                })
                .OrderByDescending(g => g.CantidadMovimientos)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> ComprasPorProveedor(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Compras
                .AsNoTracking()
                .Include(c => c.Proveedor)
                .Where(c => c.Activo);

            if (fechaInicio.HasValue) query = query.Where(c => c.FechaCompra >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(c => c.FechaCompra <= fechaFin.Value.ToUniversalTime());

            var model = await query
                .GroupBy(c => c.Proveedor != null ? c.Proveedor.Nombre : "Desconocido")
                .Select(g => new ComprasPorProveedorDto
                {
                    Proveedor = g.Key,
                    CantidadCompras = g.Count(),
                    TotalInvertido = g.Sum(c => c.Total)
                })
                .OrderByDescending(g => g.TotalInvertido)
                .Take(15)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            return View(model);
        }

        public async Task<IActionResult> AuditoriaPorAccion(DateTime? fechaInicio, DateTime? fechaFin, string? accion)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (fechaInicio.HasValue) query = query.Where(a => a.Timestamp >= fechaInicio.Value.ToUniversalTime());
            if (fechaFin.HasValue) query = query.Where(a => a.Timestamp <= fechaFin.Value.ToUniversalTime());
            if (!string.IsNullOrEmpty(accion)) query = query.Where(a => a.Action == accion);

            var model = await query
                .GroupBy(a => a.Action)
                .Select(g => new AuditoriaPorAccionDto
                {
                    Accion = g.Key,
                    CantidadEventos = g.Count()
                })
                .OrderByDescending(g => g.CantidadEventos)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Accion = accion;
            return View(model);
        }

        private async Task EnsureViewsExistAsync()
        {
            try
            {
                var sqlViews = @"
                    CREATE OR REPLACE VIEW ""vw_VentasPorMes"" AS
                    SELECT
                        EXTRACT(YEAR FROM ""FechaVenta"")::INT AS ""Anio"",
                        EXTRACT(MONTH FROM ""FechaVenta"")::INT AS ""Mes"",
                        SUM(""Total"") AS ""Total"",
                        COUNT(*)::INT AS ""Cantidad""
                    FROM ""Ventas""
                    WHERE ""Activo"" = true
                    GROUP BY EXTRACT(YEAR FROM ""FechaVenta""), EXTRACT(MONTH FROM ""FechaVenta"");

                    CREATE OR REPLACE VIEW ""vw_PromedioVentasCliente"" AS
                    SELECT
                        c.""Nombre"" || ' ' || c.""Apellido"" AS ""Cliente"",
                        AVG(v.""Total"") AS ""PromedioVentas""
                    FROM ""Ventas"" v
                    JOIN ""Clientes"" c ON v.""ClienteId"" = c.""Id""
                    WHERE v.""Activo"" = true
                    GROUP BY c.""Id"", c.""Nombre"", c.""Apellido""
                    ORDER BY ""PromedioVentas"" DESC
                    LIMIT 20;
                ";
                await _context.Database.ExecuteSqlRawAsync(sqlViews);
            }
            catch (Exception)
            {
                // Ignorar excepciones de concurrencia al crear las vistas
            }
        }
    }
}
