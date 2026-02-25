using BaseConLogin.Services.Carritos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BaseConLogin.ViewComponents // Cambiado a plural para evitar conflictos
{
    public class CartPreviewViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly ICarritoService _carritoService;

        public CartPreviewViewComponent(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 1. Obtener el TiendaId del usuario actual
            var user = User as ClaimsPrincipal;
            var tiendaIdClaim = user?.FindFirst("TiendaId")?.Value;
            int tiendaId = string.IsNullOrEmpty(tiendaIdClaim) ? 0 : int.Parse(tiendaIdClaim);

            // 2. IMPORTANTE: Usar 'await' y pasar el 'tiendaId'
            // Esto soluciona los errores de Task<CarritoSession>
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            // 3. Pasamos el carrito directamente a la vista (Default.cshtml)
            // Asegúrate de que la vista use @model BaseConLogin.Models.CarritoSession
            return View(carrito);
        }
    }
}