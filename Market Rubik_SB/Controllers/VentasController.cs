using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using MiniMarketApp.ViewModels;

namespace MiniMarketApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            int tamañoPagina = 20;

            var consulta = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Where(v => v.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(v => EF.Functions.ILike(v.NumeroVenta, $"%{buscar}%") ||
                                               (v.Cliente != null && (EF.Functions.ILike(v.Cliente.Nombre, $"%{buscar}%") || EF.Functions.ILike(v.Cliente.Apellido, $"%{buscar}%"))));
            }

            consulta = consulta.OrderByDescending(v => v.FechaVenta);

            int totalRegistros = await consulta.CountAsync();

            var ventas = await consulta
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
            return View(ventas);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (venta == null) return NotFound();

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Activo), "Id", "Apellido");
            return View();
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NumeroVenta,ClienteId,FechaVenta,Subtotal,Iva,Total,Estado")] Venta venta)
        {
            if (ModelState.IsValid)
            {
                venta.Activo = true;
                venta.FechaCreacion = DateTime.UtcNow;
                _context.Add(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Activo), "Id", "Apellido", venta.ClienteId);
            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound();

            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Activo), "Id", "Apellido", venta.ClienteId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroVenta,ClienteId,FechaVenta,Subtotal,Iva,Total,Estado,Activo,FechaCreacion")] Venta venta)
        {
            if (id != venta.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    venta.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(venta);
                    _context.Entry(venta).Property(v => v.CompradorId).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Activo), "Id", "Apellido", venta.ClienteId);
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (venta == null) return NotFound();

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                venta.Activo = false;
                venta.FechaEliminacion = DateTime.UtcNow;
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}
