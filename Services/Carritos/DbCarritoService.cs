using System.Text.Json;
using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Carritos
{
    public class DbCarritoService : ICarritoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private const string SessionKey = "CARRITO_TIENDA_";

        public DbCarritoService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // =========================
        // OBTENER CARRITO
        // =========================
        public async Task<Carrito> ObtenerCarritoAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
                return ObtenerCarritoSesion(tiendaId);

            var userId = _userManager.GetUserId(user);

            // Fusionar carrito de sesión
            await FusionarCarritoSesionAsync(tiendaId);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {
                carritoPersistente = new CarritoPersistente { UserId = userId, TiendaId = tiendaId };
                _context.Carritos.Add(carritoPersistente);
                await _context.SaveChangesAsync();
            }

            return MapCarrito(carritoPersistente);
        }

        // =========================
        // AÑADIR PRODUCTO
        // =========================
        public async Task AñadirProductoAsync(int tiendaId, int productoBaseId, int cantidad = 1)
        {
            if (cantidad < 1) cantidad = 1;

            var producto = await _context.ProductosBase
                .Where(p => p.Activo && p.Id == productoBaseId && p.TiendaId == tiendaId)
                .Select(p => new { p.Id, p.Nombre, p.PrecioBase, p.TipoProducto })
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new InvalidOperationException("Producto no disponible para esta tienda.");

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                // Sesión
                var carrito = ObtenerCarritoSesion(tiendaId);
                var item = carrito.Items.FirstOrDefault(i => i.ProductoBaseId == producto.Id);
                if (item == null)
                {
                    carrito.Items.Add(new CarritoItem
                    {
                        ProductoBaseId = producto.Id,
                        Nombre = producto.Nombre,
                        PrecioUnitario = producto.PrecioBase,
                        TipoProducto = producto.TipoProducto,
                        Cantidad = cantidad
                    });
                }
                else
                {
                    item.Cantidad += cantidad;
                }
                GuardarCarritoSesion(tiendaId, carrito);
                return;
            }

            // Persistente
            var userId = _userManager.GetUserId(user);
            var carritoPersistente = await _context.Carritos.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {
                carritoPersistente = new CarritoPersistente { UserId = userId, TiendaId = tiendaId };
                _context.Carritos.Add(carritoPersistente);
            }

            var existingItem = carritoPersistente.Items.FirstOrDefault(i => i.ProductoBaseId == producto.Id);
            if (existingItem == null)
                carritoPersistente.Items.Add(new CarritoPersistenteItem { ProductoBaseId = producto.Id, Cantidad = cantidad });
            else
                existingItem.Cantidad += cantidad;

            await _context.SaveChangesAsync();
        }

        // =========================
        // ELIMINAR PRODUCTO
        // =========================
        public async Task EliminarProductoAsync(int tiendaId, int productoBaseId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carrito = ObtenerCarritoSesion(tiendaId);
                carrito.Items.RemoveWhere(i => i.ProductoBaseId == productoBaseId);
                GuardarCarritoSesion(tiendaId, carrito);
                return;
            }

            var userId = _userManager.GetUserId(user);
            var carritoPersistente = await _context.Carritos.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente != null)
            {
                carritoPersistente.Items.RemoveWhere(i => i.ProductoBaseId == productoBaseId);
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // ACTUALIZAR CANTIDAD
        // =========================
        public async Task ActualizarCantidadAsync(int tiendaId, int productoBaseId, int cantidad)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carrito = ObtenerCarritoSesion(tiendaId);
                var item = carrito.Items.FirstOrDefault(i => i.ProductoBaseId == productoBaseId);
                if (item != null)
                {
                    if (cantidad <= 0) carrito.Items.Remove(item);
                    else item.Cantidad = cantidad;
                    GuardarCarritoSesion(tiendaId, carrito);
                }
                return;
            }

            var userId = _userManager.GetUserId(user);
            var carritoPersistente = await _context.Carritos.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente != null)
            {
                var item = carritoPersistente.Items.FirstOrDefault(i => i.ProductoBaseId == productoBaseId);
                if (item != null)
                {
                    if (cantidad <= 0) carritoPersistente.Items.Remove(item);
                    else item.Cantidad = cantidad;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // =========================
        // VACIAR CARRITO
        // =========================
        public async Task VaciarAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                GuardarCarritoSesion(tiendaId, new Carrito { TiendaId = tiendaId });
                return;
            }

            var userId = _userManager.GetUserId(user);
            var carritoPersistente = await _context.Carritos.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente != null)
            {
                carritoPersistente.Items.Clear();
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // FUSIONAR CARRITO SESIÓN
        // =========================
        public async Task FusionarCarritoSesionAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated) return;

            var userId = _userManager.GetUserId(user);
            var sessionCarrito = ObtenerCarritoSesion(tiendaId);
            if (!sessionCarrito.Items.Any()) return;

            var carritoPersistente = await _context.Carritos.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {
                carritoPersistente = new CarritoPersistente { UserId = userId, TiendaId = tiendaId };
                _context.Carritos.Add(carritoPersistente);
            }

            foreach (var item in sessionCarrito.Items)
            {
                var existing = carritoPersistente.Items.FirstOrDefault(i => i.ProductoBaseId == item.ProductoBaseId);
                if (existing == null)
                    carritoPersistente.Items.Add(new CarritoPersistenteItem { ProductoBaseId = item.ProductoBaseId, Cantidad = item.Cantidad });
                else
                    existing.Cantidad += item.Cantidad;
            }

            await _context.SaveChangesAsync();

            _httpContextAccessor.HttpContext.Session.Remove(SessionKey + tiendaId);
        }

        // =========================
        // Helpers
        // =========================
        private Carrito ObtenerCarritoSesion(int tiendaId)
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            var json = session.GetString(SessionKey + tiendaId);
            if (string.IsNullOrEmpty(json)) return new Carrito { TiendaId = tiendaId };
            return JsonSerializer.Deserialize<Carrito>(json) ?? new Carrito { TiendaId = tiendaId };
        }

        private void GuardarCarritoSesion(int tiendaId, Carrito carrito)
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            session.SetString(SessionKey + tiendaId, JsonSerializer.Serialize(carrito));
        }

        private Carrito MapCarrito(CarritoPersistente carritoPersistente)
        {
            var carrito = new Carrito { TiendaId = carritoPersistente.TiendaId };
            foreach (var item in carritoPersistente.Items)
            {
                var producto = _context.ProductosBase.FirstOrDefault(p => p.Id == item.ProductoBaseId);
                if (producto != null)
                {
                    carrito.Items.Add(new CarritoItem
                    {
                        ProductoBaseId = item.ProductoBaseId,
                        Nombre = producto.Nombre,
                        PrecioUnitario = producto.PrecioBase,
                        TipoProducto = producto.TipoProducto,
                        Cantidad = item.Cantidad
                    });
                }
            }
            return carrito;
        }
    }

    // Helper de extensión para ICollection<T>
    public static class CollectionExtensions
    {
        public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var items = collection.Where(predicate).ToList();
            foreach (var i in items) collection.Remove(i);
        }
    }
}
