using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;
using MiniMarketApp.Services.Payments;

namespace MiniMarketApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly PayPhoneApiLinkService _payPhoneService;
        private readonly PayPalService _payPalService;
        private readonly IInventoryService _inventoryService;
        private readonly IAuditService _auditService;

        public PaymentService(
            AppDbContext context,
            PayPhoneApiLinkService payPhoneService,
            PayPalService payPalService,
            IInventoryService inventoryService,
            IAuditService auditService)
        {
            _context = context;
            _payPhoneService = payPhoneService;
            _payPalService = payPalService;
            _inventoryService = inventoryService;
            _auditService = auditService;
        }

        public async Task<TransaccionPago> IniciarPagoAsync(int ventaId, string provider)
        {
            var venta = await _context.Ventas
                .Include(v => v.DetallesVenta)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
            {
                throw new KeyNotFoundException($"Venta #{ventaId} no encontrada.");
            }

            if (venta.Total < 0.01m)
            {
                throw new InvalidOperationException("El monto de la venta debe ser mayor a 0.");
            }

            string clientTransactionId = DateTime.Now.ToString("yyMMddHHmmssfff")[..15];
            string reference = $"Orden Market Rubik #{venta.Id}";

            var payment = new TransaccionPago
            {
                VentaId = venta.Id,
                ClientTransactionId = clientTransactionId,
                Provider = provider,
                AmountInCents = (int)Math.Round(venta.Total * 100, MidpointRounding.AwayFromZero),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            if (provider.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _payPalService.CreateOrderAsync(venta.Total, reference);
                payment.PayPalOrderId = result.OrderId;
                payment.PayPalApprovalUrl = result.ApprovalUrl;
                payment.GatewayResponse = result.RawResponse;
                payment.ClientTransactionId = result.OrderId;
            }
            else
            {
                string link = await _payPhoneService.CreatePaymentLinkAsync(venta.Total, clientTransactionId, reference);
                payment.PayphonePaymentUrl = link;
                payment.Provider = "PayPhone";
            }

            _context.TransaccionesPago.Add(payment);
            await _context.SaveChangesAsync();

            await _auditService.RegistrarAccionAsync("Pagos", "IniciarPago", $"Pago iniciado con pasarela {provider} para Venta #{ventaId}. Monto: ${venta.Total}");

            return payment;
        }

        public async Task<TransaccionPago?> ConfirmarPagoPayPalAsync(string token)
        {
            var payment = await _context.TransaccionesPago
                .Include(p => p.Venta)
                .ThenInclude(v => v.DetallesVenta)
                .FirstOrDefaultAsync(p => p.Provider == "PayPal" && p.PayPalOrderId == token);

            if (payment == null) return null;

            if (payment.Status == "Paid") return payment;

            var capture = await _payPalService.CaptureOrderAsync(token);

            payment.PayPalCaptureId = capture.CaptureId;
            payment.GatewayResponse = capture.RawResponse;
            payment.ConfirmedAt = DateTime.UtcNow;

            if (capture.Status == "COMPLETED")
            {
                payment.Status = "Paid";
                payment.Venta.Estado = "Completada";

                await _inventoryService.ProcesarVentaStockAsync(payment.VentaId, payment.Venta.DetallesVenta);
                await _auditService.RegistrarAccionAsync("Pagos", "ConfirmarPago", $"Pago completado exitosamente vía PayPal para Venta #{payment.VentaId}");
            }
            else
            {
                payment.Status = capture.Status;
            }

            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task CancelarPagoPayPalAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            var payment = await _context.TransaccionesPago
                .FirstOrDefaultAsync(p => p.Provider == "PayPal" && p.PayPalOrderId == token);

            if (payment != null && payment.Status == "Pending")
            {
                payment.Status = "Canceled";
                await _context.SaveChangesAsync();
                await _auditService.RegistrarAccionAsync("Pagos", "CancelarPago", $"Pago cancelado por usuario en PayPal para Transacción ID #{payment.Id}");
            }
        }

        public async Task MarcarComoPagadoDemostracionAsync(int transaccionId, string? usuarioId = null)
        {
            var payment = await _context.TransaccionesPago
                .Include(p => p.Venta)
                .ThenInclude(v => v.DetallesVenta)
                .FirstOrDefaultAsync(p => p.Id == transaccionId);

            if (payment == null) return;

            if (payment.Status != "Paid")
            {
                payment.Status = "Paid";
                payment.ConfirmedAt = DateTime.UtcNow;
                payment.Venta.Estado = "Completada";

                await _inventoryService.ProcesarVentaStockAsync(payment.VentaId, payment.Venta.DetallesVenta, usuarioId);
                await _context.SaveChangesAsync();
                await _auditService.RegistrarAccionAsync("Pagos", "DemostracionPagado", $"Pago marcado manualmente como PAGADO para Venta #{payment.VentaId}", usuarioId);
            }
        }

        public async Task<TransaccionPago?> ObtenerTransaccionPorIdAsync(int id)
        {
            return await _context.TransaccionesPago
                .Include(p => p.Venta)
                .ThenInclude(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
