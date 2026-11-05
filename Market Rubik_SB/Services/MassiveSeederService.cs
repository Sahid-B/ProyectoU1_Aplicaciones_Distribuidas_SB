using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;

namespace MiniMarketApp.Services
{
    public class MassiveSeederService
    {
        private readonly AppDbContext _context;

        public MassiveSeederService(AppDbContext context)
        {
            _context = context;
        }

        public async Task Seed1MillionAsync()
        {
            try
            {
                var totalActual = (await _context.Categorias.CountAsync()) +
                                  (await _context.Proveedores.CountAsync()) +
                                  (await _context.Sucursales.CountAsync()) +
                                  (await _context.Empleados.CountAsync()) +
                                  (await _context.Clientes.CountAsync()) +
                                  (await _context.Productos.CountAsync()) +
                                  (await _context.Compras.CountAsync()) +
                                  (await _context.DetallesCompra.CountAsync()) +
                                  (await _context.Ventas.CountAsync()) +
                                  (await _context.DetallesVenta.CountAsync()) +
                                  (await _context.TransaccionesPago.CountAsync()) +
                                  (await _context.MovimientosInventario.CountAsync());

                if (totalActual >= 1000000) return;

                var sqlFile = Path.Combine(Directory.GetCurrentDirectory(), "seed_1million.sql");
                if (!File.Exists(sqlFile))
                {
                    sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed_1million.sql");
                }

                if (File.Exists(sqlFile))
                {
                    var sql = await File.ReadAllTextAsync(sqlFile);
                    _context.Database.SetCommandTimeout(300); // 5 minutos de tiempo de espera para 1M de registros
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Massive Seeder Concurrent Exec Handled]: {ex.Message}");
            }
        }
    }
}
