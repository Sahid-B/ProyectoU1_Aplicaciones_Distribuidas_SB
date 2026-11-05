using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;

namespace MiniMarketApp.Realtime;

public record ChatMessage(long Id, string Sender, string Recipient, string Body,
    int? ProductId, string? ProductName, decimal? Price, DateTime Created, long? ReplyTo, bool Edited, bool Deleted, string? SenderReaction, string? RecipientReaction);

[Authorize(Roles = "Cliente,Admin")]
public class MarketHub(AppDbContext db, UserManager<IdentityUser> users) : Hub
{
    string Me => Context.UserIdentifier ?? throw new HubException("Inicia sesión.");
    public override async Task OnConnectedAsync()
    {
        var role = Context.User!.IsInRole("Admin") ? "Admin" : "Cliente";
        await Groups.AddToGroupAsync(Context.ConnectionId, Environment.MachineName + role);
        await Groups.AddToGroupAsync(Context.ConnectionId, Environment.MachineName + ":" + Me);
        if (role == "Cliente")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "contacts");
            await Heartbeat();
            await Clients.Group("contacts").SendAsync("ContactsChanged");
        }
        await base.OnConnectedAsync();
    }

    [Authorize(Roles = "Cliente")]
    public async Task Heartbeat()
    {
        if (Context.User!.IsInRole("Admin")) return;
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO market_presence VALUES ({Context.ConnectionId},{Me},now()) ON CONFLICT (connection) DO UPDATE SET touched=now()");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.IsInRole("Cliente") == true && !Context.User.IsInRole("Admin"))
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM market_presence WHERE connection={Context.ConnectionId} OR touched < now()-interval '2 minutes'");
            await Clients.Group("contacts").SendAsync("ContactsChanged");
        }
        await base.OnDisconnectedAsync(exception);
    }

    async Task CheckPeer(string peer)
    {
        var other = await users.FindByIdAsync(peer);
        if (Context.User!.IsInRole("Admin") || peer == Me || other == null ||
            !await users.IsInRoleAsync(other, "Cliente") || await users.IsInRoleAsync(other, "Admin"))
            throw new HubException("Conversación no disponible.");
    }

    [Authorize(Roles = "Cliente")]
    public async Task<List<ChatMessage>> History(string peer)
    {
        await CheckPeer(peer);
        var rows = await db.Database.SqlQuery<ChatMessage>($"""
            SELECT * FROM market_chat WHERE ("Sender"={Me} AND "Recipient"={peer})
            OR ("Sender"={peer} AND "Recipient"={Me}) ORDER BY "Id" DESC LIMIT 100
            """).ToListAsync();
        rows.Reverse();
        return rows;
    }

    [Authorize(Roles = "Cliente")]
    public Task Send(string peer, string body, int? productId) => SendCore(peer, body, productId, null);

    [Authorize(Roles = "Cliente")]
    public Task Reply(string peer, string body, int? productId, long replyTo) => SendCore(peer, body, productId, replyTo);

    async Task SendCore(string peer, string body, int? productId, long? replyTo)
    {
        await CheckPeer(peer);
        body = (body ?? "").Trim();
        if (body.Length > 2000 || (body.Length == 0 && productId == null))
            throw new HubException("Escribe un mensaje de hasta 2000 caracteres o adjunta un producto.");
        var product = productId == null ? null : await db.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Activo && p.Id == productId);
        if (productId != null && product == null) throw new HubException("Producto no disponible.");
        var name = product?.Nombre;
        decimal? price = product?.Precio;
        var rows = await db.Database.SqlQuery<ChatMessage>($"""
            INSERT INTO market_chat ("Sender","Recipient","Body","ProductId","ProductName","Price","ReplyTo")
            SELECT {Me},{peer},{body},{productId}::integer,{name}::text,{price}::numeric,{replyTo}::bigint
            WHERE {replyTo}::bigint IS NULL OR EXISTS (SELECT 1 FROM market_chat WHERE "Id"={replyTo}
              AND (("Sender"={Me} AND "Recipient"={peer}) OR ("Sender"={peer} AND "Recipient"={Me})))
            RETURNING *
            """).ToListAsync();
        if (rows.Count == 0) throw new HubException("Mensaje no disponible.");
        await Clients.Users(Me, peer).SendAsync("Message", rows.Single());
    }

    async Task NotifyChange(FormattableString query)
    {
        var rows = await db.Database.SqlQuery<ChatMessage>(query).ToListAsync();
        if (rows.Count == 0) throw new HubException("Mensaje no disponible.");
        var message = rows.Single();
        await Clients.Users(message.Sender, message.Recipient).SendAsync("Changed", message);
    }

    [Authorize(Roles = "Cliente")]
    public Task Edit(long id, string body)
    {
        body = (body ?? "").Trim();
        if (body.Length is 0 or > 2000) throw new HubException("Texto inválido.");
        return NotifyChange($"""
            UPDATE market_chat SET "Body"={body},"Edited"=true WHERE "Id"={id} AND "Sender"={Me} AND NOT "Deleted" RETURNING *
            """);
    }

    [Authorize(Roles = "Cliente")]
    public Task Remove(long id) => NotifyChange($"""
        UPDATE market_chat SET "Deleted"=true,"Body"='', "ProductId"=NULL,"ProductName"=NULL,"Price"=NULL,
        "SenderReaction"=NULL,"RecipientReaction"=NULL WHERE "Id"={id} AND "Sender"={Me} AND NOT "Deleted" RETURNING *
        """);

    [Authorize(Roles = "Cliente")]
    public Task React(long id, string emoji)
    {
        if (emoji is not ("👍" or "❤️" or "😂")) throw new HubException("Reacción inválida.");
        return NotifyChange($"""
            UPDATE market_chat SET "SenderReaction"=CASE WHEN "Sender"={Me} THEN
            CASE WHEN "SenderReaction"={emoji} THEN NULL ELSE {emoji} END ELSE "SenderReaction" END,
            "RecipientReaction"=CASE WHEN "Recipient"={Me} THEN CASE WHEN "RecipientReaction"={emoji}
            THEN NULL ELSE {emoji} END ELSE "RecipientReaction" END
            WHERE "Id"={id} AND {Me} IN ("Sender","Recipient") AND NOT "Deleted" RETURNING *
            """);
    }

    [Authorize(Roles = "Cliente")]
    public async Task Typing(string peer)
    {
        await CheckPeer(peer);
        await Clients.User(peer).SendAsync("Typing", Me);
    }

    [Authorize(Roles = "Cliente")]
    public async Task Signal(string peer, string callId, string kind, string data)
    {
        await CheckPeer(peer);
        if (!Guid.TryParse(callId, out _) || kind is not ("invite" or "accept" or "end" or "offer" or "answer" or "ice")
            || data.Length > 24000) throw new HubException("Señal inválida.");
        await Clients.User(peer).SendAsync("Call", Me, callId, kind, data);
    }

    [Authorize(Roles = "Admin")]
    public async Task<object> Totals() => new {
        count = await db.Ventas.CountAsync(v => v.Activo),
        total = await db.Ventas.Where(v => v.Activo).SumAsync(v => (decimal?)v.Total) ?? 0
    };
}
