using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MiniMarketApp.Data;
using MiniMarketApp.Settings;
using MiniMarketApp.Services;
using MiniMarketApp.Services.Payments;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var dotenv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(dotenv))
{
    foreach (var line in File.ReadAllLines(dotenv))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
    builder.Configuration.AddEnvironmentVariables();
}

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var redis = builder.Configuration.GetConnectionString("Redis");
var realtime = builder.Services.AddSignalR();
if (!string.IsNullOrWhiteSpace(redis))
{
    realtime.AddStackExchangeRedis(redis);
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redis);
}
else builder.Services.AddDistributedMemoryCache();
builder.Services.AddHostedService<MiniMarketApp.Realtime.MarketEvents>();
builder.Services.AddSession();

// Configuración de Data Protection para Docker Swarm (Compartir cookies entre réplicas)
var protection = builder.Services.AddDataProtection()
    
    .SetApplicationName("MarketRubikApp");
if (!string.IsNullOrEmpty(redis)) protection.PersistKeysToStackExchangeRedis(StackExchange.Redis.ConnectionMultiplexer.Connect(redis), "market:keys");
else protection.PersistKeysToFileSystem(new DirectoryInfo(@"/app/keys"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var secretPath = "/run/secrets/db_password";
if (File.Exists(secretPath))
{
    var dbPassword = File.ReadAllText(secretPath).Trim();
    connectionString = connectionString?.Replace("PASSWORD_PLACEHOLDER", dbPassword);
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; // Requiere verificación de correo
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ─── ASIGNACIÓN AUTOMÁTICA DE ROL CLIENTE A USUARIOS REGISTRADOS VÍA FORMULARIO O GOOGLE OAUTH ───
builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, CustomUserClaimsPrincipalFactory>();

// ─── CONFIGURACIÓN DE REGISTRO DE SERVICIOS ESPECIALIZADOS DEL TERCER PARCIAL ───
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, GmailEmailSender>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.Configure<PayPhoneSettings>(builder.Configuration.GetSection("PayPhone"));
builder.Services.AddHttpClient<PayPhoneApiLinkService>();

builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));
builder.Services.AddHttpClient<PayPalService>();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPromotionsService, PromotionsService>();
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddScoped<MassiveSeederService>();

if (!string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Google:ClientId"]) &&
    !string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Google:ClientSecret"]))
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "DUMMY_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "DUMMY_SECRET";
    });

var app = builder.Build();

bool initializeOnly = args.Contains("--initialize");
if (initializeOnly || !builder.Configuration.GetValue<bool>("Database:SkipInitialization"))
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        // First run migrations to create base tables (Categorias, Proveedores, Sucursales, etc.)
        await context.Database.MigrateAsync();
        await identityContext.Database.MigrateAsync();

        // ─── REQUISITO 16: CREAR VISTAS SQL PARA MAPEO CON EF CORE ───
        var sqlViews = @"
            CREATE OR REPLACE VIEW ""vw_VentasPorMes"" AS
            SELECT
                EXTRACT(YEAR FROM ""FechaVenta"")::INT AS ""Anio"",
                EXTRACT(MONTH FROM ""FechaVenta"")::INT AS ""Mes"",
                SUM(""Total"") AS ""Total"",
                COUNT(*)::INT AS ""Cantidad""
            FROM ""Ventas""
            WHERE ""Activo"" = true
            GROUP BY EXTRACT(YEAR FROM ""FechaVenta""), EXTRACT(MONTH FROM ""FechaVenta"");

            CREATE OR REPLACE VIEW ""vw_PromedioVentasCliente"" AS
            SELECT
                c.""Nombre"" || ' ' || c.""Apellido"" AS ""Cliente"",
                AVG(v.""Total"") AS ""PromedioVentas""
            FROM ""Ventas"" v
            JOIN ""Clientes"" c ON v.""ClienteId"" = c.""Id""
            WHERE v.""Activo"" = true
            GROUP BY c.""Id"", c.""Nombre"", c.""Apellido""
            ORDER BY ""PromedioVentas"" DESC
            LIMIT 20;
        ";
        await context.Database.ExecuteSqlRawAsync(sqlViews);
        await context.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Realtime/schema.sql")));
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Falló la inicialización de PostgreSQL.", ex);
    }

    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<MassiveSeederService>();
        if (builder.Configuration.GetValue<bool>("Database:SeedDemo")) await seeder.Seed1MillionAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seeder DDL Exception]: {ex.Message}");
    }

    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        
        if (!await roleManager.RoleExistsAsync("Cliente"))
            await roleManager.CreateAsync(new IdentityRole("Cliente"));

        // ─── USUARIOS POR DEFECTO PARA DEMOSTRACIÓN E INICIO DE SESIÓN INMEDIATO ───
        var defaultAdminEmail = "admin@marketrubik.com";
        var defaultAdmin = await userManager.FindByEmailAsync(defaultAdminEmail);
        if (defaultAdmin == null && builder.Configuration["Bootstrap:AdminPassword"] is string adminPassword)
        {
            defaultAdmin = new IdentityUser
            {
                UserName = defaultAdminEmail,
                Email = defaultAdminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(defaultAdmin, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultAdmin, "Admin");
            }
        }
        else if (defaultAdmin != null)
        {
            if (!await userManager.IsInRoleAsync(defaultAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(defaultAdmin, "Admin");
            }
        }

        // ─── GARANTIZAR QUE TODOS LOS DEMÁS USUARIOS (INCLUIDOS LOS DE OAUTH) TENGAN ROL CLIENTE ───
        if (builder.Configuration["Bootstrap:DemoPassword"] is string demoPassword)
        {
            foreach (var email in new[] { "jose@market.test", "maria@market.test" })
            {
                var demo = await userManager.FindByEmailAsync(email);
                if (demo == null)
                {
                    demo = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    var created = await userManager.CreateAsync(demo, demoPassword);
                    if (!created.Succeeded) throw new InvalidOperationException("No se pudo crear la cuenta de demostración.");
                }
            }
        }
        var existingUsers = await userManager.Users.ToListAsync();
        foreach (var user in existingUsers)
        {
            if (user.Email != defaultAdminEmail)
            {
                if (await userManager.IsInRoleAsync(user, "Admin")) continue;
                if (!await userManager.IsInRoleAsync(user, "Cliente"))
                {
                    await userManager.AddToRoleAsync(user, "Cliente");
                }
            }
        }
    }
    catch (Exception ex)
    {
        // En Swarm con múltiples réplicas, puede haber conflictos de clave duplicada
        // si dos réplicas intentan crear los roles al mismo tiempo. Es seguro ignorarlo.
        throw new InvalidOperationException("Falló la inicialización de usuarios.", ex);
    }
}

if (initializeOnly) return;
app.MapHub<MiniMarketApp.Realtime.MarketHub>("/live");
app.MapGet("/health/live", () => Results.Ok());
app.MapGet("/health/ready", async (AppDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok() : Results.StatusCode(503));
var cultureInfo = new System.Globalization.CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();