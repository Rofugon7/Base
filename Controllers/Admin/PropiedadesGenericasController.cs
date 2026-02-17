using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Admin
{
    public class PropiedadesGenericasController : Controller
    {

        private readonly ApplicationDbContext _context;

        public PropiedadesGenericasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var propiedades = await _context.PropiedadesGenericas.ToListAsync();

            // Categorías para el modal
            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre");

            // Productos para el modal (puedes filtrarlos por tienda)
            ViewBag.Productos = new SelectList(await _context.ProductosBase.ToListAsync(), "Id", "Nombre");

            return View("~/Views/PropiedadesGenericas/Index.cshtml", propiedades);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propiedad = await _context.PropiedadesGenericas.FindAsync(id);
            if (propiedad != null)
            {
                propiedad.IsDeleted = true; // Borrado lógico
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarAsignacion(int PropiedadGenericaId, int? CategoriaId, int? ProductoId)
        {
            var pg = await _context.PropiedadesGenericas.FindAsync(PropiedadGenericaId);
            if (pg == null) return NotFound();

            List<ProductoBase> productosDestino = new List<ProductoBase>();

            if (ProductoId.HasValue)
            {
                var p = await _context.ProductosBase.Include(x => x.PropiedadesExtendidas)
                    .FirstOrDefaultAsync(x => x.Id == ProductoId);
                if (p != null) productosDestino.Add(p);
            }
            else if (CategoriaId.HasValue)
            {
                productosDestino = await _context.ProductosBase.Include(x => x.PropiedadesExtendidas)
                    .Where(x => x.CategoriaId == CategoriaId).ToListAsync();
            }

            int asignados = 0;
            int omitidos = 0;

            foreach (var prod in productosDestino)
            {
                // VALIDACIÓN: ¿Ya tiene una propiedad con este nombre?
                bool yaExiste = prod.PropiedadesExtendidas
                    .Any(x => x.NombreEnProducto == pg.NombreEnProducto);

                if (!yaExiste)
                {
                    int maxOrden = prod.PropiedadesExtendidas.Any() ? prod.PropiedadesExtendidas.Max(x => x.Orden) : 0;

                    var nuevaProp = new ProductoPropiedadConfigurada
                    {
                        ProductoBaseId = prod.Id,
                        TiendaId = pg.TiendaId,
                        NombreEnProducto = pg.NombreEnProducto,
                        Valor = pg.Valor,
                        Operacion = pg.Operacion,
                        Orden = maxOrden + 1
                    };
                    _context.ProductoPropiedades.Add(nuevaProp);
                    asignados++;
                }
                else
                {
                    omitidos++;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Proceso finalizado. Asignados: {asignados}. Omitidos por duplicado: {omitidos}.";

            return RedirectToAction(nameof(Index));
        }

        private int ObtenerTiendaId()
        {
            // 1. Intentar obtenerlo de los Claims (si está logueado)
            var claim = User.FindFirst("TiendaId");
            if (claim != null && int.TryParse(claim.Value, out int tiendaIdFromClaim))
            {
                return tiendaIdFromClaim;
            }

            // 2. Si es invitado (o el claim no existe), devolver un ID por defecto
            // Aquí puedes poner el ID de tu tienda principal (ejemplo: 1)
            // Lo ideal es que esto venga de un servicio de contexto de tienda o configuración
            return 1;
        }

        // GET: PropiedadesGenericas/Create
        public IActionResult Create()
        {
            return View("~/Views/PropiedadesGenericas/Create.cshtml");
        }

        // POST: PropiedadesGenericas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropiedadGenerica propiedad)
        {
            ModelState.Remove("Tienda");
            ModelState.Remove("TiendaId");
            if (ModelState.IsValid)
            {
                // Asignamos el TiendaId actual (ajusta según tu método de obtener ID)
                propiedad.TiendaId = ObtenerTiendaId();
                propiedad.IsDeleted = false;

                _context.Add(propiedad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/PropiedadesGenericas/Create.cshtml", propiedad);
        }

        // GET: PropiedadesGenericas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var propiedad = await _context.PropiedadesGenericas.FindAsync(id);

            // Si no existe o está borrada lógicamente, no permitir editar
            if (propiedad == null || propiedad.IsDeleted) return NotFound();

            return View("~/Views/PropiedadesGenericas/Edit.cshtml", propiedad);
        }

        // POST: PropiedadesGenericas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropiedadGenerica propiedad)
        {
            if (id != propiedad.Id) return NotFound();

            ModelState.Remove("Tienda");
            ModelState.Remove("TiendaId");

            if (ModelState.IsValid)
            {
                try
                {
                    propiedad.TiendaId = ObtenerTiendaId();
                    // Marcamos como modificado pero aseguramos que IsDeleted siga false
                    propiedad.IsDeleted = false;
                    _context.Update(propiedad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PropiedadesGenericas.Any(e => e.Id == propiedad.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/PropiedadesGenericas/Edit.cshtml", propiedad);
        }
    }
}
