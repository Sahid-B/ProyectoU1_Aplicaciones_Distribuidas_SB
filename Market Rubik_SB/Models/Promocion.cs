using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class Promocion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        // Ej: "Porcentaje", "MontoFijo"
        public string TipoDescuento { get; set; } = string.Empty; 

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDescuento { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int? ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int CantidadMinima { get; set; } = 1;

        public bool Activo { get; set; } = true;
    }
}
