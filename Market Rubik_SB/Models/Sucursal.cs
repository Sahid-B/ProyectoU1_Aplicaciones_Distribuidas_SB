using System.ComponentModel.DataAnnotations;

namespace MiniMarketApp.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200)]
        public string Direccion { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
