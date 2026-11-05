using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;

namespace MiniMarketApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Obtener los últimos 4 productos como "Noticias/Novedades"
        var ultimosProductos = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.Id)
            .Take(4)
            .ToListAsync();

        // Obtener Promociones Activas
        var ahora = DateTime.UtcNow;
        var promocionesActivas = await _context.Promociones
            .Where(p => p.Activo && p.FechaInicio <= ahora && p.FechaFin >= ahora)
            .ToListAsync();
        ViewBag.Promociones = promocionesActivas;

        // Algunas estadísticas básicas para el dashboard
        ViewBag.TotalClientes = await _context.Clientes.CountAsync(c => c.Activo);
        ViewBag.TotalVentas = await _context.Ventas.CountAsync(v => v.Activo);

        return View(ultimosProductos);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
