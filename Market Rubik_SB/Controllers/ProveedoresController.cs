using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using MiniMarketApp.ViewModels;

namespace MiniMarketApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProveedoresController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            int tamañoPagina = 20;

            var consulta = _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(p => EF.Functions.ILike(p.Nombre, $"%{buscar}%") ||
                                               (p.Contacto != null && EF.Functions.ILike(p.Contacto, $"%{buscar}%")));
            }

            consulta = consulta.OrderBy(p => p.Nombre);

            int totalRegistros = await consulta.CountAsync();

            var proveedores = await consulta
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
            return View(proveedores);
        }

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Contacto,Telefono,Correo")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                proveedor.Activo = true;
                proveedor.FechaCreacion = DateTime.UtcNow;
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Contacto,Telefono,Correo,Activo,FechaCreacion")] Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    proveedor.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                proveedor.Activo = false;
                proveedor.FechaEliminacion = DateTime.UtcNow;
                _context.Update(proveedor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.Id == id);
        }
    }
}
