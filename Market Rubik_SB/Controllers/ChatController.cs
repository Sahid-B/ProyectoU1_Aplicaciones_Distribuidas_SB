using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketApp.Data;

namespace MiniMarketApp.Controllers;

[Authorize(Roles = "Cliente")]
public class ChatController(UserManager<IdentityUser> users, AppDbContext db, IConfiguration config) : Controller
{
    public IActionResult Index() => User.IsInRole("Admin") ? Forbid() : View();

    public record DirectoryContact(string Id, string Name, bool Online);

    public async Task<IActionResult> DirectoryList()
    {
        if (User.IsInRole("Admin")) return Forbid();
        var me = users.GetUserId(User);
        var contacts = await db.Database.SqlQuery<DirectoryContact>($"""
            SELECT u."Id", split_part(u."UserName",'@',1) AS "Name",
                EXISTS (SELECT 1 FROM market_presence p WHERE p.owner=u."Id"
                    AND p.touched > now()-interval '75 seconds') AS "Online"
            FROM "AspNetUsers" u JOIN "AspNetUserRoles" ur ON ur."UserId"=u."Id"
            JOIN "AspNetRoles" r ON ur."RoleId"=r."Id"
            WHERE r."Name"='Cliente' AND u."Id"<>{me}
            AND NOT EXISTS (SELECT 1 FROM "AspNetUserRoles" ar JOIN "AspNetRoles" a ON a."Id"=ar."RoleId"
                WHERE ar."UserId"=u."Id" AND a."Name"='Admin')
            ORDER BY "Online" DESC, "Name", u."Id"
            """).ToListAsync();
        return Json(contacts);
    }

    public async Task<IActionResult> Conversations()
    {
        if (User.IsInRole("Admin")) return Forbid();
        string me = users.GetUserId(User)!;
        var rows = await db.Database.SqlQuery<ChatContact>($"""
            SELECT u."Id", u."UserName" AS "Name" FROM market_chat m
            JOIN "AspNetUsers" u ON u."Id" = CASE WHEN m."Sender"={me} THEN m."Recipient" ELSE m."Sender" END
            WHERE m."Sender"={me} OR m."Recipient"={me}
            GROUP BY u."Id",u."UserName" ORDER BY MAX(m."Id") DESC LIMIT 100
            """).ToListAsync();
        return Json(rows);
    }

    public record ChatContact(string Id, string Name);
    public async Task<IActionResult> Contact(string email)
    {
        if (User.IsInRole("Admin")) return Forbid();
        if (string.IsNullOrWhiteSpace(email)) return BadRequest();
        var peer = await users.FindByEmailAsync(email.Trim());
        if (peer == null || peer.Id == users.GetUserId(User) || !await users.IsInRoleAsync(peer, "Cliente")
            || await users.IsInRoleAsync(peer, "Admin")) return NotFound();
        return Json(new { peer.Id, name = peer.UserName });
    }

    public async Task<IActionResult> Products(string term)
    {
        if (User.IsInRole("Admin")) return Forbid();
        if (string.IsNullOrWhiteSpace(term)) return Json(Array.Empty<object>());
        return Json(await db.Productos.AsNoTracking().Where(p => p.Activo && p.Nombre.Contains(term))
            .OrderBy(p => p.Nombre).Take(20).Select(p => new { p.Id, p.Nombre, p.Precio }).ToListAsync());
    }

    public IActionResult Ice()
    {
        if (User.IsInRole("Admin")) return Forbid();
        var servers = new List<object>();
        if (config["Rtc:Stun"] is string stun) servers.Add(new { urls = stun });
        if (config["Rtc:Turn"] is string turn) servers.Add(new { urls = turn,
            username = config["Rtc:Username"], credential = config["Rtc:Password"] });
        return Json(servers);
    }
}
