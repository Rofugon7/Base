using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Pedidos;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly ICarritoService _carritoService;
    private readonly ITiendaContext _tiendaContext;
    private readonly IPedidoService _pedidoService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CheckoutController(
        IPedidoService pedidoService,
        ITiendaContext tiendaContext,
        UserManager<ApplicationUser> userManager,
        ICarritoService carritoService,
        ApplicationDbContext context)
    {
        _pedidoService = pedidoService;
        _tiendaContext = tiendaContext;
        _userManager = userManager;
        _carritoService = carritoService;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var tiendaId = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;
        var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);
        var config = await _context.TiendaConfigs.FirstOrDefaultAsync(); // O por TiendaId

        if (carrito == null || !carrito.Items.Any())
            return RedirectToAction("Index", "Carrito");

        // --- LÓGICA DE ENVÍO ---
        // Aquí puedes consultar tu base de datos. Por ahora, usemos valores de ejemplo:
        decimal precioEnvio = 5.00m;
        decimal umbralGratis = 50.00m;
        decimal subtotal = carrito.Items.Sum(i => i.PrecioUnitario * i.Cantidad);

        decimal gastosEnvioFinal = (subtotal >= umbralGratis) ? 0 : precioEnvio;

        var model = new CheckoutViewModel
        {
            TiendaId = tiendaId,
            Carrito = carrito,
            NombreCompleto = user.NombreCompleto,
            DniCif = user.NifCif,
            Direccion = user.Direccion,
            Ciudad = user.Ciudad,
            CodigoPostal = user.CodigoPostal,
            Telefono = user.TelefonoContacto,
            GastosEnvio = gastosEnvioFinal,
            PermitirRecogidaTienda = config.PermitirRecogidaTienda,
            PrecioUrgente = config.EnvioUrgente,
            UmbralEnvioGratis = config.EnvioGratisDesde,
            // Lógica de Envío Gratis
            PrecioEstandar = (subtotal >= config.EnvioGratisDesde) ? 0 : config.EnvioEstandar,
            TipoEnvioSeleccionado = "Estandar" // Por defecto
        };
        return View(model);
    }

    [HttpPost("confirmar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(CheckoutViewModel model)
    {
        // 1. Intentamos obtener el carrito de cualquier forma posible
        // Primero probamos con el ID que viene (aunque sea 0)
        var carrito = await _carritoService.ObtenerCarritoAsync(model.TiendaId);

        // 2. TRUCO DE SEGURIDAD: Si el carrito está vacío y el ID era 0, 
        // es muy probable que el ID real esté en la sesión pero bajo otro nombre.
        // Muchos sistemas guardan la tienda actual en una cookie o Claim.
        if (carrito == null || !carrito.Items.Any())
        {
            // Si el TiendaContext funciona en otros lados, lo usamos aquí
            int tiendaIdRecuperada = _tiendaContext.ObtenerTiendaIdOpcional() ?? 0;
            if (tiendaIdRecuperada > 0)
            {
                carrito = await _carritoService.ObtenerCarritoAsync(tiendaIdRecuperada);
                model.TiendaId = tiendaIdRecuperada;
            }
        }

        // 3. Verificación final
        if (carrito == null || !carrito.Items.Any())
        {
            return RedirectToAction("Index", "Carrito");
        }

        // --- RECALCULAR ENVÍO POR SEGURIDAD ---
        decimal precioEnvio = 5.00m;
        decimal umbralGratis = 50.00m;
        decimal subtotal = carrito.Items.Sum(i => i.PrecioUnitario * i.Cantidad);
        model.GastosEnvio = (subtotal >= umbralGratis) ? 0 : precioEnvio;

        // Si el ID del modelo era 0 pero el carrito traía el ID correcto (porque lo recuperamos de sesión)
        if (model.TiendaId == 0 && carrito.TiendaId > 0)
        {
            model.TiendaId = carrito.TiendaId.Value;
        }

        if (!ModelState.IsValid)
        {
            model.Carrito = carrito;
            return View("Index", model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            // Usamos model.TiendaId que ya debería estar reparado arriba
            int pedidoId = await _pedidoService.CrearPedidoDesdeCarritoAsync(user.Id, model.TiendaId, carrito, model);

            await _carritoService.LimpiarCarritoAsync(model.TiendaId);

            return RedirectToAction("Gracias", new { id = pedidoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al procesar: " + ex.Message);
            model.Carrito = carrito;
            return View("Index", model);
        }
    }

    [HttpGet("gracias/{id}")]
    public async Task<IActionResult> Gracias(int id)
    {
        // Usamos el servicio para traer el pedido con sus items
        var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(id);

        if (pedido == null)
        {
            return NotFound();
        }

        // Opcional: Verificar que el pedido pertenezca al usuario conectado por seguridad
        var user = await _userManager.GetUserAsync(User);
        if (pedido.UserId != user.Id)
        {
            return Forbid();
        }

        return View(pedido);
    }
}