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
using MiniMarketApp.Services;
using MiniMarketApp.ViewModels;

namespace MiniMarketApp.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAuditService _auditService;

        public ProductosController(AppDbContext context, IInventoryService inventoryService, IAuditService auditService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _auditService = auditService;
        }

        // GET: Productos (Paginación física optimizada para 1.000.000 de registros)
        public async Task<IActionResult> Index(int pagina = 1, string buscar = "")
        {
            int tamañoPagina = 20;

            var consulta = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(p => EF.Functions.ILike(p.Nombre, $"%{buscar}%"));
            }

            consulta = consulta.OrderBy(p => p.Nombre);

            int totalRegistros = await consulta.CountAsync();

            var productos = await consulta
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
            return View(productos);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // GET: Productos/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Activo), "Id", "Nombre");
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores.Where(p => p.Activo), "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Stock,CategoriaId,ProveedorId")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                producto.Activo = true;
                producto.FechaCreacion = DateTime.UtcNow;
                _context.Add(producto);
                await _context.SaveChangesAsync();

                if (producto.Stock > 0)
                {
                    await _inventoryService.RegistrarMovimientoAsync(
                        producto.Id, null, "Ingreso", producto.Stock, "Stock Inicial", User.Identity?.Name, "Creación de producto");
                }

                await _auditService.RegistrarAccionAsync("Productos", "Crear", $"Producto creado: {producto.Nombre}", User.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Activo), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores.Where(p => p.Activo), "Id", "Nombre", producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Activo), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores.Where(p => p.Activo), "Id", "Nombre", producto.ProveedorId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,CategoriaId,ProveedorId,Activo,FechaCreacion")] Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    producto.FechaActualizacion = DateTime.UtcNow;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    await _auditService.RegistrarAccionAsync("Productos", "Editar", $"Producto editado ID #{producto.Id}: {producto.Nombre}", User.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Activo), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores.Where(p => p.Activo), "Id", "Nombre", producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5 (Eliminación Lógica)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.Activo = false;
                producto.FechaEliminacion = DateTime.UtcNow;
                _context.Update(producto);
                await _context.SaveChangesAsync();
                await _auditService.RegistrarAccionAsync("Productos", "EliminacionLogica", $"Eliminación lógica de producto #{id}: {producto.Nombre}", User.Identity?.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
