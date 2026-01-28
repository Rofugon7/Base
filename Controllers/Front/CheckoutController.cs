using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Pedidos;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers.Front
{
    [Authorize]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly ITiendaContext _tiendaContext;
        private readonly IPedidoService _pedidoService;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            IPedidoService pedidoService,
            ITiendaContext tiendaContext,
            UserManager<IdentityUser> userManager,
            ICarritoService carritoService)
        {
            _pedidoService = pedidoService;
            _tiendaContext = tiendaContext;
            _userManager = userManager;
            _carritoService = carritoService;                   
        }

        public CheckoutController(
            ICarritoService carritoService,
            ITiendaContext tiendaContext)
        {
            _carritoService = carritoService;
            _tiendaContext = tiendaContext;
        }

        // =========================
        // Página checkout
        // =========================
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;

            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            if (carrito == null || !carrito.Items.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            return View(carrito);
        }

        // =========================
        // Confirmar checkout
        // =========================
        [HttpPost("confirmar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar()
        {
            var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;

            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            if (carrito == null || !carrito.Items.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            // Aquí en la siguiente fase:
            // -> Crear Pedido
            // -> Redirigir a pago

            return RedirectToAction("Index", "Pago");
        }

        [HttpGet("gracias/{id}")]
        public IActionResult Gracias(int id)
        {
            ViewBag.PedidoId = id;
            return View();
        }

    }
}
