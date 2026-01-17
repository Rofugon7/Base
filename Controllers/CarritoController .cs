using BaseConLogin.Models;
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

        // ========================
        // Mostrar carrito
        // ========================
        public async Task<IActionResult> Index()
        {
            var carrito = await _carritoService.ObtenerCarritoAsync();
            return View(carrito);
        }

        // ========================
        // Añadir producto al carrito
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Añadir(int productoBaseId, int cantidad = 1, string returnUrl = null)
        {
            try
            {
                await _carritoService.AñadirProductoAsync(productoBaseId, cantidad);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            // Redirige a la página anterior o al carrito
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // ========================
        // Eliminar producto del carrito
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int productoBaseId)
        {
            await _carritoService.EliminarProductoAsync(productoBaseId);
            return RedirectToAction("Index");
        }

        // ========================
        // Vaciar carrito
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vaciar()
        {
            await _carritoService.VaciarAsync();
            return RedirectToAction("Index");
        }

        // ========================
        // Actualizar cantidad de un producto
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(int productoBaseId, int cantidad)
        {
            await _carritoService.ActualizarCantidadAsync(productoBaseId, cantidad);
            return RedirectToAction("Index");
        }

        // Obtener carrito en JSON
        [HttpGet]
        public async Task<IActionResult> ObtenerCarrito()
        {
            var carrito = await _carritoService.ObtenerCarritoAsync();
            return Json(carrito);
        }

        // Actualizar cantidad (AJAX)        

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidadAjax(int productoBaseId, int cantidad)
        {
            await _carritoService.ActualizarCantidadAsync(productoBaseId, cantidad);
            var carrito = await _carritoService.ObtenerCarritoAsync();
            return Json(carrito);
        }


        // Eliminar producto (AJAX)
        [HttpPost]
        public async Task<IActionResult> EliminarAjax(int productoBaseId)
        {
            await _carritoService.EliminarProductoAsync(productoBaseId);
            var carrito = await _carritoService.ObtenerCarritoAsync();

            return Json(new { carrito });
        }



        // Vaciar carrito (AJAX)
        [HttpPost]
        public async Task<IActionResult> VaciarAjax()
        {
            await _carritoService.VaciarAsync();
            var carrito = await _carritoService.ObtenerCarritoAsync();
            return Json(carrito);
        }

    }
}
