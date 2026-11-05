using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniMarketApp.Services
{
    public class PredictionResultDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int DemandaEstimadaMes { get; set; }
        public string NivelRiesgo { get; set; } = "Bajo"; // Bajo, Medio, Alto
        public string RecomendacionReabastecimiento { get; set; } = string.Empty;
        public double ConfianzaModelo { get; set; } = 0.95;
    }

    public interface IAIService
    {
        Task<PredictionResultDto> PredecirDemandaProductoAsync(int productoId);
        Task<IEnumerable<PredictionResultDto>> PredecirProductosCriticosAsync();
        Task<string> ConsultarAsistenteIAAsync(string pregunta);
        Task<string> ConsultarAsistenteClienteAsync(string pregunta);
        Task<string> ConsultarAsistenteAdminAsync(string pregunta);
        Task<bool> HealthCheckAsync();
    }
}
