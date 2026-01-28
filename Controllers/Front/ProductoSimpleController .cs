using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace BaseConLogin.Controllers
{
    public class ProductoSimpleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductoSimpleController(
    ApplicationDbContext context,
    IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // LISTAR (PÚBLICO)
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var productos = await _context.ProductoSimples
                .Include(p => p.Producto)
                .Where(p => p.Producto.Activo)
                .ToListAsync();

            return View(productos);
        }

        // =========================
        // DETALLE (PÚBLICO)
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p =>
                    p.ProductoBaseId == id &&
                    p.Producto.Activo);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // =========================
        // CREAR (ADMIN)
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoSimpleCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string rutaImagen = "/imagenes/ProductoSinImagen.png";

            // =============================
            // PROCESAR IMAGEN
            // =============================
            if (vm.Imagen != null && vm.Imagen.Length > 0)
            {
                // Validar tamaño (2MB)
                if (vm.Imagen.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("Imagen", "Máx 2MB");
                    return View(vm);
                }

                // Validar tipo
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var extension = Path.GetExtension(vm.Imagen.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("Imagen", "Formato no válido");
                    return View(vm);
                }

                // Generar nombre único
                var nombreArchivo = Guid.NewGuid() + extension;

                // Ruta física
                var rutaFisica = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    "productos",
                    nombreArchivo
                );

                // Guardar archivo
                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await vm.Imagen.CopyToAsync(stream);
                }

                // Ruta para BD
                rutaImagen = "/uploads/productos/" + nombreArchivo;
            }

            // =============================
            // GUARDAR PRODUCTO
            // =============================

            var tiendaId = ObtenerTiendaIdUsuario();

            var productoBase = new ProductoBase
            {
                Nombre = vm.Nombre,
                Descripcion = vm.Descripcion,
                PrecioBase = vm.PrecioBase,
                TipoProducto = TipoProducto.Simple,
                Activo = true,
                ImagenPrincipal = rutaImagen,
                TiendaId = tiendaId
            };

            _context.ProductosBase.Add(productoBase);
            await _context.SaveChangesAsync();

            var productoSimple = new ProductoSimple
            {
                ProductoBaseId = productoBase.Id,
                Stock = vm.Stock
            };

            _context.ProductoSimples.Add(productoSimple);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        // =========================
        // EDITAR (ADMIN)
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.ProductoBaseId == id);

            if (producto == null)
                return NotFound();

            // Seguridad: solo su tienda (excepto Admin)
            if (!PuedeGestionarProducto(producto.Producto.TiendaId))
                return Forbid();

            var vm = new ProductoSimpleEditVM
            {
                ProductoBaseId = producto.ProductoBaseId,
                Nombre = producto.Producto.Nombre,
                Descripcion = producto.Producto.Descripcion,
                PrecioBase = producto.Producto.PrecioBase,
                ImagenActual = producto.Producto.ImagenPrincipal,
                Activo = producto.Producto.Activo,
                Stock = producto.Stock
            };

            return View(vm);
        }




        [HttpPost]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoSimpleEditVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.ProductoBaseId == vm.ProductoBaseId);

            if (producto == null)
                return NotFound();

            if (!PuedeGestionarProducto(producto.Producto.TiendaId))
                return Forbid();

            // ===============================
            // GESTIÓN IMAGEN
            // ===============================

            string rutaImagen = vm.ImagenActual;

            if (vm.NuevaImagen != null && vm.NuevaImagen.Length > 0)
            {
                // 1️⃣ Borrar anterior
                if (!string.IsNullOrEmpty(vm.ImagenActual) &&
                    !vm.ImagenActual.Contains("ProductoSinImagen"))
                {
                    var rutaVieja = Path.Combine(
                        _env.WebRootPath,
                        vm.ImagenActual.TrimStart('/'));

                    if (System.IO.File.Exists(rutaVieja))
                        System.IO.File.Delete(rutaVieja);
                }

                // 2️⃣ Guardar nueva
                var carpeta = Path.Combine(
                    _env.WebRootPath,
                    "imagenes",
                    "productos");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = Guid.NewGuid().ToString()
                                    + Path.GetExtension(vm.NuevaImagen.FileName);

                var rutaFisica = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await vm.NuevaImagen.CopyToAsync(stream);
                }

                rutaImagen = "/imagenes/productos/" + nombreArchivo;
            }

            // ===============================
            // ACTUALIZAR DATOS
            // ===============================

            producto.Producto.Nombre = vm.Nombre;
            producto.Producto.Descripcion = vm.Descripcion;
            producto.Producto.PrecioBase = vm.PrecioBase;
            producto.Producto.Activo = vm.Activo;
            producto.Producto.ImagenPrincipal = rutaImagen;

            producto.Stock = vm.Stock;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        // =========================
        // ELIMINAR (ADMIN)
        // =========================
        [Authorize(Roles = "Admin,AdministradorTienda")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.ProductoBaseId == id);

            if (producto == null)
                return NotFound();


            if (!PuedeGestionarProducto(producto.Producto.TiendaId))
                return Forbid();


            return View(producto);
        }


        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Admin,AdministradorTienda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.ProductoSimples
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.ProductoBaseId == id);

            if (producto == null)
                return NotFound();


            if (!PuedeGestionarProducto(producto.Producto.TiendaId))
                return Forbid();

            // BORRAR IMAGEN
            var ruta = producto.Producto.ImagenPrincipal;

            if (!string.IsNullOrEmpty(ruta) &&
                !ruta.Contains("ProductoSinImagen"))
            {
                var path = Path.Combine(
                    _env.WebRootPath,
                    ruta.TrimStart('/'));

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }


            _context.ProductoSimples.Remove(producto);
            _context.ProductosBase.Remove(producto.Producto);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // HELPERS
        // =========================

        private int ObtenerTiendaIdUsuario()
        {
            var claim = User.FindFirst("TiendaId");

            if (claim == null)
                throw new Exception("Usuario sin tienda asociada");

            return int.Parse(claim.Value);
        }


   

        private bool PuedeGestionarProducto(int tiendaIdProducto)
        {
            // Admin → todo
            if (User.IsInRole("Admin"))
                return true;

            // Admin tienda → solo su tienda
            if (User.IsInRole("AdministradorTienda"))
                return tiendaIdProducto == ObtenerTiendaIdUsuario();

            return false;
        }
    }
}
