using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniMarketApp.Services;

namespace MiniMarketApp.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        // ─── CHATBOT ACCESIBLE PARA CLIENTES (RECETAS Y PRESUPUESTO) ───
        [HttpGet]
        public async Task<IActionResult> ChatbotCliente()
        {
            ViewBag.IsAiOnline = await _aiService.HealthCheckAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConsultarCliente([FromBody] ChatQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return Json(new { response = "Por favor ingrese una consulta sobre recetas o presupuestos." });
            }

            var respuesta = await _aiService.ConsultarAsistenteClienteAsync(request.Prompt);
            return Json(new { response = respuesta });
        }

        // ─── SECCIÓN EXCLUSIVA DE ADMINISTRACIÓN ───
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAiOnline = await _aiService.HealthCheckAsync();
            var productosCriticos = await _aiService.PredecirProductosCriticosAsync();
            return View(productosCriticos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> PredecirProducto(int id)
        {
            var resultado = await _aiService.PredecirDemandaProductoAsync(id);
            return Json(resultado);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Consultar([FromBody] ChatQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return Json(new { response = "Por favor ingrese una pregunta válida." });
            }

            var respuesta = await _aiService.ConsultarAsistenteAdminAsync(request.Prompt);
            return Json(new { response = respuesta });
        }
    }

    public class ChatQueryRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
