using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;
using BaseConLogin.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Controllers
{
    public class ProductoSimpleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductoSimpleController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create(
            string nombre,
            string? descripcion,
            decimal precioBase,
            string? imagenPrincipal,
            int stock)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("", "El nombre es obligatorio");
                return View();
            }

            // Imagen por defecto
            var imagenFinal = string.IsNullOrWhiteSpace(imagenPrincipal)
                ? "/imagenes/ProductoSinImagen.png"
                : imagenPrincipal;


            var tiendaId = ObtenerTiendaIdUsuario();


            // Producto base
            var productoBase = new ProductoBase
            {
                Nombre = nombre,
                Descripcion = descripcion,
                PrecioBase = precioBase,
                TipoProducto = TipoProducto.Simple,
                Activo = true,
                ImagenPrincipal = imagenFinal,
                TiendaId = tiendaId
            };

            _context.ProductosBase.Add(productoBase);
            await _context.SaveChangesAsync();


            // Producto simple
            var productoSimple = new ProductoSimple
            {
                ProductoBaseId = productoBase.Id,
                Stock = stock
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
            if (!EsAdmin() && producto.Producto.TiendaId != ObtenerTiendaIdUsuario())
                return Forbid();

            var vm = new ProductoSimpleEditVM
            {
                ProductoBaseId = producto.ProductoBaseId,
                Nombre = producto.Producto.Nombre,
                Descripcion = producto.Producto.Descripcion,
                PrecioBase = producto.Producto.PrecioBase,
                ImagenPrincipal = producto.Producto.ImagenPrincipal,
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

            if (!EsAdmin() && producto.Producto.TiendaId != ObtenerTiendaIdUsuario())
                return Forbid();

            // Actualizar campos
            producto.Producto.Nombre = vm.Nombre;
            producto.Producto.Descripcion = vm.Descripcion;
            producto.Producto.PrecioBase = vm.PrecioBase;
            producto.Producto.ImagenPrincipal = vm.ImagenPrincipal;
            producto.Producto.Activo = vm.Activo;
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


            if (!EsAdmin() && producto.Producto.TiendaId != ObtenerTiendaIdUsuario())
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


            if (!EsAdmin() && producto.Producto.TiendaId != ObtenerTiendaIdUsuario())
                return Forbid();


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


        private bool EsAdmin()
        {
            return User.IsInRole("Admin");
        }
    }
}
