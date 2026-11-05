using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
namespace MiniMarketApp.Realtime;

// PostgreSQL delivers notifications only after commit. Each pod addresses its own connections.
public class MarketEvents(IConfiguration config, IHubContext<MarketHub> hub, ILogger<MarketEvents> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            try
            {
                await using var db = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
                await db.OpenAsync(stop);
                var pending = new Queue<string>();
                db.Notification += (_, e) => pending.Enqueue(e.Payload);
                await using var cmd = new NpgsqlCommand("LISTEN market_change", db);
                await cmd.ExecuteNonQueryAsync(stop);
                while (!stop.IsCancellationRequested)
                {
                    await db.WaitAsync(stop);
                    while (pending.TryDequeue(out var raw))
                    {
                        var e = JsonSerializer.Deserialize<JsonElement>(raw);
                        string pod = Environment.MachineName;
                        if (e.GetProperty("kind").GetString() == "stock")
                            await hub.Clients.Group(pod + "Cliente").SendAsync("Market", e, stop);
                        else if (e.GetProperty("owner").GetString() is string owner)
                            await hub.Clients.Group(pod + ":" + owner).SendAsync("Order", e, stop);
                        await hub.Clients.Group(pod + "Admin").SendAsync("Market", e, stop);
                    }
                }
            }
            catch (Exception ex) when (!stop.IsCancellationRequested)
            {
                log.LogWarning(ex, "Reconectando a los eventos del mercado");
                await Task.Delay(3000, stop);
            }
        }
    }
}
