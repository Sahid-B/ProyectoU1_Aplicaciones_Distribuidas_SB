using System;
using System.ComponentModel.DataAnnotations;

namespace MiniMarketApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UserId { get; set; }
    }
}
