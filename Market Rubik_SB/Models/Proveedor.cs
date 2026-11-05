using System.ComponentModel.DataAnnotations;

namespace MiniMarketApp.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contacto")]
        public string? Contacto { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [Display(Name = "Correo")]
        public string? Correo { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
