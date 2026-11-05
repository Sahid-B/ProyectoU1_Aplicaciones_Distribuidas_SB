using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Models;

namespace MiniMarketApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<TransaccionPago> TransaccionesPago { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Promocion> Promociones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransaccionPago>()
                .HasOne(t => t.Venta)
                .WithMany()
                .HasForeignKey(t => t.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(dc => dc.Compra)
                .WithMany(c => c.DetallesCompra)
                .HasForeignKey(dc => dc.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Sucursal)
                .WithMany()
                .HasForeignKey(m => m.SucursalId)
                .OnDelete(DeleteBehavior.SetNull);

            // ─── INDEXACIÓN DE ALTO RENDIMIENTO PARA POSTGRESQL (1,000,000 REGISTROS) ───
            modelBuilder.Entity<Producto>().HasIndex(p => new { p.Activo, p.CategoriaId });
            modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre);

            modelBuilder.Entity<Cliente>().HasIndex(c => new { c.Activo, c.Cedula });
            modelBuilder.Entity<Cliente>().HasIndex(c => c.Apellido);

            modelBuilder.Entity<Venta>().HasIndex(v => new { v.Activo, v.FechaVenta });
            modelBuilder.Entity<Venta>().HasIndex(v => v.Estado);
            modelBuilder.Entity<Venta>().HasIndex(v => v.ClienteId);

            modelBuilder.Entity<DetalleVenta>().HasIndex(dv => dv.VentaId);
            modelBuilder.Entity<DetalleVenta>().HasIndex(dv => dv.ProductoId);

            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => new { m.ProductoId, m.FechaMovimiento });
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.SucursalId);

            modelBuilder.Entity<TransaccionPago>().HasIndex(t => t.VentaId);
            modelBuilder.Entity<TransaccionPago>().HasIndex(t => t.ClientTransactionId);
            modelBuilder.Entity<TransaccionPago>().HasIndex(t => t.PayPalOrderId);

            modelBuilder.Entity<AuditLog>().HasIndex(a => a.Timestamp);

            modelBuilder.Entity<Promocion>().HasIndex(p => new { p.Activo, p.FechaInicio, p.FechaFin });
        }
    }
}
