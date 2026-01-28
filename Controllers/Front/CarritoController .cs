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

        // =========================
        // Mostrar carrito (NO INDEX)
        // =========================
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var tiendaId =
                _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;

            var carrito =
                await _carritoService.ObtenerCarritoAsync(tiendaId);

            // === CANONICAL ===
            ViewData["Canonical"] =
                _canonical.Build("carrito");

            // === SEO ===
            ViewData["Title"] =
                "Carrito de compra";

            ViewData["Description"] =
                "Revisa los productos añadidos antes de finalizar tu compra.";

            // === BLOQUEAR INDEXACIÓN ===
            ViewData["Robots"] =
                "noindex, nofollow";

            return View(carrito);
        }

        // =========================
        // AJAX: obtener carrito
        // =========================
        [HttpGet("obtener")]
        public async Task<JsonResult> ObtenerCarrito(int tiendaId)
        {
            var carrito =
                await _carritoService.ObtenerCarritoAsync(tiendaId);

            return Json(carrito);
        }

        // =========================
        // AJAX: actualizar cantidad
        // =========================
        [HttpPost("actualizar")]
        public async Task<JsonResult> ActualizarCantidadAjax(
            int productoBaseId,
            int cantidad,
            int tiendaId)
        {
            await _carritoService
                .ActualizarCantidadAsync(
                    tiendaId,
                    productoBaseId,
                    cantidad);

            var carrito =
                await _carritoService.ObtenerCarritoAsync(tiendaId);

            return Json(carrito);
        }

        // =========================
        // AJAX: eliminar producto
        // =========================
        [HttpPost("eliminar")]
        public async Task<JsonResult> EliminarAjax(
            int productoBaseId,
            int tiendaId)
        {
            await _carritoService
                .EliminarProductoAsync(
                    tiendaId,
                    productoBaseId);

            var carrito =
                await _carritoService.ObtenerCarritoAsync(tiendaId);

            return Json(new { carrito });
        }

        // =========================
        // AJAX: vaciar carrito
        // =========================
        [HttpPost("vaciar")]
        public async Task<JsonResult> VaciarAjax(int tiendaId)
        {
            await _carritoService.VaciarAsync(tiendaId);

            var carrito =
                await _carritoService.ObtenerCarritoAsync(tiendaId);

            return Json(carrito);
        }

        [HttpGet("cantidad")]
        public async Task<JsonResult> Cantidad()
        {
            var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;

            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            var total = carrito?.Items?.Sum(i => i.Cantidad) ?? 0;

            return Json(new { total });
        }

    }
}
