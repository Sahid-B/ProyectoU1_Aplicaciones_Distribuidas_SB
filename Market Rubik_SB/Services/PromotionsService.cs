using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public class PromotionsService : IPromotionsService
    {
        private readonly AppDbContext _context;
        // Ratio: 100 puntos = $1
        private const decimal ValorPunto = 0.01m;
        // Ganancia: 1 punto por cada $1 gastado
        private const decimal PuntosPorDolar = 1m;

        public PromotionsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalcularDescuentoPromocionesAsync(List<DetalleVenta> detalles)
        {
            var ahora = DateTime.UtcNow;
            var promocionesActivas = await _context.Promociones
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.FechaInicio <= ahora && p.FechaFin >= ahora)
                .ToListAsync();

            if (!promocionesActivas.Any() || detalles == null || !detalles.Any())
                return 0;

            decimal descuentoTotal = 0;

            // Extraer IDs de productos en el carrito
            var productoIds = detalles.Select(d => d.ProductoId).Distinct().ToList();
            
            // Cargar los productos reales para saber sus categorías si no vinieron incluidos
            var productosDb = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => productoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var promo in promocionesActivas)
            {
                Console.WriteLine($"[PROMO] Evaluando promo: {promo.Nombre}, CategoriaId: {promo.CategoriaId}, Categoria.Nombre: {promo.Categoria?.Nombre}");
                var detallesAplicables = detalles.Where(d => 
                {
                    productosDb.TryGetValue(d.ProductoId, out var producto);
                    Console.WriteLine($"[PROMO]   -> Producto: {producto?.Nombre}, CategoriaId: {producto?.CategoriaId}, Categoria.Nombre: {producto?.Categoria?.Nombre}");
                    
                    bool matchCategoria = promo.CategoriaId == null;
                    if (!matchCategoria && producto != null && producto.Categoria != null && promo.Categoria != null)
                    {
                        var promoWord = promo.Categoria.Nombre.Trim().ToLower().Split(' ')[0];
                        var prodWord = producto.Categoria.Nombre.Trim().ToLower().Split(' ')[0];
                        Console.WriteLine($"[PROMO]      -> Comparando primera palabra: promo='{promoWord}', prod='{prodWord}'");
                        
                        if (producto.CategoriaId == promo.CategoriaId) 
                            matchCategoria = true;
                        else if (prodWord == promoWord) 
                            matchCategoria = true;
                        else if (producto.Categoria.Nombre.ToLower().Contains(promoWord) || promo.Categoria.Nombre.ToLower().Contains(prodWord))
                            matchCategoria = true;
                    }

                    bool matchProducto = promo.ProductoId == null || d.ProductoId == promo.ProductoId;
                    Console.WriteLine($"[PROMO]   -> matchCategoria: {matchCategoria}, matchProducto: {matchProducto}");
                    return matchCategoria && matchProducto;
                }).ToList();

                var cantidadAplicable = detallesAplicables.Sum(d => d.Cantidad);
                Console.WriteLine($"[PROMO] Cantidad aplicable: {cantidadAplicable}, minima: {promo.CantidadMinima}");

                if (cantidadAplicable >= promo.CantidadMinima)
                {
                    decimal subtotalAplicable = detallesAplicables.Sum(d => d.Cantidad * d.PrecioUnitario);

                    if (promo.TipoDescuento == "Porcentaje")
                    {
                        descuentoTotal += subtotalAplicable * (promo.ValorDescuento / 100m);
                    }
                    else if (promo.TipoDescuento == "MontoFijo")
                    {
                        descuentoTotal += promo.ValorDescuento;
                    }
                }
            }

            return Math.Round(descuentoTotal, 2);
        }

        public decimal CalcularDescuentoPorPuntos(int puntos, decimal totalVenta)
        {
            decimal descuentoPotencial = puntos * ValorPunto;
            return Math.Min(descuentoPotencial, totalVenta);
        }

        public int CalcularPuntosGanados(decimal totalVenta)
        {
            return (int)Math.Floor(totalVenta * PuntosPorDolar);
        }
    }
}
