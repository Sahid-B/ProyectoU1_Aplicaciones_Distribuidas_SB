using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using MiniMarketApp.Services;

namespace MiniMarketApp.Controllers
{
    [Authorize]
    public class TiendaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAuditService _auditService;
        private readonly IPromotionsService _promotionsService;
        private readonly Microsoft.Extensions.Options.IOptions<MiniMarketApp.Settings.PayPalSettings> _paypalSettings;

        public TiendaController(
            AppDbContext context, 
            IInventoryService inventoryService, 
            IAuditService auditService, 
            IPromotionsService promotionsService,
            Microsoft.Extensions.Options.IOptions<MiniMarketApp.Settings.PayPalSettings> paypalSettings)
        {
            _context = context;
            _inventoryService = inventoryService;
            _auditService = auditService;
            _promotionsService = promotionsService;
            _paypalSettings = paypalSettings;
        }

        // Obtener el carrito actual desde la sesión
        private List<CarritoItem> ObtenerCarrito()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoJson))
                return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
        }

        // Guardar el carrito en la sesión
        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", carritoJson);
        }

        // GET: Tienda
        public async Task<IActionResult> Index(string? search, string? categoria, int pagina = 1)
        {
            const int PageSize = 30;

            var query = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock > 0);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(searchLower) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(searchLower)));
            }

            // Filtrar por grupo de categoría (primera palabra del nombre de la categoría)
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria != null && p.Categoria.Nombre.StartsWith(categoria));
            }

            var totalProductos = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalProductos / PageSize);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, totalPaginas)));

            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Obtener grupos de categorías únicos (la primera palabra del nombre)
            var todasCategorias = await _context.Categorias
                .Where(c => c.Activo)
                .Select(c => c.Nombre)
                .ToListAsync();

            var gruposCategorias = todasCategorias
                .Select(n => n.Split(' ')[0])
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            ViewBag.GruposCategorias = gruposCategorias;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoria = categoria;
            ViewBag.CarritoItems = ObtenerCarrito().Sum(c => c.Cantidad);
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProductos = totalProductos;

            var promocionesActivas = await _context.Promociones
                .Where(p => p.Activo && p.FechaInicio <= DateTime.UtcNow && p.FechaFin >= DateTime.UtcNow)
                .ToListAsync();
            ViewBag.Promociones = promocionesActivas;

            return View(productos);
        }

        // POST: Tienda/AgregarAlCarrito
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AgregarAlCarrito(int productoId)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo || producto.Stock <= 0)
                return Json(new { success = false, message = "Producto no disponible" });

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == productoId);

            if (item != null)
            {
                if (item.Cantidad < producto.Stock)
                    item.Cantidad++;
                else
                    return Json(new { success = false, message = "Stock máximo alcanzado" });
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1
                });
            }

            GuardarCarrito(carrito);
            return Json(new { success = true, totalItems = carrito.Sum(c => c.Cantidad), productoNombre = producto.Nombre });
        }

        // GET: Tienda/Carrito
        public async Task<IActionResult> Carrito()
        {
            var carrito = ObtenerCarrito();
            
            var detallesVenta = carrito.Select(item => new DetalleVenta
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Precio
            }).ToList();
            
            decimal descuentoPromociones = await _promotionsService.CalcularDescuentoPromocionesAsync(detallesVenta);
            ViewBag.DescuentoPromociones = descuentoPromociones;

            return View(carrito);
        }

        // POST: Tienda/QuitarDelCarrito
        [HttpPost]
        public IActionResult QuitarDelCarrito(int productoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == productoId);
            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);
            }
            return RedirectToAction(nameof(Carrito));
        }

        // POST: Tienda/Pagar
        [HttpPost]
        public async Task<IActionResult> Pagar()
        {
            var carrito = ObtenerCarrito();
            if (!carrito.Any()) return RedirectToAction(nameof(Index));

            var userEmail = User.Identity?.Name ?? "cliente@marketrubik.com";
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre + "@marketrubik.com" == userEmail || c.Cedula == userEmail);
            if (cliente == null)
            {
                cliente = new Cliente { Nombre = User.Identity?.Name ?? "Cliente", Apellido = "Supermarket", Activo = true };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            var detallesVenta = carrito.Select(item => new DetalleVenta
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Precio
            }).ToList();

            decimal subtotal = carrito.Sum(c => c.Subtotal);
            
            // 1. Calcular descuentos por promociones
            decimal descuentoPromociones = await _promotionsService.CalcularDescuentoPromocionesAsync(detallesVenta);
            
            decimal baseImponible = Math.Max(0, subtotal - descuentoPromociones);
            decimal ivaCalculado = baseImponible * 0.15m;
            decimal totalFinal = baseImponible + ivaCalculado;

            // 2. Asignar puntos ganados al cliente
            int puntosGanados = _promotionsService.CalcularPuntosGanados(totalFinal);
            cliente.PuntosAcumulados += puntosGanados;
            _context.Clientes.Update(cliente);

            var venta = new Venta
            {
                CompradorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                NumeroVenta = "V-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ClienteId = cliente.Id,
                FechaVenta = DateTime.UtcNow,
                Subtotal = subtotal,
                DescuentoPromociones = descuentoPromociones,
                Iva = ivaCalculado,
                Total = totalFinal,
                Estado = "Pendiente",
                Activo = true,
                DetallesVenta = detallesVenta
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Limpiar carrito
            HttpContext.Session.Remove("Carrito");
            await _auditService.RegistrarAccionAsync("Tienda", "CrearOrden", $"Orden de compra #{venta.Id} creada por ${totalFinal}", userEmail);

            return RedirectToAction(nameof(Exito), new { id = venta.Id });
        }

        // GET: Tienda/Exito
        public IActionResult Exito(int id)
        {
            ViewBag.VentaId = id;
            ViewBag.PayPalClientId = _paypalSettings.Value.ClientId;
            return View();
        }

        // GET: Tienda/CancelarCompra/5
        public async Task<IActionResult> CancelarCompra(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null && venta.CompradorId == User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value && venta.CompradorId != null && (venta.Estado == "Pendiente" || venta.Estado == "Creado"))
            {
                venta.Estado = "Cancelada";
                _context.Ventas.Update(venta);
                await _context.SaveChangesAsync();
                await _auditService.RegistrarAccionAsync("Tienda", "CancelarOrden", $"Orden de compra #{id} cancelada por el usuario", User.Identity?.Name);
            }
            TempData["MensajeAlerta"] = "Compra Cancelada";
            return RedirectToAction(nameof(Index));
        }
    }
}
