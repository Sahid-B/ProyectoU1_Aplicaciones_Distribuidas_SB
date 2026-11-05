using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int? SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimiento { get; set; } = string.Empty; // Compra, Venta, Devolucion, Ajuste, Transferencia

        [Required]
        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockPosterior { get; set; }

        [StringLength(100)]
        public string Referencia { get; set; } = string.Empty;

        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UsuarioId { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}
