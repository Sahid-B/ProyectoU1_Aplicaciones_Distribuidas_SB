using System.Threading.Tasks;

namespace MiniMarketApp.Services.Payments
{
    public class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? CaptureId { get; set; }
        public string Status { get; set; } = "Pending";
        public string RawResponse { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public interface IPaymentGateway
    {
        string GatewayName { get; }
        Task<PaymentGatewayResult> CreatePaymentAsync(decimal total, string reference, string clientTransactionId);
        Task<PaymentGatewayResult> CapturePaymentAsync(string transactionOrToken);
    }
}
