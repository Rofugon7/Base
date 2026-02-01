using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers
{
    [Authorize(Roles = "Admin,AdministradorTienda")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .ToListAsync();

            // Filtrado por tienda si no es Admin
            if (!User.IsInRole("Admin"))
            {
                var tiendaId = int.Parse(User.FindFirst("TiendaId")!.Value);
                categorias = categorias.Where(c => c.TiendaId == tiendaId).ToList();
            }

            return View(categorias);
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
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!ModelState.IsValid)
                return View(categoria);

            var tiendaId = ObtenerTiendaIdUsuario();

            var categ = new Categoria
            {
                Nombre = categoria.Nombre,
                TiendaId = tiendaId,
                Descripcion = categoria.Descripcion 
            };

            categ.FechaAlta = DateTime.UtcNow;
            _context.Categorias.Add(categ);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            if (!User.IsInRole("Admin") && categoria.TiendaId != int.Parse(User.FindFirst("TiendaId")!.Value))
                return Forbid();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(categoria);

            var dbCategoria = await _context.Categorias.FindAsync(id);
            if (dbCategoria == null)
                return NotFound();

            if (!User.IsInRole("Admin") && dbCategoria.TiendaId != int.Parse(User.FindFirst("TiendaId")!.Value))
                return Forbid();

            dbCategoria.Nombre = categoria.Nombre;
            dbCategoria.Descripcion = categoria.Descripcion;
            dbCategoria.Activa = categoria.Activa;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            if (!User.IsInRole("Admin") && categoria.TiendaId != int.Parse(User.FindFirst("TiendaId")!.Value))
                return Forbid();

            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            if (!User.IsInRole("Admin") && categoria.TiendaId != int.Parse(User.FindFirst("TiendaId")!.Value))
                return Forbid();

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private int ObtenerTiendaIdUsuario()
        {
            var claim = User.FindFirst("TiendaId");

            if (claim == null)
                throw new Exception("Usuario sin tienda asociada");

            return int.Parse(claim.Value);
        }

    }
}
