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
    public class SucursalesController : Controller
    {
        private readonly AppDbContext _context;

        public SucursalesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Sucursales (Accesible para todos los usuarios)
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            int tamañoPagina = 20;

            var consulta = _context.Sucursales
                .AsNoTracking()
                .Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(c => EF.Functions.ILike(c.Nombre, $"%{buscar}%") ||
                                               (c.Direccion != null && EF.Functions.ILike(c.Direccion, $"%{buscar}%")));
            }

            consulta = consulta.OrderBy(c => c.Nombre);

            int totalRegistros = await consulta.CountAsync();

            var sucursales = await consulta
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
            return View(sucursales);
        }

        // GET: Sucursales/Details/5 (Accesible para todos los usuarios)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sucursal = await _context.Sucursales
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sucursal == null) return NotFound();

            return View(sucursal);
        }

        // GET: Sucursales/Create (Solo Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sucursales/Create (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Direccion,Telefono")] Sucursal sucursal)
        {
            if (ModelState.IsValid)
            {
                sucursal.Activo = true;
                sucursal.FechaCreacion = DateTime.UtcNow;
                _context.Add(sucursal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        // GET: Sucursales/Edit/5 (Solo Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal == null) return NotFound();

            return View(sucursal);
        }

        // POST: Sucursales/Edit/5 (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Direccion,Telefono,Activo,FechaCreacion")] Sucursal sucursal)
        {
            if (id != sucursal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    sucursal.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(sucursal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SucursalExists(sucursal.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sucursal);
        }

        // GET: Sucursales/Delete/5 (Solo Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sucursal = await _context.Sucursales
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sucursal == null) return NotFound();

            return View(sucursal);
        }

        // POST: Sucursales/Delete/5 (Solo Admin)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal != null)
            {
                sucursal.Activo = false;
                sucursal.FechaEliminacion = DateTime.UtcNow;
                _context.Update(sucursal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SucursalExists(int id)
        {
            return _context.Sucursales.Any(e => e.Id == id);
        }
    }
}
