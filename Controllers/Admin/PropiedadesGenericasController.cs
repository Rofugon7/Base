using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Authorization;
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
            // Incluimos el GrupoPropiedad si existe la relación en tu modelo
            var propiedades = await _context.PropiedadesGenericas.Where(p => !p.IsDeleted).ToListAsync();

            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nombre");
            ViewBag.Productos = new SelectList(await _context.ProductosBase.ToListAsync(), "Id", "Nombre");

            return View("~/Views/PropiedadesGenericas/Index.cshtml", propiedades);
        }

        // =========================
        // CREATE
        // =========================
        public async Task<IActionResult> Create()
        {
            await CargarGrupos(); // Llenar ViewBag.Grupos
            return View("~/Views/PropiedadesGenericas/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropiedadGenerica propiedad)
        {
            ModelState.Remove("Tienda");
            ModelState.Remove("TiendaId");

            if (ModelState.IsValid)
            {
                propiedad.TiendaId = ObtenerTiendaId();
                propiedad.IsDeleted = false;

                _context.Add(propiedad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarGrupos(); // Recargar en caso de error
            return View("~/Views/PropiedadesGenericas/Create.cshtml", propiedad);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var propiedad = await _context.PropiedadesGenericas.FindAsync(id);
            if (propiedad == null || propiedad.IsDeleted) return NotFound();

            await CargarGrupos(); // Importante para el dropdown de edición
            return View("~/Views/PropiedadesGenericas/Edit.cshtml", propiedad);
        }

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
            await CargarGrupos();
            return View("~/Views/PropiedadesGenericas/Edit.cshtml", propiedad);
        }

        // =========================
        // PROCESAR ASIGNACIÓN (CORREGIDO PARA GRUPOS)
        // =========================
        [HttpPost]
        public async Task<IActionResult> ProcesarAsignacion(int PropiedadGenericaId, int? CategoriaId, int? ProductoId)
        {
            var pg = await _context.PropiedadesGenericas.FindAsync(PropiedadGenericaId);
            if (pg == null) return NotFound();

            // 1. Obtener el nombre del grupo basado en el ID seleccionado en la Propiedad Genérica
            string nombreGrupo = null;
            if (pg.GrupoPropiedadId.HasValue)
            {
                var grupo = await _context.GrupoPropiedades.FindAsync(pg.GrupoPropiedadId.Value);
                nombreGrupo = grupo?.Nombre;
            }

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
                // 2. CORRECCIÓN: Usamos NombreEnProducto que es el campo real de tu modelo
                // Comprobamos si el producto ya tiene esta propiedad dentro del MISMO grupo
                bool yaExiste = prod.PropiedadesExtendidas
                    .Any(x => x.NombrePropiedad == pg.NombreEnProducto && x.NombreEnProducto == nombreGrupo);

                if (!yaExiste)
                {
                    int maxOrden = prod.PropiedadesExtendidas.Any() ? prod.PropiedadesExtendidas.Max(x => x.Orden) : 0;

                    var nuevaProp = new ProductoPropiedadConfigurada
                    {
                        ProductoBaseId = prod.Id,
                        TiendaId = prod.TiendaId,
                        NombreEnProducto = nombreGrupo,      // El nombre del Grupo (ej: "Papel")
                        NombrePropiedad = pg.NombreEnProducto, // El nombre de la opción (ej: "Kraft")
                        Valor = pg.Valor,
                        Operacion = pg.Operacion,
                        Orden = maxOrden + 1,
                        EsConfigurable = pg.EsConfigurable
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
            TempData["Success"] = $"Proceso finalizado. Asignados: {asignados}. Omitidos: {omitidos}.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS
        // =========================
        private async Task CargarGrupos()
        {
            var tiendaId = ObtenerTiendaId();
            var listaGrupos = await _context.GrupoPropiedades
                .Where(g => g.TiendaId == tiendaId)
                .OrderBy(g => g.Nombre)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Nombre
                }).ToListAsync();

            ViewBag.Grupos = listaGrupos;
        }

        private int ObtenerTiendaId()
        {
            var claim = User.FindFirst("TiendaId");
            return claim != null ? int.Parse(claim.Value) : 1;
        }
    }
}