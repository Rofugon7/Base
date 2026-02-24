using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Authorization;
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
            var tiendaId = ObtenerTiendaId();
            var tiendaSlug = "mi-tienda"; // obtener dinámicamente de ITiendaContext si es necesario

            ViewData["Canonical"] = _canonical.Build($"t/{tiendaSlug}");
            ViewData["Title"] = $"Home de {tiendaSlug}";
            ViewData["Description"] = $"Página principal de {tiendaSlug}.";

            // 1. Categorías visibles
            var categorias = await _context.Categorias
                .Where(c => c.TiendaId == tiendaId && c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // 2. Obtener productos base incluyendo sus propiedades
            var query = _context.ProductosBase
                .Include(p => p.PropiedadesExtendidas)
                .AsNoTracking()
                .Where(p => p.Activo && p.TiendaId == tiendaId);

            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Nombre.Contains(search) || p.Descripcion.Contains(search));
            }

            // Aplicar filtro de categoría si existe
            if (categoria.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoria.Value);
            }

            var todosProductos = await query.OrderByDescending(p => p.FechaAlta).ToListAsync();

            // ========================================================
            // FUNCIÓN DE AYUDA: Calcular precio con propiedades FIJAS
            // ========================================================
            decimal CalcularPrecioConFijos(decimal basePrice, IEnumerable<ProductoPropiedadConfigurada> props)
            {
                decimal precioFinal = basePrice;
                // Solo procesamos las que NO son configurables por el usuario
                var propiedadesFijas = props.Where(pe => !pe.EsConfigurable).OrderBy(pe => pe.Orden);

                foreach (var prop in propiedadesFijas)
                {
                    if (prop.Operacion == "Suma") precioFinal += prop.Valor;
                    else if (prop.Operacion == "Resta") precioFinal -= prop.Valor;
                    else if (prop.Operacion == "Multiplicacion") precioFinal *= prop.Valor;
                }
                return precioFinal;
            }

            // 3. Mapeo a ProductoSimpleVM aplicando el cálculo de precios fijos

            // Destacados (primeros 3 en oferta)
            var destacados = todosProductos
                .Where(p => p.Oferta)
                .Take(3)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioBase = CalcularPrecioConFijos(p.PrecioBase, p.PropiedadesExtendidas),
                    ImagenPrincipal = p.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    EsNuevo = p.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Oferta,
                    CategoriaId = p.CategoriaId
                }).ToList();

            // Últimas unidades (stock <= 5)
            var ultimasUnidades = todosProductos
                .Where(p => p.Stock <= 5)
                .Take(4)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioBase = CalcularPrecioConFijos(p.PrecioBase, p.PropiedadesExtendidas),
                    ImagenPrincipal = p.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    EsNuevo = p.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Oferta,
                    CategoriaId = p.CategoriaId
                }).ToList();

            // Paginación principal
            var productosPagina = todosProductos
                .Skip((page - 1) * itemsPorPagina)
                .Take(itemsPorPagina)
                .Select(p => new ProductoSimpleVM
                {
                    ProductoBaseId = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioBase = CalcularPrecioConFijos(p.PrecioBase, p.PropiedadesExtendidas),
                    ImagenPrincipal = p.ImagenPrincipal,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    EsNuevo = p.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Oferta,
                    CategoriaId = p.CategoriaId
                }).ToList();

            // 4. Construcción del ViewModel Final
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

            var query = _context.ProductosBase
                .Where(p => p.TiendaId == tiendaId);

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p => p.Nombre.Contains(q) || p.Descripcion.Contains(q));

            if (categoria.HasValue)
                query = query.Where(p => p.CategoriaId == categoria);

            int totalItems = await query.CountAsync();

            var productos = await query
                .OrderByDescending(p => p.Id)
                .Include(p => p.PropiedadesExtendidas)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoCardVM
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.PrecioBase,
                    Imagen = p.ImagenPrincipal,
                    EsNuevo = p.FechaAlta > DateTime.UtcNow.AddDays(-7),
                    EsOferta = p.Oferta,
                    Propiedades = p.PropiedadesExtendidas.Select(pe => new PropiedadFilaVM
                    {
                        Valor = pe.Valor,
                        Operacion = pe.Operacion,
                        Orden = pe.Orden
                    }).ToList()
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
        [HttpGet("Home/QuienesSomos")]
        public IActionResult QuienesSomos()
        {
            return View();
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

        [HttpGet("Home/Cookies")]
        [AllowAnonymous]
        public IActionResult Cookies()
        {
            return View();
        }

        [HttpGet("Home/Privacidad")]
        [AllowAnonymous]
        public IActionResult Privacidad()
        {
            return View();
        }

        [HttpGet("Home/CondicionesGenerales")]
        [AllowAnonymous]
        public IActionResult CondicionesGenerales()
        {
            return View();
        }
        

    }
}
