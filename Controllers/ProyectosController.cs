using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;


namespace BaseConLogin.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public ProyectosController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
            _userManager = userManager;
        }

        // GET: Proyectos
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var proyectos = from p in _context.Proyectos select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                proyectos = proyectos
                    .Include(p => p.ProyectoTags)
                    .ThenInclude(pt => pt.Tag)
                    .Where(s =>
                    (s.Nombre != null && s.Nombre.Contains(searchString)) ||
                    (s.Descripcion != null && s.Descripcion.Contains(searchString)) ||
                    (s.ProyectoTags.Any(pt => pt.Tag.Nombre.Contains(searchString)))
                );
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedList = proyectos.OrderByDescending(p => p.FechaInicio).ToPagedList(pageNumber, pageSize);

            // Obtener el usuario conectado
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user?.Id;

            return View(pagedList);
        }


        // GET: Proyectos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
        .Include(p => p.ProyectoTags)
        .ThenInclude(pt => pt.Tag)
        .FirstOrDefaultAsync(p => p.Id == id);

            if (proyecto == null) return NotFound();

            // Obtener el Id del usuario conectado con Identity
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user?.Id;

            // Buscar el nombre del autor usando el Id guardado en el proyecto
            var autorNombre = _context.Users
                .Where(u => u.Id == proyecto.Autor) // Comparas Id con el que está en el proyecto
                .Select(u => u.UserName) // O UserName si no tienes campo Nombre
                .FirstOrDefault();

            ViewBag.AutorNombre = autorNombre;

            return View(proyecto);
        }




        // GET: Proyectos/Create
        public IActionResult Create()
        {
            ViewBag.Tags = _context.Tags.ToList();
            return View();
        }

        // POST: Proyectos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Proyectos proyecto, int[] selectedTags, IFormFile imagenPortadaFile, IFormFile archivoPdfFile)
        {
            ModelState.Remove("Autor");
            ModelState.Remove("ProyectoTags");

            if (ModelState.IsValid)
            {
                // Obtener el usuario conectado
                var user = await _userManager.GetUserAsync(User);

                // Guardar el Id del usuario conectado en Autor
                proyecto.Autor = user.Id;

                await ProcesarArchivos(proyecto, imagenPortadaFile, archivoPdfFile);

                _context.Add(proyecto);
                await _context.SaveChangesAsync();


                foreach (var tagId in selectedTags)
                {
                    _context.ProyectoTags.Add(new ProyectoTag
                    {
                        ProyectoId = proyecto.Id,
                        TagId = tagId
                    });
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(proyecto);
        }



        // GET: Proyectos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos
        .Include(p => p.ProyectoTags)
        .ThenInclude(pt => pt.Tag).FirstOrDefaultAsync(p => p.Id == id); 
            if (proyecto == null) return NotFound();

            // Obtener el Id del usuario conectado con Identity
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user?.Id;

            // Lista de tags para el selector
            ViewBag.Tags = _context.Tags.ToList();

            // Array con IDs de los tags que tiene este proyecto
            ViewBag.SelectedTags = proyecto.ProyectoTags.Select(pt => pt.TagId).ToArray();

            return View(proyecto);
        }

        // POST: Proyectos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Proyectos proyecto, int[] selectedTags, IFormFile imagenPortadaFile, IFormFile archivoPdfFile)
        {
            if (id != proyecto.Id)
                return NotFound();

            if (imagenPortadaFile == null)
                ModelState.Remove("ImagenPortadaFile");

            if (archivoPdfFile == null)
                ModelState.Remove("ArchivoPdfFile");

            ModelState.Remove("ProyectoTags");

            if (ModelState.IsValid)
            {
                try
                {
                    // Cargar el proyecto original CON tags
                    var proyectoOriginal = await _context.Proyectos
                        .Include(p => p.ProyectoTags)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (proyectoOriginal == null)
                        return NotFound();

                    // Procesar archivos (si hay)
                    await ProcesarArchivos(proyecto, imagenPortadaFile, archivoPdfFile);

                    // Copiar los campos actualizables
                    proyectoOriginal.Nombre = proyecto.Nombre;
                    proyectoOriginal.Descripcion = proyecto.Descripcion;
                    proyectoOriginal.Aportaciones = proyecto.Aportaciones;
                    proyectoOriginal.FechaInicio = proyecto.FechaInicio;
                    proyectoOriginal.FechaFin = proyecto.FechaFin;

                    if (!string.IsNullOrEmpty(proyecto.ImagenPortada))
                        proyectoOriginal.ImagenPortada = proyecto.ImagenPortada;

                    if (!string.IsNullOrEmpty(proyecto.ArchivoPdf))
                        proyectoOriginal.ArchivoPdf = proyecto.ArchivoPdf;

                    // Actualizar tags
                    proyectoOriginal.ProyectoTags.Clear();
                    foreach (var tagId in selectedTags)
                    {
                        proyectoOriginal.ProyectoTags.Add(new ProyectoTag
                        {
                            ProyectoId = proyectoOriginal.Id,
                            TagId = tagId
                        });
                    }

                    _context.Update(proyectoOriginal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProyectosExists(proyecto.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(proyecto);
        }



        // GET: Proyectos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proyecto = await _context.Proyectos.FirstOrDefaultAsync(m => m.Id == id);
            if (proyecto == null) return NotFound();

            return View(proyecto);
        }

        // POST: Proyectos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        private bool ProyectosExists(int id)
        {
            return _context.Proyectos.Any(e => e.Id == id);
        }

        private async Task ProcesarArchivos(Proyectos proyecto, IFormFile imagenPortadaFile, IFormFile archivoPdfFile)
        {
            var carpetaImagenes = _configuration["Carpetas:Imagenes"] ?? "imagenes";
            var carpetaPdfs = _configuration["Carpetas:Pdf"] ?? "pdfs";

            string rutaImagenes = Path.Combine(_env.WebRootPath, carpetaImagenes);
            string rutaPdfs = Path.Combine(_env.WebRootPath, carpetaPdfs);

            Directory.CreateDirectory(rutaImagenes);
            Directory.CreateDirectory(rutaPdfs);

            if (imagenPortadaFile != null && imagenPortadaFile.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imagenPortadaFile.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("ImagenPortada", "Formato de imagen no permitido (jpg, jpeg, png, gif).");
                    return;
                }

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaFisica = Path.Combine(rutaImagenes, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await imagenPortadaFile.CopyToAsync(stream);
                }

                // Guarda solo la ruta relativa para que funcione en las vistas
                proyecto.ImagenPortada = $"{carpetaImagenes}/{nombreArchivo}".Replace("\\", "/");
            }

            if (archivoPdfFile != null && archivoPdfFile.Length > 0)
            {
                var extension = Path.GetExtension(archivoPdfFile.FileName).ToLowerInvariant();
                if (extension != ".pdf")
                {
                    ModelState.AddModelError("ArchivoPdf", "Solo se permiten archivos PDF.");
                    return;
                }

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaFisica = Path.Combine(rutaPdfs, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await archivoPdfFile.CopyToAsync(stream);
                }

                proyecto.ArchivoPdf = $"{carpetaPdfs}/{nombreArchivo}".Replace("\\", "/");
            }
        }
    }
}
