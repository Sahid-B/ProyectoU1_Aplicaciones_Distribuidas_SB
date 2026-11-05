using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMarketApp.Models
{
    public class TransaccionPago
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int VentaId { get; set; }
        
        [ForeignKey("VentaId")]
        public Venta Venta { get; set; } = null!;
        
        public string ClientTransactionId { get; set; } = string.Empty;
        public string? PayphonePaymentUrl { get; set; }
        public string? PayphoneTransactionId { get; set; }
        public int AmountInCents { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        
        public string Provider { get; set; } = "PayPhone";
        public string? PayPalOrderId { get; set; }
        public string? PayPalCaptureId { get; set; }
        public string? PayPalApprovalUrl { get; set; }
        public string? GatewayResponse { get; set; }
    }
}
