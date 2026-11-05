using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using MiniMarketApp.ViewModels;
namespace MiniMarketApp.Controllers
{
    public class DetallesVentaController : Controller
    {
        private readonly AppDbContext _context;

        public DetallesVentaController(AppDbContext context)
        {
            _context = context;
        }

        private bool SesionActiva()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));
        }

        // GET: DetallesVenta
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");

            int tamañoPagina = 20;

            var consulta = _context.DetallesVenta
                .AsNoTracking()
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                consulta = consulta.Where(d => d.Producto.Nombre.Contains(buscar) || d.Venta.NumeroVenta.Contains(buscar));

            consulta = consulta.OrderByDescending(d => d.FechaCreacion);

            int totalRegistros = await consulta.CountAsync();

            var detalles = await consulta
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            ViewBag.Paginacion = new PaginacionViewModel
            {
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamañoPagina),
                TotalRegistros = totalRegistros,
                TamañoPagina = tamañoPagina
            };

            ViewBag.Buscar = buscar;
            return View(detalles);
        }

        // GET: DetallesVenta/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        // GET: DetallesVenta/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Rol") != "Admin") return RedirectToAction("Index");
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre");
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id");
            return View();
        }

        // POST: DetallesVenta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VentaId,ProductoId,Cantidad,PrecioUnitario,FechaCreacion")] DetalleVenta detalleVenta)
        {
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            if (ModelState.IsValid)
            {
                _context.Add(detalleVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetallesVenta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin") return RedirectToAction("Index");
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetallesVenta.FindAsync(id);
            if (detalleVenta == null)
            {
                return NotFound();
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // POST: DetallesVenta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VentaId,ProductoId,Cantidad,PrecioUnitario,FechaCreacion")] DetalleVenta detalleVenta)
        {
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            if (id != detalleVenta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleVenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleVentaExists(detalleVenta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetallesVenta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Rol") != "Admin") return RedirectToAction("Index");
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        // POST: DetallesVenta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SesionActiva()) return RedirectToAction("Login", "Auth");
            var detalleVenta = await _context.DetallesVenta.FindAsync(id);
            if (detalleVenta != null)
            {
                _context.DetallesVenta.Remove(detalleVenta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleVentaExists(int id)
        {
            return _context.DetallesVenta.Any(e => e.Id == id);
        }
    }
}


