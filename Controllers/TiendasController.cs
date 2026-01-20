using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers
{
    public class TiendasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarritoService _carritoService;
        private readonly ITiendaContext _tiendaContext;

        public TiendasController(
            ApplicationDbContext context,
            ICarritoService carritoService,
            ITiendaContext tiendaContext)
        {
            _context = context;
            _carritoService = carritoService;
            _tiendaContext = tiendaContext;
        }

        // =========================
        // ENTRAR EN TIENDA
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Index(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var tienda = await _context.Tiendas
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tienda == null)
                return NotFound();

            // 👉 Establecer tienda activa
            _tiendaContext.EstablecerTienda(tienda.Id);

            // 👉 Obtener carrito de ESA tienda
            var carrito = await _carritoService.ObtenerCarritoAsync(tienda.Id);

            return View(carrito);
        }

        // =========================
        // CREATE
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tienda tienda)
        {
            if (!ModelState.IsValid)
                return View(tienda);

            _context.Tiendas.Add(tienda);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { slug = tienda.Slug });
        }

        // =========================
        // EDIT
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tienda = await _context.Tiendas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tienda == null)
                return NotFound();

            return View(tienda);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tienda tienda)
        {
            if (id != tienda.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(tienda);

            _context.Update(tienda);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { slug = tienda.Slug });
        }

        // =========================
        // DESACTIVAR
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var tienda = await _context.Tiendas.FindAsync(id);
            if (tienda == null)
                return NotFound();

            tienda.Activa = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REACTIVAR
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var tienda = await _context.Tiendas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tienda == null)
                return NotFound();

            tienda.Activa = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { slug = tienda.Slug });
        }
    }
}
