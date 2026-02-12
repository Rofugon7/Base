using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
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
            var query = _context.ProductoSimples
                .Include(p => p.Producto)
                .AsQueryable();

            // --- FILTROS ---
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(p => p.Producto.Nombre.Contains(nombre));

            if (precioMin.HasValue)
                query = query.Where(p => p.Producto.PrecioBase >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.Producto.PrecioBase <= precioMax.Value);

            if (!string.IsNullOrEmpty(activo) && activo != "todos")
            {
                bool esActivo = activo == "activos";
                query = query.Where(p => p.Producto.Activo == esActivo);
            }

            // --- ORDENACIÓN ---
            query = query.OrderBy(p => p.Producto.Nombre);

            // --- PAGINACIÓN ---
            int totalItems = await query.CountAsync();
            var productos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pasar datos a la vista para mantener el estado de los filtros
            ViewBag.FilterNombre = nombre;
            ViewBag.FilterPrecioMin = precioMin;
            ViewBag.FilterPrecioMax = precioMax;
            ViewBag.EstadoSeleccionado = activo;
            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lista de estados para el select
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
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .ThenInclude(p => p.Imagenes) // <-- Importante incluir las imágenes
                .FirstOrDefaultAsync(p => p.ProductoBaseId == id && p.Producto.Activo);

            if (producto == null) return NotFound();

            // Lógica de Productos Relacionados
            var relacionados = await _context.ProductoSimples
                .Include(p => p.Producto)
                .Where(p => p.Producto.CategoriaId == producto.Producto.CategoriaId && p.ProductoBaseId != id && p.Producto.Activo)
                .Take(4) // Limitamos a 4 para el diseño
                .ToListAsync();


            ViewBag.Relacionados = relacionados;
            ViewBag.ReturnUrl = returnUrl;
            ViewData["Title"] = producto.Producto.Nombre;

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
            if (!ModelState.IsValid)
            {
                vm.Categorias = await GetCategoriasAsync();
                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tiendaId = ObtenerTiendaIdUsuario();

                // 1. Crear el ProductoBase primero
                var productoBase = new ProductoBase
                {
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion,
                    PrecioBase = vm.PrecioBase,
                    TipoProducto = TipoProducto.Simple,
                    Activo = vm.Activo,
                    TiendaId = tiendaId,
                    CategoriaId = vm.CategoriaId,
                    SKU = vm.SKU,
                    ImagenPrincipal = "/imagenes/ProductoSinImagen.png"
                };

                _context.ProductosBase.Add(productoBase);
                await _context.SaveChangesAsync();

                // 2. Procesar SOLO las imágenes que vienen en el Request
                // Usamos Request.Form.Files para identificar exactamente qué slot envió qué archivo
                var archivos = Request.Form.Files;
                string principalPath = "/imagenes/ProductoSinImagen.png";

                for (int i = 0; i < 4; i++)
                {
                    // Buscamos el archivo que tenga el nombre exacto "Imagenes[0]", "Imagenes[1]", etc.
                    var file = archivos.GetFile($"Imagenes[{i}]");

                    if (file != null && file.Length > 0)
                    {
                        // Guardar archivo físico
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var rutaRelativa = "uploads/" + fileName;
                        bool esEstaPrincipal = (i == vm.PrincipalIndex);

                        // Insertar en ProductoImagenes
                        var nuevaImagen = new ProductoImagen
                        {
                            ProductoBaseId = productoBase.Id,
                            Ruta = rutaRelativa,
                            EsPrincipal = esEstaPrincipal
                        };
                        _context.ProductoImagenes.Add(nuevaImagen);

                        if (esEstaPrincipal)
                        {
                            principalPath = "/" + rutaRelativa;
                        }
                    }
                }

                // 3. Asignar la imagen principal al producto base y crear el ProductoSimple
                productoBase.ImagenPrincipal = principalPath;

                var productoSimple = new ProductoSimple
                {
                    ProductoBaseId = productoBase.Id,
                    Stock = vm.Stock
                };
                _context.ProductoSimples.Add(productoSimple);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error: " + ex.Message);
                vm.Categorias = await GetCategoriasAsync();
                return View(vm);
            }
        }

        // =========================
        // EDIT
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .Include(p => p.Producto.Imagenes)
                .FirstOrDefaultAsync(p => p.ProductoBaseId == id);

            if (producto == null)
                return NotFound();

            if (!PuedeGestionarProducto(producto.Producto.TiendaId))
                return Forbid();

            var imagenes = producto.Producto.Imagenes
                .OrderBy(i => i.Id)
                .ToList();

            var vm = new ProductoSimpleVM
            {
                ProductoBaseId = id,

                Nombre = producto.Producto.Nombre,
                Descripcion = producto.Producto.Descripcion,
                PrecioBase = producto.Producto.PrecioBase,
                Activo = producto.Producto.Activo,
                CategoriaId = producto.Producto.CategoriaId,
                SKU = producto.Producto.SKU,
                Stock = producto.Stock,

                Categorias = await GetCategoriasAsync(),

                ImagenesActuales = imagenes,
                SlotsIds = new List<int?>(),
                Imagenes = new List<IFormFile> { null, null, null, null }
            };

            // Rellenar slots (4 fijos)
            for (int i = 0; i < 4; i++)
            {
                if (i < imagenes.Count)
                    vm.SlotsIds.Add(imagenes[i].Id);
                else
                    vm.SlotsIds.Add(null);
            }

            vm.PrincipalIndex = imagenes.FindIndex(i => i.EsPrincipal);
            if (vm.PrincipalIndex < 0)
                vm.PrincipalIndex = 0;

            while (vm.SlotsIds.Count < 4)
            {
                vm.SlotsIds.Add(null);
            }

            return View(vm);
        }




        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoSimpleVM vm)
        {
            ModelState.Remove("ImagenPrincipal");
            if (!ModelState.IsValid)
            {
                vm.Categorias = await GetCategoriasAsync();
                return View(vm);
            }

            var productoBase = await _context.ProductosBase
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == vm.ProductoBaseId);

            var productoSimple = await _context.ProductoSimples
                .FirstOrDefaultAsync(p => p.ProductoBaseId == vm.ProductoBaseId);

            if (productoBase == null || productoSimple == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Actualizar datos básicos
                productoBase.Nombre = vm.Nombre;
                productoBase.Descripcion = vm.Descripcion;
                productoBase.PrecioBase = vm.PrecioBase;
                productoBase.SKU = vm.SKU;
                productoBase.CategoriaId = vm.CategoriaId;
                productoBase.Activo = vm.Activo;
                productoSimple.Stock = vm.Stock;

                // 2. Ejecutar Borrado explícito (Checkbox "Borrar")
                if (vm.ImagenesBorrar != null && vm.ImagenesBorrar.Any())
                {
                    foreach (var imgId in vm.ImagenesBorrar)
                    {
                        var img = productoBase.Imagenes.FirstOrDefault(x => x.Id == imgId);
                        if (img != null)
                        {
                            BorrarArchivoFisico(img.Ruta);
                            _context.ProductoImagenes.Remove(img);
                        }
                    }
                }

                // 3. Procesar Slots (Sustituir o Añadir)
                var archivos = Request.Form.Files;

                for (int i = 0; i < 4; i++)
                {
                    var file = archivos.GetFile($"Imagenes[{i}]");

                    if (file != null && file.Length > 0)
                    {
                        // ¿Había una imagen en este slot? (Sustitución)
                        if (i < vm.SlotsIds.Count && vm.SlotsIds[i].HasValue)
                        {
                            int idVieja = vm.SlotsIds[i].Value;
                            // Solo borramos si no estaba ya en la lista de ImagenesBorrar (para no duplicar delete)
                            var vieja = productoBase.Imagenes.FirstOrDefault(x => x.Id == idVieja);
                            if (vieja != null)
                            {
                                BorrarArchivoFisico(vieja.Ruta);
                                _context.ProductoImagenes.Remove(vieja);
                            }
                        }

                        // Guardar la nueva imagen
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.ProductoImagenes.Add(new ProductoImagen
                        {
                            ProductoBaseId = productoBase.Id,
                            Ruta = "uploads/" + fileName,
                            EsPrincipal = false // Se define al final
                        });
                    }
                }

                // Guardamos cambios intermedios para tener la lista de imágenes actualizada en BD
                await _context.SaveChangesAsync();

                // 4. Recalcular Imagen Principal
                // Recargamos de BD para asegurar que tenemos la lista real tras borrados y adiciones
                var imagenesFinales = await _context.ProductoImagenes
                    .Where(x => x.ProductoBaseId == productoBase.Id)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                foreach (var img in imagenesFinales) img.EsPrincipal = false;

                if (imagenesFinales.Any())
                {
                    // Intentamos usar el PrincipalIndex enviado
                    int pIdx = vm.PrincipalIndex ?? 0;

                    // Si el índice es mayor a lo que tenemos (ej. borraste imágenes), usamos la primera
                    if (pIdx < 0 || pIdx >= imagenesFinales.Count) pIdx = 0;

                    imagenesFinales[pIdx].EsPrincipal = true;
                    productoBase.ImagenPrincipal = "/" + imagenesFinales[pIdx].Ruta;
                }
                else
                {
                    productoBase.ImagenPrincipal = "/imagenes/ProductoSinImagen.png";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                vm.Categorias = await GetCategoriasAsync();
                return View(vm);
            }
        }


        private void BorrarArchivoFisico(string ruta)
        {
            var path = Path.Combine(_env.WebRootPath, ruta.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }




        // =========================
        // HELPERS
        // =========================
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
            return await _context.Categorias
                .Where(c => c.TiendaId == tiendaId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();
        }

        [HttpPost("desactivar/{id}")]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            // Buscamos el ProductoBase directamente, que es donde está el campo 'Activo'
            var productoBase = await _context.ProductosBase.FindAsync(id);

            if (productoBase != null)
            {
                // Verificamos permisos antes de actuar
                if (!PuedeGestionarProducto(productoBase.TiendaId)) return Forbid();

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
            // Buscamos directamente el ProductoBase usando el id
            var productoBase = await _context.ProductosBase.FindAsync(id);

            if (productoBase != null)
            {
                // 1. Verificación de seguridad (que el usuario sea dueño de la tienda)
                if (!PuedeGestionarProducto(productoBase.TiendaId)) return Forbid();

                // 2. Cambiamos el estado
                productoBase.Activo = true;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
