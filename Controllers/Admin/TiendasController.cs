using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/tiendas")]
    public class TiendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TiendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tiendas
                .IgnoreQueryFilters()
                .ToListAsync());
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tienda tienda)
        {
            if (!ModelState.IsValid)
                return View(tienda);

            _context.Tiendas.Add(tienda);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var tienda = await _context.Tiendas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tienda == null)
                return NotFound();

            return View(tienda);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tienda tienda)
        {
            if (id != tienda.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(tienda);

            _context.Update(tienda);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
