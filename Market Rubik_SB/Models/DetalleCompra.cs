using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }

        [Required]
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
