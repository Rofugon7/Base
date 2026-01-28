using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Seo;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICanonicalService _canonical;

        public HomeController(ApplicationDbContext context, ICanonicalService canonical)
        {
            _context = context;
            _canonical = canonical;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, int itemsPorPagina = 9)
        {
            var tiendaSlug = "mi-tienda"; // obtener dinámicamente de ITiendaContext
            ViewData["Canonical"] = _canonical.Build($"t/{tiendaSlug}");
            ViewData["Title"] = $"Home de {tiendaSlug}";
            ViewData["Description"] = $"pagina principal de {tiendaSlug}.";

            // Todos los productos simples activos
            var todosProductos = await _context.ProductoSimples
                .Include(p => p.Producto)
                .Where(p => p.Producto.Activo)
                .OrderByDescending(p => p.Producto.FechaAlta)
                .ToListAsync();

            // Destacados (primeros 3 en oferta)
            var destacados = todosProductos
                .Where(p => p.Producto.Oferta)
                .Take(3)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.ProductoBaseId,
                    Nombre = p.Producto.Nombre,
                    Descripcion = p.Producto.Descripcion,
                    PrecioBase = p.Producto.PrecioBase,
                    ImagenPrincipal = p.Producto.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Producto.Activo,
                    EsNuevo = p.Producto.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Producto.Oferta
                })
                .ToList();

            // Últimas unidades (menos de 5 unidades)
            var ultimasUnidades = todosProductos
                .Where(p => p.Stock <= 5)
                .Take(3)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.ProductoBaseId,
                    Nombre = p.Producto.Nombre,
                    Descripcion = p.Producto.Descripcion,
                    PrecioBase = p.Producto.PrecioBase,
                    ImagenPrincipal = p.Producto.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Producto.Activo,
                    EsNuevo = p.Producto.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Producto.Oferta
                })
                .ToList();

            // Paginación
            var productosPagina = todosProductos
                .Skip((page - 1) * itemsPorPagina)
                .Take(itemsPorPagina)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.ProductoBaseId,
                    Nombre = p.Producto.Nombre,
                    Descripcion = p.Producto.Descripcion,
                    PrecioBase = p.Producto.PrecioBase,
                    ImagenPrincipal = p.Producto.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Producto.Activo,
                    EsNuevo = p.Producto.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Producto.Oferta
                })
                .ToList();

            var vm = new HomeIndexVM
            {
                Productos = productosPagina,
                Destacados = destacados,
                UltimasUnidades = ultimasUnidades,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(todosProductos.Count / (double)itemsPorPagina)
            };

            return View(vm);
        }


    }
}
