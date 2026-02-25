using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    [Route("carrito")]
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly ITiendaContext _tiendaContext;
        private readonly ICanonicalService _canonical;
        private readonly ApplicationDbContext _context;

        public CarritoController(
            ICarritoService carritoService,
            ITiendaContext tiendaContext,
            ICanonicalService canonical,
            ApplicationDbContext context)
        {
            _carritoService = carritoService;
            _tiendaContext = tiendaContext;
            _canonical = canonical;
            _context = context;
        }

        //[HttpGet("")]
        //public async Task<IActionResult> Index()
        //{
        //    var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;
        //    var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

        //    ViewData["Canonical"] = _canonical.Build("carrito");
        //    ViewData["Title"] = "Carrito de compra";
        //    ViewData["Description"] = "Revisa los productos añadidos antes de finalizar tu compra.";
        //    ViewData["Robots"] = "noindex, nofollow";
        //    ViewBag.TiendaId = tiendaId;

        //    return View(carrito);
        //}


        // 1. El Index suele ser la vista (página del carrito)
        [HttpGet]
        public async Task<IActionResult> Index(int tiendaId)
        {
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            ViewData["Canonical"] = _canonical.Build("carrito");
            ViewData["Title"] = "Carrito de compra";
            ViewData["Description"] = "Revisa los productos añadidos antes de finalizar tu compra.";
            ViewData["Robots"] = "noindex, nofollow";
            ViewBag.TiendaId = tiendaId;
            return View(carrito);
        }

        // 2. El Obtener debe tener su propia "sub-ruta" para el JSON
        [HttpGet("Obtener")] // Esto hace que la URL sea /Carrito/Obtener
        public async Task<IActionResult> Obtener(int tiendaId)
        {
            try
            {
                var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
                return Json(carrito);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("añadir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Añadir(int productoBaseId, int tiendaId, int cantidad, decimal precioCalculado, string detallesOpciones)
        {
            var producto = await _context.ProductosBase
                 .Include(p => p.PropiedadesExtendidas)
                 .FirstOrDefaultAsync(p => p.Id == productoBaseId);

            if (producto == null) return NotFound();

            decimal precioRealServidor = producto.PrecioBase;

            // 1. APLICAR PROPIEDADES NO CONFIGURABLES (Las que no elige el cliente)
            var propsFijas = producto.PropiedadesExtendidas
                .Where(pc => pc.EsConfigurable == false); // Ajusta el nombre de la propiedad 'EsConfigurable' si es distinto

            foreach (var prop in propsFijas)
            {
                precioRealServidor = CalcularPrecio(precioRealServidor, prop.Operacion, prop.Valor);
            }

            // 2. APLICAR PROPIEDADES SELECCIONADAS POR EL CLIENTE
            if (!string.IsNullOrEmpty(detallesOpciones))
            {
                var opcionesSeleccionadas = detallesOpciones.Split(',')
                     .Select(o => o.Contains(':') ? o.Split(':').Last().Trim() : o.Trim())
                     .ToList();

                foreach (var nombreOpt in opcionesSeleccionadas)
                {
                    var propiedadDb = producto.PropiedadesExtendidas
                        .FirstOrDefault(pc => pc.NombrePropiedad != null &&
                                         pc.NombrePropiedad.Trim().Equals(nombreOpt, StringComparison.OrdinalIgnoreCase));

                    if (propiedadDb != null)
                    {
                        precioRealServidor = CalcularPrecio(precioRealServidor, propiedadDb.Operacion, propiedadDb.Valor);
                    }
                }
            }

            // 3. Validación de precio
            if (Math.Abs(precioRealServidor - precioCalculado) > 0.01m)
            {
                precioCalculado = precioRealServidor;
            }

            await _carritoService.AñadirProductoAsync(tiendaId, productoBaseId, cantidad, precioCalculado, detallesOpciones);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Añadido: " + precioCalculado.ToString("C2") });
            }

            TempData["Success"] = "Añadido: " + precioCalculado.ToString("C2");
            return RedirectToAction("Index", "Home");
        }

        // Función auxiliar local para no repetir el switch
        private decimal CalcularPrecio(decimal actual, string operacion, decimal valor)
        {
            return operacion switch
            {
                "Suma" => actual + valor,
                "Resta" => actual - valor,
                "Multiplicacion" => actual * valor,
                _ => actual
            };
        }

        [HttpGet("cantidad")]
        public async Task<JsonResult> Cantidad()
        {
            var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;
            int conteo = await _carritoService.ObtenerCantidadItemsAsync(tiendaId);
            return Json(new { total = conteo });
        }

        // Resto de métodos AJAX (Actualizar, Eliminar, Vaciar) se mantienen igual...

        [HttpPost("actualizar")]
        public async Task<JsonResult> ActualizarCantidadAjax(int productoBaseId, int cantidad, int tiendaId)
        {
            await _carritoService.ActualizarCantidadAsync(tiendaId, productoBaseId, cantidad);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpPost("eliminar")]
        public async Task<JsonResult> EliminarAjax(int productoBaseId, int tiendaId)
        {
            await _carritoService.EliminarProductoAsync(tiendaId, productoBaseId);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpPost("vaciar")]
        public async Task<JsonResult> VaciarAjax(int tiendaId)
        {
            await _carritoService.VaciarAsync(tiendaId);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpGet("GetCartPreview")]
        public IActionResult GetCartPreview()
        {
            // Esto devuelve el ViewComponent que ya tienes creado
            // para que el HTML se refresque con los datos actuales de la DB
            return ViewComponent("CartPreview");
        }


    }
}