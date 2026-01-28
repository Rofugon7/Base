using BaseConLogin.Data;
using BaseConLogin.Services.Seo;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    [AllowAnonymous]
    [Route("t")]
    public class TiendasPublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITiendaContext _tiendaContext;
        private readonly ICanonicalService _canonical;

        public TiendasPublicController(
            ApplicationDbContext context,
            ITiendaContext tiendaContext,
    ICanonicalService canonical)
        {
            _context = context;
            _tiendaContext = tiendaContext;
            _canonical = canonical;
        }

        // /t/mi-tienda
        [HttpGet("{slug}")]
        public async Task<IActionResult> Entrar(string slug)
        {
            var tienda = await _context.Tiendas
                .FirstOrDefaultAsync(t => t.Slug == slug && t.Activa);

            if (tienda == null)
                return NotFound();

            ViewData["Title"] = "Entrar en la tienda";
            ViewData["Description"] =
                "selecciona la tienda disponible.";


            // 🔐 SOLO AQUÍ SE USA EL SLUG
            _tiendaContext.EstablecerTienda(tienda.Id, tienda.Slug);

            // Redirección interna limpia
            //return RedirectToAction("Index", "Home");
            return Redirect($"/t/{slug}");

        }
    }
}
