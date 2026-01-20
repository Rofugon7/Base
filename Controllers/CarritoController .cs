using BaseConLogin.Services.Carritos;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;

        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        public async Task<IActionResult> Index(int tiendaId = 1)
        {
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return View(carrito);
        }

        [HttpPost]
        public async Task<IActionResult> Añadir(int tiendaId, int productoBaseId, int cantidad = 1)
        {
            await _carritoService.AñadirProductoAsync(tiendaId, productoBaseId, cantidad);
            return RedirectToAction(nameof(Index), new { tiendaId });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarAjax(int tiendaId, int productoBaseId)
        {
            await _carritoService.EliminarProductoAsync(tiendaId, productoBaseId);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpPost]
        public async Task<IActionResult> VaciarAjax(int tiendaId)
        {
            await _carritoService.VaciarAsync(tiendaId);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidadAjax(int tiendaId, int productoBaseId, int cantidad)
        {
            await _carritoService.ActualizarCantidadAsync(tiendaId, productoBaseId, cantidad);
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCarrito(int tiendaId)
        {
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
            return Json(carrito);
        }
    }
}
