using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Seo;
using BaseConLogin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    [Route("tags")]
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICanonicalService _canonical;

        public TagsController(ApplicationDbContext context,
    ICanonicalService canonical)
        {
            _context = context;
            _canonical = canonical;
        }

        // GET: Tags
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Canonical"] = _canonical.Build("tags");
            ViewData["Title"] = "Proyectos disponibles | BaseConLogin";
            ViewData["Description"] =
                "Explora nuestros proyectos activos realizados por distintas tiendas y profesionales.";


            var tags = await _context.Tags
                .Select(t => new TagViewModel
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    EstaEnUso = _context.ProyectoTags.Any(pt => pt.TagId == t.Id)
                })
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return View(tags);
        }


        // GET: Tags/Details/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            ViewData["Canonical"] = _canonical.Build($"tags/{id}");




            var tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tag == null)
                return NotFound();

            ViewData["Title"] =
        $"Proyectos sobre {tag.Nombre} | Etiqueta";

            ViewData["Description"] =
                $"Explora proyectos relacionados con {tag.Nombre}. " +
                $"Encuentra trabajos, ideas y profesionales especializados.";

            return View(tag);
        }

        // GET: Tags/Create
        [HttpGet("crear")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Crear etiqueta";
            ViewData["Description"] = "Crear una nueva etiqueta para clasificar proyectos.";
            return View();
        }

        // POST: Tags/Create
        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Nombre")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                tag.Slug = SlugService.Generar(tag.Nombre);
                ViewData["Title"] = "Crear etiqueta";
                ViewData["Description"] = "Crear una nueva etiqueta para clasificar proyectos.";
                _context.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound();

            ViewData["Title"] = "Editar etiqueta";
            ViewData["Description"] =
                $"Editar la etiqueta {tag.Nombre}.";

            return View(tag);
        }

        // POST: Tags/Edit/5
        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Tag tag)
        {
            if (id != tag.Id)
                return NotFound();
            ViewData["Title"] = "Editar etiqueta";
            ViewData["Description"] =
                $"Editar la etiqueta {tag.Nombre}.";

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet("eliminar/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tag == null)
                return NotFound();

            ViewData["Title"] = "Eliminar etiqueta";
            ViewData["Description"] =
                $"Eliminar la etiqueta {tag.Nombre}.";

            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost("eliminar/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                ViewData["Title"] = "Eliminar etiqueta";
                ViewData["Description"] =
                    $"Eliminar la etiqueta {tag.Nombre}.";

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
