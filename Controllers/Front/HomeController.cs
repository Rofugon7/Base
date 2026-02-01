using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> Index(int? categoria, string? search, int page = 1, int itemsPorPagina = 9)
        {
            var tiendaSlug = "mi-tienda"; // obtener dinámicamente de ITiendaContext
            ViewData["Canonical"] = _canonical.Build($"t/{tiendaSlug}");
            ViewData["Title"] = $"Home de {tiendaSlug}";
            ViewData["Description"] = $"pagina principal de {tiendaSlug}.";

            var tiendaId = ObtenerTiendaId();

            // Categorías visibles
            var categorias = await _context.Categorias
                .Where(c => c.TiendaId == tiendaId && c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();


            // Todos los productos simples activos
            var todosProductos = await _context.ProductoSimples
                .Include(p => p.Producto)
                .Where(p => p.Producto.Activo && p.Producto.TiendaId == tiendaId)
                .OrderByDescending(p => p.Producto.FechaAlta)
                .ToListAsync();

            // Destacados (primeros 3 en oferta)
            var destacados = todosProductos
                .Where(p => p.Producto.Oferta && p.Producto.TiendaId == tiendaId)
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
                    EsOferta = p.Producto.Oferta,
                    CategoriaId = p.Producto.CategoriaId
                })
                .ToList();

            // Últimas unidades (menos de 5 unidades)
            var ultimasUnidades = todosProductos
                .Where(p => p.Stock <= 5 && p.Producto.TiendaId == tiendaId)
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
                    EsOferta = p.Producto.Oferta,
                    CategoriaId = p.Producto.CategoriaId
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
                    EsOferta = p.Producto.Oferta,
                    CategoriaId = p.Producto.CategoriaId
                })
                .ToList();

            if (categoria.HasValue)
            {
                todosProductos = todosProductos.Where(p =>
                    p.Producto.CategoriaId == categoria.Value).ToList();
                destacados = destacados.Where(p =>
                    p.CategoriaId == categoria.Value).ToList();
                ultimasUnidades = ultimasUnidades.Where(p =>
                    p.CategoriaId == categoria.Value).ToList();
                productosPagina = productosPagina.Where(p =>
                    p.CategoriaId == categoria.Value).ToList();
            }

            var vm = new HomeIndexVM
            {
                Productos = productosPagina,
                Destacados = destacados,
                UltimasUnidades = ultimasUnidades,
                Categorias = categorias,
                CategoriaSeleccionada = categoria,
                Busqueda = search,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(todosProductos.Count / (double)itemsPorPagina)
            };

            return View(vm);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string q, int? categoria, int page = 1)
        {
            const int pageSize = 12;
            int tiendaId = ObtenerTiendaId();

            var query = _context.ProductoSimples
                .Include(p => p.Producto)
                .Where(p => p.Producto.TiendaId == tiendaId);

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p => p.Producto.Nombre.Contains(q) || p.Producto.Descripcion.Contains(q));

            if (categoria.HasValue)
                query = query.Where(p => p.Producto.CategoriaId == categoria);

            int totalItems = await query.CountAsync();

            var productos = await query
                .OrderByDescending(p => p.Producto.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoCardVM
                {
                    Id = p.Producto.Id,
                    Nombre = p.Producto.Nombre,
                    Precio = p.Producto.PrecioBase,
                    Imagen = p.Producto.ImagenPrincipal,
                    EsNuevo = p.Producto.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Producto.Oferta
                })
                .ToListAsync();

            var vm = new SearchResultVM
            {
                Query = q,
                CategoriaId = categoria,
                Productos = productos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return View(vm);
        }



        private int ObtenerTiendaId()
        {
            var claim = User.FindFirst("TiendaId");

            if (claim == null)
                throw new Exception("El usuario no tiene TiendaId en los claims");

            if (!int.TryParse(claim.Value, out int tiendaId))
                throw new Exception("TiendaId inválido en claims");

            return tiendaId;
        }

    }
}
