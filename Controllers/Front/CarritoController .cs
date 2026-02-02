using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers.Front
{
    [Route("carrito")]
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly ITiendaContext _tiendaContext;
        private readonly ICanonicalService _canonical;

        public CarritoController(
            ICarritoService carritoService,
            ITiendaContext tiendaContext,
            ICanonicalService canonical)
        {
            _carritoService = carritoService;
            _tiendaContext = tiendaContext;
            _canonical = canonical;
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

        // ==========================================
        // MÉTODO PARA AÑADIR (CORREGIDO Y RENOMBRADO)
        // ==========================================
        [HttpPost("añadir")]
        public async Task<IActionResult> Añadir(int productoBaseId, int cantidad = 1, int? tiendaId = null)
        {
            // Priorizamos el tiendaId del formulario, si no, usamos el contexto
            var idFinalTienda = tiendaId ?? _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;

            try
            {
                await _carritoService.AñadirProductoAsync(idFinalTienda, productoBaseId, cantidad);
                int nuevoTotal = await _carritoService.ObtenerCantidadItemsAsync(idFinalTienda);

                return Json(new { success = true, total = nuevoTotal });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        
    }
}