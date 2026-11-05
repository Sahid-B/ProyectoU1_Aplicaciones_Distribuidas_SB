using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;
using MiniMarketApp.Models;

namespace MiniMarketApp.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAccionAsync(string entityName, string action, string details, string? userId = null)
        {
            try
            {
                var audit = new AuditLog
                {
                    EntityName = entityName,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                };

                _context.AuditLogs.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silently swallow log save failures to avoid breaking business operations
            }
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> ObtenerAuditoriasAsync(string? busqueda, int page = 1, int pageSize = 20)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(a => 
                    EF.Functions.ILike(a.EntityName, $"%{busqueda}%") ||
                    EF.Functions.ILike(a.Action, $"%{busqueda}%") ||
                    EF.Functions.ILike(a.Details, $"%{busqueda}%"));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
