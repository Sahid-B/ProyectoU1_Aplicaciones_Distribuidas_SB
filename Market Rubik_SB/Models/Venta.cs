using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "Número de Venta")]
        public string NumeroVenta { get; set; } = string.Empty;

        public string? CompradorId { get; set; }

        [Required]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Display(Name = "Fecha de Venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public int PuntosUtilizados { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DescuentoPuntos { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DescuentoPromociones { get; set; } = 0;

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
