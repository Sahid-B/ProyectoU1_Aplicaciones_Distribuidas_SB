using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class Compra
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroCompra { get; set; } = string.Empty;

        [Required]
        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public int? SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } = "Completada";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
    }
}
