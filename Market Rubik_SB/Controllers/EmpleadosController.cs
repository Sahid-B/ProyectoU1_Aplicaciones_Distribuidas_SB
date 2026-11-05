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
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            int tamañoPagina = 20;

            var consulta = _context.Empleados
                .AsNoTracking()
                .Where(e => e.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(e => EF.Functions.ILike(e.Nombre, $"%{buscar}%") ||
                                               EF.Functions.ILike(e.Apellido, $"%{buscar}%") ||
                                               (e.Cedula != null && EF.Functions.ILike(e.Cedula, $"%{buscar}%")));
            }

            consulta = consulta.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre);

            int totalRegistros = await consulta.CountAsync();

            var empleados = await consulta
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
            return View(empleados);
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Cedula,Cargo,Telefono,Salario")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                empleado.Activo = true;
                empleado.FechaCreacion = DateTime.UtcNow;
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Cedula,Cargo,Telefono,Salario,Activo,FechaCreacion")] Empleado empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    empleado.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                empleado.Activo = false;
                empleado.FechaEliminacion = DateTime.UtcNow;
                _context.Update(empleado);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}
