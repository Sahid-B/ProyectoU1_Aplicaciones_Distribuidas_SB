using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniMarketApp.Services;

namespace MiniMarketApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> CreateLink(int orderId)
        {
            try
            {
                var payment = await _paymentService.IniciarPagoAsync(orderId, "PayPhone");
                return RedirectToAction(nameof(Details), new { id = payment.Id });
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> CreatePayPalOrder(int orderId)
        {
            try
            {
                var payment = await _paymentService.IniciarPagoAsync(orderId, "PayPal");
                return Redirect(payment.PayPalApprovalUrl ?? "/Home");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayPalOrderApi(int orderId)
        {
            try
            {
                var payment = await _paymentService.IniciarPagoAsync(orderId, "PayPal");
                return Json(new { id = payment.PayPalOrderId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SuccessApi(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();
            var payment = await _paymentService.ConfirmarPagoPayPalAsync(token);
            if (payment == null) return NotFound();
            return Json(new { paymentId = payment.Id });
        }

        public async Task<IActionResult> Success(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("PayPal no devolvió el token de orden.");
            }

            var payment = await _paymentService.ConfirmarPagoPayPalAsync(token);
            if (payment == null) return NotFound();

            return RedirectToAction(nameof(Details), new { id = payment.Id });
        }

        public async Task<IActionResult> Cancel(string token)
        {
            await _paymentService.CancelarPagoPayPalAsync(token);
            TempData["Error"] = "El pago con PayPal fue cancelado.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.ObtenerTransaccionPorIdAsync(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            await _paymentService.MarcarComoPagadoDemostracionAsync(id, User.Identity?.Name);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
