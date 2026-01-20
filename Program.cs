using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Productos;
using BaseConLogin.Services.Seguridad;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* =========================
 * Base de datos
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
 * Servicios de dominio
 * ========================= */

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ITiendaAuthorizationService, TiendaAuthorizationService>();

builder.Services.AddScoped<ICarritoService, DbCarritoService>();

builder.Services.AddScoped<ITiendaContext, TiendaContext>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

/* =========================
 * Hosting
 * ========================= */

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}

var app = builder.Build();

/* =========================
 * Inicialización del sistema
 * ========================= */

using (var scope = app.Services.CreateScope())
{
    await InicializarSistemaAsync(scope.ServiceProvider);
}

/* =========================
 * Middleware
 * ========================= */

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();           // SIEMPRE antes de auth
app.UseAuthentication();
app.UseAuthorization();

/* =========================
 * Rutas
 * ========================= */

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

/* =========================
 * Inicialización (roles, admin, tags, carpetas)
 * ========================= */

async Task InicializarSistemaAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    // -------------------------
    // Roles
    // -------------------------
    string[] roles = { "Admin", "User", "AdministradorTienda" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // -------------------------
    // Usuario admin por defecto
    // -------------------------
    const string adminEmail = "admin@admin.com";
    const string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // -------------------------
    // Tags por defecto
    // -------------------------
    if (!context.Tags.Any())
    {
        context.Tags.AddRange(new List<Tag>
        {
            new Tag { Nombre = "Ameritrash" },
            new Tag { Nombre = "eurogame" },
            new Tag { Nombre = "rol" },
            new Tag { Nombre = "wargame" },
            new Tag { Nombre = "miniaturas" },
            new Tag { Nombre = "preventa" }
        });

        await context.SaveChangesAsync();
    }

    // -------------------------
    // Carpetas configuradas
    // -------------------------
    var rutas = configuration.GetSection("RutasArchivos").GetChildren();
    foreach (var ruta in rutas)
    {
        if (!string.IsNullOrWhiteSpace(ruta.Value) && !Directory.Exists(ruta.Value))
        {
            Directory.CreateDirectory(ruta.Value);
        }
    }
}
