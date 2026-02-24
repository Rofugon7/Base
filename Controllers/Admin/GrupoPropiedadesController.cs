using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Admin
{   

    [Authorize(Roles = "Admin,AdministradorTienda")]
    public class GrupoPropiedadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrupoPropiedadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int ObtenerTiendaId() => int.Parse(User.FindFirst("TiendaId")?.Value ?? "0");

        // LISTADO
        public async Task<IActionResult> Index()
        {
            var tiendaId = ObtenerTiendaId();
            var grupos = await _context.GrupoPropiedades
                .Where(g => g.TiendaId == tiendaId)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
            return View(grupos);
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return RedirectToAction(nameof(Index));

            var tiendaId = ObtenerTiendaId();

            var nuevoGrupo = new GrupoPropiedad
            {
                Nombre = nombre.Trim(),
                TiendaId = tiendaId
            };

            _context.GrupoPropiedades.Add(nuevoGrupo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tiendaId = ObtenerTiendaId();
            var grupo = await _context.GrupoPropiedades
                .FirstOrDefaultAsync(g => g.Id == id && g.TiendaId == tiendaId);

            if (grupo != null)
            {
                // Opcional: Validar si hay productos usando este grupo antes de borrar
                _context.GrupoPropiedades.Remove(grupo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
