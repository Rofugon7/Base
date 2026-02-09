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
            // 1. Intentar obtenerlo de los Claims (si está logueado)
            var claim = User.FindFirst("TiendaId");
            if (claim != null && int.TryParse(claim.Value, out int tiendaIdFromClaim))
            {
                return tiendaIdFromClaim;
            }

            // 2. Si es invitado (o el claim no existe), devolver un ID por defecto
            // Aquí puedes poner el ID de tu tienda principal (ejemplo: 1)
            // Lo ideal es que esto venga de un servicio de contexto de tienda o configuración
            return 1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suscribirse(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest();

            // Verificar si ya existe
            var existe = await _context.Suscriptores.AnyAsync(s => s.Email == email);

            if (!existe)
            {
                var nuevo = new Suscriptor { Email = email };
                _context.Suscriptores.Add(nuevo);
                await _context.SaveChangesAsync();
                TempData["NewsletterSuccess"] = "¡Gracias! Te has suscrito correctamente.";
            }
            else
            {
                TempData["NewsletterInfo"] = "Este correo ya está suscrito.";
            }

            // Redirige a donde estaba el usuario
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet("Home/PreguntasFrecuentes")]
        public IActionResult PreguntasFrecuentes()
        {
            return View();
        }

        [HttpGet("Home/Envios")]
        public IActionResult Envios()
        {
            return View();
        }

        [HttpGet("Home/Plantillas")]
        public IActionResult Plantillas()
        {
            return View();
        }

        [HttpGet("Home/Presupuesto")]
        public IActionResult Presupuesto()
        {
            return View();
        }

 
        [HttpPost("Home/Presupuesto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Presupuesto(Presupuesto modelo)
        {
            if (ModelState.IsValid)
            {
                // Guardamos en Base de Datos
                _context.Add(modelo);
                await _context.SaveChangesAsync();

                // Usamos TempData para mostrar un aviso en la vista tras la redirección
                TempData["Success"] = "¡Presupuesto enviado correctamente! Nos pondremos en contacto contigo pronto.";

                return RedirectToAction(nameof(Presupuesto));
            }

            // Si hay errores de validación, volvemos a mostrar la vista con los datos
            return View(modelo);
        }

        // Listado de presupuestos (GET: Home/ListadoPresupuestos)
        [HttpGet("Home/ListadoPresupuestos")]
        public async Task<IActionResult> ListadoPresupuestos()
        {
            var presupuestos = await _context.Presupuestos
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
            return View(presupuestos);
        }

        // Detalle del presupuesto (GET: Home/DetallePresupuesto/5)
        [HttpGet("Home/DetallePresupuesto/{id}")]
        public async Task<IActionResult> DetallePresupuesto(int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (presupuesto == null) return NotFound();

            return View(presupuesto);
        }

        [HttpPost("Home/EliminarPresupuesto/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPresupuesto(int id)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);

            if (presupuesto != null)
            {
                _context.Presupuestos.Remove(presupuesto);
                await _context.SaveChangesAsync();
                TempData["Success"] = "El presupuesto ha sido eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo encontrar el presupuesto para eliminar.";
            }

            return RedirectToAction(nameof(ListadoPresupuestos));
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto != null)
            {
                presupuesto.Estado = nuevoEstado;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DetallePresupuesto), new { id = id });
        }

    }
}
