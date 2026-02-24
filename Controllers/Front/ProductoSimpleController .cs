using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers.Front
{
    public class ProductoSimpleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductoSimpleController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // INDEX
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Index(string nombre, decimal? precioMin, decimal? precioMax, string activo, int page = 1)
        {
            int pageSize = 15;
            var query = _context.ProductosBase
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadesExtendidas)
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre));

            if (precioMin.HasValue)
                query = query.Where(p => p.PrecioBase >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.PrecioBase <= precioMax.Value);

            if (!string.IsNullOrEmpty(activo) && activo != "todos")
            {
                bool esActivo = activo == "activos";
                query = query.Where(p => p.Activo == esActivo);
            }

            query = query.OrderBy(p => p.Nombre);

            int totalItems = await query.CountAsync();
            var productos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.FilterNombre = nombre;
            ViewBag.FilterPrecioMin = precioMin;
            ViewBag.FilterPrecioMax = precioMax;
            ViewBag.EstadoSeleccionado = activo;
            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.Estados = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todos", Value = "todos" },
                new SelectListItem { Text = "Activos", Value = "activos" },
                new SelectListItem { Text = "Inactivos", Value = "inactivos" }
            };

            return View(productos);
        }

        // =========================
        // DETAILS
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id, string returnUrl)
        {
            var producto = await _context.ProductosBase
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadesExtendidas)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            var todasLasProps = producto.PropiedadesExtendidas.OrderBy(p => p.Orden).ToList();

            var costesInternos = todasLasProps.Where(p => !p.EsConfigurable).ToList();
            var opcionesCliente = todasLasProps.Where(p => p.EsConfigurable).ToList();

            decimal precioPartida = producto.PrecioBase;
            foreach (var coste in costesInternos)
            {
                if (coste.Operacion == "Suma") precioPartida += coste.Valor;
                else if (coste.Operacion == "Multiplicacion") precioPartida *= coste.Valor;
                else if (coste.Operacion == "Resta") precioPartida -= coste.Valor;
            }

            var relacionados = await _context.ProductosBase
                .Where(p => p.CategoriaId == producto.CategoriaId && p.Id != id && p.Activo)
                .Take(4)
                .ToListAsync();

            ViewBag.PrecioPartida = precioPartida;
            ViewBag.OpcionesCliente = opcionesCliente.GroupBy(p => p.NombreEnProducto).ToList();
            ViewBag.Relacionados = relacionados;
            ViewBag.ReturnUrl = returnUrl;
            ViewData["Title"] = producto.Nombre;

            return View(producto);
        }

        // =========================
        // CREATE
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductoSimpleVM
            {
                Categorias = await GetCategoriasAsync(),
                GruposDisponibles = await GetGruposAsync(),
                Imagenes = new List<IFormFile> { null, null, null, null },
                ImagenesActuales = new(),
                PrincipalIndex = 0
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoSimpleVM vm)
        {
            ModelState.Remove("ImagenPrincipal");

            // Validar Regla de Negocio: Grupo obligatorio si es configurable
            if (vm.Propiedades != null)
            {
                for (int i = 0; i < vm.Propiedades.Count; i++)
                {
                    if (vm.Propiedades[i].EsConfigurable && !vm.Propiedades[i].GrupoPropiedadId.HasValue)
                    {
                        ModelState.AddModelError($"Propiedades[{i}].GrupoPropiedadId", "El grupo es obligatorio para opciones configurables.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Categorias = await GetCategoriasAsync();
                vm.GruposDisponibles = await GetGruposAsync();
                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tiendaId = ObtenerTiendaIdUsuario();

                var producto = new ProductoBase
                {
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion,
                    PrecioBase = vm.PrecioBase,
                    Stock = vm.Stock,
                    Activo = vm.Activo,
                    TiendaId = tiendaId,
                    CategoriaId = vm.CategoriaId,
                    SKU = vm.SKU,
                    TipoProducto = vm.TipoProducto,
                    ImagenPrincipal = "/imagenes/ProductoSinImagen.png",
                    FechaAlta = DateTime.UtcNow
                };

                _context.ProductosBase.Add(producto);
                await _context.SaveChangesAsync();

                // Imágenes
                var archivos = Request.Form.Files;
                string principalPath = "/imagenes/ProductoSinImagen.png";

                for (int i = 0; i < 4; i++)
                {
                    var file = archivos.GetFile($"Imagenes[{i}]");
                    if (file != null && file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await file.CopyToAsync(stream); }

                        var rutaRelativa = "uploads/" + fileName;
                        bool esEstaPrincipal = (i == vm.PrincipalIndex);

                        _context.ProductoImagenes.Add(new ProductoImagen
                        {
                            ProductoBaseId = producto.Id,
                            Ruta = rutaRelativa,
                            EsPrincipal = esEstaPrincipal
                        });
                        if (esEstaPrincipal) principalPath = "/" + rutaRelativa;
                    }
                }
                producto.ImagenPrincipal = principalPath;

                // Propiedades con Selección de Grupo
                if (vm.Propiedades != null)
                {
                    foreach (var prop in vm.Propiedades)
                    {
                        string nombreGrupoStr = null;
                        if (prop.GrupoPropiedadId.HasValue)
                        {
                            var g = await _context.GrupoPropiedades.FindAsync(prop.GrupoPropiedadId.Value);
                            nombreGrupoStr = g?.Nombre;
                        }

                        _context.ProductoPropiedades.Add(new ProductoPropiedadConfigurada
                        {
                            ProductoBaseId = producto.Id,
                            TiendaId = tiendaId,
                            NombreEnProducto = nombreGrupoStr ?? "General", // Guardamos el texto del grupo
                            NombrePropiedad = prop.NombrePropiedad,
                            Valor = prop.Valor,
                            Operacion = prop.Operacion,
                            Orden = prop.Orden,
                            EsConfigurable = prop.EsConfigurable
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error: " + ex.Message);
                vm.Categorias = await GetCategoriasAsync();
                vm.GruposDisponibles = await GetGruposAsync();
                return View(vm);
            }
        }

        // =========================
        // EDIT
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.ProductosBase
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadesExtendidas)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);



            if (producto == null) return NotFound();
            if (!PuedeGestionarProducto(producto.TiendaId)) return Forbid();

            var imagenes = producto.Imagenes.OrderBy(i => i.Id).ToList();
            var grupos = await GetGruposAsync();

            var vm = new ProductoSimpleVM
            {
                ProductoBaseId = id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioBase = producto.PrecioBase,
                Activo = producto.Activo,
                CategoriaId = producto.CategoriaId,
                SKU = producto.SKU,
                Stock = producto.Stock,
                TipoProducto = producto.TipoProducto,
                Categorias = await GetCategoriasAsync(),
                GruposDisponibles = grupos,
                ImagenesActuales = imagenes,
                SlotsIds = new List<int?>(),
                Propiedades = producto.PropiedadesExtendidas.OrderBy(p => p.Orden).Select(p => new PropiedadFilaVM
                {
                    Id = p.Id,
                    NombreEnProducto = p.NombreEnProducto,
                    NombrePropiedad = p.NombrePropiedad,
                    Valor = p.Valor,
                    Operacion = p.Operacion,
                    Orden = p.Orden,
                    EsConfigurable = p.EsConfigurable,
                    // IMPORTANTE: Buscamos el ID del grupo que coincida con el nombre guardado 
                    // para que el select marque la opción correcta.
                    GrupoPropiedadId = grupos.FirstOrDefault(g => g.Text == p.NombreEnProducto)?.Value != null
                               ? int.Parse(grupos.First(g => g.Text == p.NombreEnProducto).Value)
                               : null
                }).ToList()
            };

            for (int i = 0; i < 4; i++) vm.SlotsIds.Add(i < imagenes.Count ? imagenes[i].Id : null);
            vm.PrincipalIndex = imagenes.FindIndex(i => i.EsPrincipal);
            if (vm.PrincipalIndex < 0) vm.PrincipalIndex = 0;

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoSimpleVM vm)
        {
            ModelState.Remove("ImagenPrincipal");

            if (vm.Propiedades != null)
            {
                for (int i = 0; i < vm.Propiedades.Count; i++)
                {
                    if (vm.Propiedades[i].EsConfigurable && !vm.Propiedades[i].GrupoPropiedadId.HasValue)
                    {
                        ModelState.AddModelError($"Propiedades[{i}].GrupoPropiedadId", "El grupo es obligatorio para opciones configurables.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Categorias = await GetCategoriasAsync();
                vm.GruposDisponibles = await GetGruposAsync();
                var pImg = await _context.ProductosBase.Include(x => x.Imagenes).FirstOrDefaultAsync(x => x.Id == vm.ProductoBaseId);
                vm.ImagenesActuales = pImg?.Imagenes.OrderBy(i => i.Id).ToList() ?? new();
                return View(vm);
            }

            var productoBase = await _context.ProductosBase
                .Include(p => p.Imagenes)
                .Include(p => p.PropiedadesExtendidas)
                .FirstOrDefaultAsync(p => p.Id == vm.ProductoBaseId);

            if (productoBase == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                productoBase.Nombre = vm.Nombre;
                productoBase.Descripcion = vm.Descripcion;
                productoBase.PrecioBase = vm.PrecioBase;
                productoBase.SKU = vm.SKU;
                productoBase.CategoriaId = vm.CategoriaId;
                productoBase.Activo = vm.Activo;
                productoBase.Stock = vm.Stock;

                // Gestión de Imágenes
                if (vm.ImagenesBorrar != null)
                {
                    foreach (var imgId in vm.ImagenesBorrar)
                    {
                        var img = productoBase.Imagenes.FirstOrDefault(x => x.Id == imgId);
                        if (img != null) { BorrarArchivoFisico(img.Ruta); _context.ProductoImagenes.Remove(img); }
                    }
                }

                var archivos = Request.Form.Files;
                for (int i = 0; i < 4; i++)
                {
                    var file = archivos.GetFile($"Imagenes[{i}]");
                    if (file != null && file.Length > 0)
                    {
                        if (i < vm.SlotsIds.Count && vm.SlotsIds[i].HasValue)
                        {
                            var vieja = productoBase.Imagenes.FirstOrDefault(x => x.Id == vm.SlotsIds[i]);
                            if (vieja != null) { BorrarArchivoFisico(vieja.Ruta); _context.ProductoImagenes.Remove(vieja); }
                        }
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                        using (var stream = new FileStream(path, FileMode.Create)) { await file.CopyToAsync(stream); }
                        _context.ProductoImagenes.Add(new ProductoImagen { ProductoBaseId = productoBase.Id, Ruta = "uploads/" + fileName, EsPrincipal = false });
                    }
                }

                await _context.SaveChangesAsync();

                // Sincronización de PROPIEDADES
                var idsEnviados = vm.Propiedades?.Where(p => p.Id > 0).Select(p => p.Id).ToList() ?? new List<int>();
                var aEliminar = productoBase.PropiedadesExtendidas.Where(p => !idsEnviados.Contains(p.Id)).ToList();
                _context.ProductoPropiedades.RemoveRange(aEliminar);

                if (vm.Propiedades != null)
                {
                    foreach (var p in vm.Propiedades)
                    {
                        string nombreGrupoStr = null;
                        if (p.GrupoPropiedadId.HasValue)
                        {
                            var g = await _context.GrupoPropiedades.FindAsync(p.GrupoPropiedadId.Value);
                            nombreGrupoStr = g?.Nombre;
                        }

                        if (p.Id == 0)
                        {
                            productoBase.PropiedadesExtendidas.Add(new ProductoPropiedadConfigurada
                            {
                                NombreEnProducto = nombreGrupoStr ?? "General",
                                NombrePropiedad = p.NombrePropiedad,
                                Valor = p.Valor,
                                Operacion = p.Operacion,
                                Orden = p.Orden,
                                EsConfigurable = p.EsConfigurable,
                                TiendaId = productoBase.TiendaId
                            });
                        }
                        else
                        {
                            var pDb = productoBase.PropiedadesExtendidas.FirstOrDefault(x => x.Id == p.Id);
                            if (pDb != null)
                            {
                                pDb.NombreEnProducto = nombreGrupoStr ?? "General";
                                pDb.NombrePropiedad = p.NombrePropiedad;
                                pDb.Valor = p.Valor;
                                pDb.Operacion = p.Operacion;
                                pDb.Orden = p.Orden;
                                pDb.EsConfigurable = p.EsConfigurable;
                            }
                        }
                    }
                }

                // Actualizar Imagen Principal
                var imagenesFinales = await _context.ProductoImagenes.Where(x => x.ProductoBaseId == productoBase.Id).OrderBy(x => x.Id).ToListAsync();
                foreach (var img in imagenesFinales) img.EsPrincipal = false;
                if (imagenesFinales.Any())
                {
                    int pIdx = vm.PrincipalIndex ?? 0;
                    if (pIdx >= imagenesFinales.Count) pIdx = 0;
                    imagenesFinales[pIdx].EsPrincipal = true;
                    productoBase.ImagenPrincipal = "/" + imagenesFinales[pIdx].Ruta;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error: " + ex.Message);
                vm.Categorias = await GetCategoriasAsync();
                vm.GruposDisponibles = await GetGruposAsync();
                return View(vm);
            }
        }

        // =========================
        // HELPERS
        // =========================
        private void BorrarArchivoFisico(string ruta)
        {
            var path = Path.Combine(_env.WebRootPath, ruta.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        private int ObtenerTiendaIdUsuario()
        {
            var claim = User.FindFirst("TiendaId") ?? throw new Exception("Usuario sin tienda");
            return int.Parse(claim.Value);
        }

        private bool PuedeGestionarProducto(int tiendaIdProducto)
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("AdministradorTienda") && tiendaIdProducto == ObtenerTiendaIdUsuario()) return true;
            return false;
        }

        private async Task<List<SelectListItem>> GetCategoriasAsync()
        {
            var tiendaId = ObtenerTiendaIdUsuario();
            return await _context.Categorias.Where(c => c.TiendaId == tiendaId)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre }).ToListAsync();
        }

        private async Task<List<SelectListItem>> GetGruposAsync()
        {
            var tiendaId = ObtenerTiendaIdUsuario();
            return await _context.GrupoPropiedades.Where(g => g.TiendaId == tiendaId)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nombre }).ToListAsync();
        }

        [HttpPost("desactivar/{id}")]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var productoBase = await _context.ProductosBase.FindAsync(id);
            if (productoBase != null && PuedeGestionarProducto(productoBase.TiendaId))
            {
                productoBase.Activo = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("activar/{id}")]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var productoBase = await _context.ProductosBase.FindAsync(id);
            if (productoBase != null && PuedeGestionarProducto(productoBase.TiendaId))
            {
                productoBase.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}