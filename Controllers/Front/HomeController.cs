using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Seo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var tiendaSlug = "mi-tienda"; // obtener dinámicamente de ITiendaContext
            ViewData["Canonical"] = _canonical.Build($"t/{tiendaSlug}");

            ViewData["Title"] = $"Home de {tiendaSlug}";
            ViewData["Description"] =
                $"pagina principal de  {tiendaSlug}.";

            var proyectos = await _context.Proyectos
                .Include(p => p.Producto)
                .ThenInclude(p => p.Tienda) // asegura filtro global
                .Where(p => p.Revisado)
                .ToListAsync();

            return View(proyectos);
        }
    }
}
