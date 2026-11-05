using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;

namespace MiniMarketApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PromocionesController : Controller
    {
        private readonly AppDbContext _context;

        public PromocionesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Promociones.Include(p => p.Categoria).Include(p => p.Producto);
            return View(await appDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            var categorias = _context.Categorias.Where(c => c.Activo).ToList();
            var groupedCategorias = categorias
                .GroupBy(c => c.Nombre.Split(' ')[0])
                .Select(g => new { Id = g.First().Id, Nombre = g.Key })
                .OrderBy(g => g.Nombre)
                .ToList();

            ViewData["CategoriaId"] = new SelectList(groupedCategorias, "Id", "Nombre");
            ViewData["ProductoId"] = new SelectList(_context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,TipoDescuento,ValorDescuento,FechaInicio,FechaFin,CategoriaId,ProductoId,CantidadMinima,Activo")] Promocion promocion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promocion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var categorias = _context.Categorias.Where(c => c.Activo).ToList();
            var groupedCategorias = categorias
                .GroupBy(c => c.Nombre.Split(' ')[0])
                .Select(g => new { Id = g.First().Id, Nombre = g.Key })
                .OrderBy(g => g.Nombre)
                .ToList();
            ViewData["CategoriaId"] = new SelectList(groupedCategorias, "Id", "Nombre", promocion.CategoriaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre), "Id", "Nombre", promocion.ProductoId);
            return View(promocion);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var promocion = await _context.Promociones.FindAsync(id);
            if (promocion == null) return NotFound();
            
            var categorias = _context.Categorias.Where(c => c.Activo).ToList();
            var groupedCategorias = categorias
                .GroupBy(c => c.Nombre.Split(' ')[0])
                .Select(g => new { Id = g.First().Id, Nombre = g.Key })
                .OrderBy(g => g.Nombre)
                .ToList();
            ViewData["CategoriaId"] = new SelectList(groupedCategorias, "Id", "Nombre", promocion.CategoriaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre), "Id", "Nombre", promocion.ProductoId);
            return View(promocion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,TipoDescuento,ValorDescuento,FechaInicio,FechaFin,CategoriaId,ProductoId,CantidadMinima,Activo")] Promocion promocion)
        {
            if (id != promocion.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promocion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromocionExists(promocion.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var categorias = _context.Categorias.Where(c => c.Activo).ToList();
            var groupedCategorias = categorias
                .GroupBy(c => c.Nombre.Split(' ')[0])
                .Select(g => new { Id = g.First().Id, Nombre = g.Key })
                .OrderBy(g => g.Nombre)
                .ToList();
            ViewData["CategoriaId"] = new SelectList(groupedCategorias, "Id", "Nombre", promocion.CategoriaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre), "Id", "Nombre", promocion.ProductoId);
            return View(promocion);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var promocion = await _context.Promociones
                .Include(p => p.Categoria)
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promocion == null) return NotFound();

            return View(promocion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promocion = await _context.Promociones.FindAsync(id);
            if (promocion != null)
            {
                promocion.Activo = false;
                _context.Promociones.Update(promocion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PromocionExists(int id)
        {
            return _context.Promociones.Any(e => e.Id == id);
        }
    }
}
