using System.ComponentModel.DataAnnotations;

namespace MiniMarketApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Contrasena { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NombreCompleto { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
