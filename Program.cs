using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Pedidos;
using BaseConLogin.Services.Productos;
using BaseConLogin.Services.Seguridad;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* =========================
 * Base de Datos
 * ========================= */
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

/* =========================
 * Identity
 * ========================= */
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

/* =========================
 * MVC + Razor Pages
 * ========================= */
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

/* =========================
 * Configuración de Sesión (CORREGIDO)
 * ========================= */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Crucial para carritos de invitados
});

/* =========================
 * Servicios de Dominio
 * ========================= */
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ITiendaAuthorizationService, TiendaAuthorizationService>();
builder.Services.AddScoped<ITiendaMenuService, TiendaMenuService>();
builder.Services.AddScoped<ICarritoService, DbCarritoService>();
builder.Services.AddScoped<ITiendaContext, TiendaContext>();
builder.Services.AddScoped<ICanonicalService, CanonicalService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IClaimsTransformation, TiendaClaimsTransformation>();

// Inyección personalizada de ApplicationDbContext si es necesaria para Multi-tenancy
builder.Services.AddScoped<ApplicationDbContext>(provider =>
{
    var options = provider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
    var tiendaContext = provider.GetRequiredService<ITiendaContext>();
    return new ApplicationDbContext(options, tiendaContext);
});

/* =========================
 * Hosting & Optimización
 * ========================= */
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}
builder.Services.AddResponseCompression();

var app = builder.Build();

/* =========================
 * Inicialización del Sistema
 * ========================= */
using (var scope = app.Services.CreateScope())
{
    await InicializarSistemaAsync(scope.ServiceProvider);
}

/* =========================
 * Pipeline de Middleware (ORDEN CORREGIDO)
 * ========================= */
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseStaticFiles();
app.UseResponseCompression();

// Lógica de redirección (WWW y HTTPS)
app.Use(async (context, next) =>
{
    var request = context.Request;
    if (!app.Environment.IsDevelopment() && !request.IsHttps)
    {
        var httpsUrl = "https://" + request.Host + request.Path + request.QueryString;
        context.Response.Redirect(httpsUrl, true);
        return;
    }
    if (request.Host.Host.StartsWith("www."))
    {
        var newHost = request.Host.Host.Replace("www.", "");
        var newUrl = $"{request.Scheme}://{newHost}{request.Path}{request.QueryString}";
        context.Response.Redirect(newUrl, true);
        return;
    }
    await next();
});

app.UseRouting();

// El orden de estos 4 es vital para el funcionamiento del Carrito y la Tienda
app.UseSession();
app.UseMiddleware<TiendaResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

/* =========================
 * Rutas
 * ========================= */
app.MapControllerRoute(
    name: "tienda",
    pattern: "t/{slug}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

/* =========================
 * Método de Inicialización
 * ========================= */
async Task InicializarSistemaAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    string[] roles = { "Admin", "User", "AdministradorTienda" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    const string adminEmail = "admin@admin.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    if (!context.Tags.Any())
    {
        context.Tags.AddRange(new List<Tag> {
            new Tag { Nombre = "Ameritrash" }, new Tag { Nombre = "eurogame" },
            new Tag { Nombre = "rol" }, new Tag { Nombre = "wargame" },
            new Tag { Nombre = "miniaturas" }, new Tag { Nombre = "preventa" }
        });
        await context.SaveChangesAsync();
    }

    var rutas = configuration.GetSection("RutasArchivos").GetChildren();
    foreach (var ruta in rutas)
    {
        if (!string.IsNullOrWhiteSpace(ruta.Value) && !Directory.Exists(ruta.Value))
            Directory.CreateDirectory(ruta.Value);
    }
}