using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Productos;
using BaseConLogin.Services.Seguridad;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionCarritoService>();

builder.Services.AddSession();
builder.Services.AddScoped<ITiendaAuthorizationService, TiendaAuthorizationService>();



if (builder.Environment.IsDevelopment())
{
    // No forzar UseUrls en local
}
else
{
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}

var app = builder.Build();

// Crear roles y usuario admin al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CrearRolesYAdminPorDefectoAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Crear carpetas de archivos si no existen
var rutasArchivos = builder.Configuration.GetSection("RutasArchivos").GetChildren();
foreach (var ruta in rutasArchivos)
{
    var rutaCompleta = ruta.Value;
    if (!Directory.Exists(rutaCompleta))
    {
        Directory.CreateDirectory(rutaCompleta);
        Console.WriteLine($"[INIT] Carpeta creada: {rutaCompleta}");
    }
}

//Crear tags por defecto si no existe ninguno en la BBDD

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Tags.Any())
    {
        var tags = new List<Tag>
        {
            new Tag { Nombre = "Ameritrash" },
            new Tag { Nombre = "ciencia ficción" },
            new Tag { Nombre = "comic" },
            new Tag { Nombre = "digital" },
            new Tag { Nombre = "eurogame" },
            new Tag { Nombre = "fantasía" },
            new Tag { Nombre = "filler" },
            new Tag { Nombre = "físico" },
            new Tag { Nombre = "histórico" },
            new Tag { Nombre = "juego" },
            new Tag { Nombre = "libro" },
            new Tag { Nombre = "libro-juego" },
            new Tag { Nombre = "miniaturas" },
            new Tag { Nombre = "preventa" },
            new Tag { Nombre = "rol" },
            new Tag { Nombre = "STL" },
            new Tag { Nombre = "temático" },
            new Tag { Nombre = "terror" },
            new Tag { Nombre = "vídeo-juego" },
            new Tag { Nombre = "wargame" }
        };

        context.Tags.AddRange(tags);
        context.SaveChanges();
    }
}

// Crear roles al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = new[] { "Admin", "User", "AdministradorTienda" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Crear los roles en la base de datos
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    string[] roles = new[] { "Administrador", "Usuario" };

//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//        {
//            await roleManager.CreateAsync(new IdentityRole(role));
//        }
//    }
//}

app.Run();


async Task CrearRolesYAdminPorDefectoAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "User" };

    // Crear roles si no existen
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Crear usuario admin por defecto
    string adminEmail = "admin@admin.com";
    string adminPassword = "Admin123!";

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
}

