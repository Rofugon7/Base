using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TiendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TiendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(bool mostrarInactivas = false)
        {
            IQueryable<Tienda> query = _context.Tiendas;

            if (mostrarInactivas)
            {
                query = query.IgnoreQueryFilters();
                
            }

            var tiendas = await query
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.MostrarInactivas = mostrarInactivas;

            return View(tiendas);
        }




        // =========================
        // CREATE
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tienda tienda)
        {
            if (!ModelState.IsValid)
                return View(tienda);

            _context.Tiendas.Add(tienda);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var tienda = await _context.Tiendas.FindAsync(id);
            if (tienda == null)
                return NotFound();

            return View(tienda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tienda tienda)
        {
            if (id != tienda.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(tienda);

            try
            {
                _context.Update(tienda);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TiendaExists(tienda.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // REACTIVAR TIENDA
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var tienda = await _context.Tiendas
                .IgnoreQueryFilters()  // <--- ignoramos el filtro global de Activa
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tienda == null)
                return NotFound();

            tienda.Activa = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // DESACTIVAR TIENDA
        // =========================

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
        // HELPERS
        // =========================
        private bool TiendaExists(int id)
        {
            return _context.Tiendas.Any(e => e.Id == id);
        }
    }
}
