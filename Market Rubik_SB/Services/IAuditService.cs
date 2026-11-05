using System.Collections.Generic;
using System.Threading.Tasks;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public interface IAuditService
    {
        Task RegistrarAccionAsync(string entityName, string action, string details, string? userId = null);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> ObtenerAuditoriasAsync(string? busqueda, int page = 1, int pageSize = 20);
    }
}
