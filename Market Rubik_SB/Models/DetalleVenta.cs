using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
