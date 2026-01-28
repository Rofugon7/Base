using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;

namespace BaseConLogin.Services.Carritos
{
    public class DbCarritoService : ICarritoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        private const string SessionKey = "CARRITO_TIENDA_";

        public DbCarritoService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        // =========================
        // CANTIDAD TOTAL DE ITEMS
        // =========================
        public async Task<int> ObtenerCantidadItemsAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // Invitado -> sesión
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carritoSesion = ObtenerCarritoSesion(tiendaId);
                return carritoSesion.Items.Sum(i => i.Cantidad);
            }

            // Usuario autenticado -> BD
            var userId = _userManager.GetUserId(user);

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carrito == null)
                return 0;

            return carrito.Items.Sum(i => i.Cantidad);
        }

        

// Por esto:
public async Task<int> ObtenerCantidadItemsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var carrito = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (carrito == null)
                return 0;

            return carrito.Items.Sum(i => i.Cantidad);
        }


        // =========================
        // OBTENER CARRITO
        // =========================
        public async Task<CarritoSession> ObtenerCarritoAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
                return ObtenerCarritoSesion(tiendaId);

            var userId = _userManager.GetUserId(user);

            // Fusionar carrito de sesión
            var carritoSesion = ObtenerCarritoSesion(tiendaId);

            if (carritoSesion.Items.Any())
                await FusionarCarritoSesionAsync(tiendaId);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {


                var claim = user.FindFirst("TiendaId");

                if (claim != null)
                {
                    tiendaId = int.Parse(claim.Value);
                }

                carritoPersistente = new CarritoPersistente
                {
                    UserId = userId,
                    TiendaId = tiendaId
                };

                _context.Carritos.Add(carritoPersistente);
                await _context.SaveChangesAsync();
            }

            return await MapCarritoAsync(carritoPersistente);
        }

        // =========================
        // AÑADIR PRODUCTO
        // =========================
        public async Task AñadirProductoAsync(
            int tiendaId,
            int productoBaseId,
            int cantidad = 1)
        {
            if (cantidad < 1)
                cantidad = 1;

            var producto = await _context.ProductosBase
                .Where(p =>
                    p.Activo &&
                    p.Id == productoBaseId &&
                    p.TiendaId == tiendaId)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.PrecioBase,
                    p.TipoProducto
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                throw new InvalidOperationException("Producto no disponible.");

            var user = _httpContextAccessor.HttpContext?.User;

            // INVITADO -> SESIÓN
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carrito = ObtenerCarritoSesion(tiendaId);

                var item = carrito.Items
                    .FirstOrDefault(i => i.ProductoBaseId == producto.Id);

                if (item == null)
                {
                    carrito.Items.Add(new CarritoSessionItem
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

            // USUARIO -> BD
            var userId = _userManager.GetUserId(user);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {
                carritoPersistente = new CarritoPersistente
                {
                    UserId = userId,
                    TiendaId = tiendaId
                };

                _context.Carritos.Add(carritoPersistente);
            }

            var existente = carritoPersistente.Items
                .FirstOrDefault(i => i.ProductoBaseId == producto.Id);

            if (existente == null)
            {
                carritoPersistente.Items.Add(
                    new CarritoPersistenteItem
                    {
                        ProductoBaseId = producto.Id,
                        Cantidad = cantidad
                    });
            }
            else
            {
                existente.Cantidad += cantidad;
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // ELIMINAR PRODUCTO
        // =========================
        public async Task EliminarProductoAsync(int tiendaId, int productoBaseId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // SESIÓN
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carrito = ObtenerCarritoSesion(tiendaId);

                carrito.Items.RemoveWhere(i =>
                    i.ProductoBaseId == productoBaseId);

                GuardarCarritoSesion(tiendaId, carrito);
                return;
            }

            // BD
            var userId = _userManager.GetUserId(user);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
                return;

            carritoPersistente.Items.RemoveWhere(i =>
                i.ProductoBaseId == productoBaseId);

            await _context.SaveChangesAsync();
        }

        // =========================
        // ACTUALIZAR CANTIDAD
        // =========================
        public async Task ActualizarCantidadAsync(
            int tiendaId,
            int productoBaseId,
            int cantidad)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // SESIÓN
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                var carrito = ObtenerCarritoSesion(tiendaId);

                var item = carrito.Items
                    .FirstOrDefault(i => i.ProductoBaseId == productoBaseId);

                if (item == null)
                    return;

                if (cantidad <= 0)
                    carrito.Items.Remove(item);
                else
                    item.Cantidad = cantidad;

                GuardarCarritoSesion(tiendaId, carrito);
                return;
            }

            // BD
            var userId = _userManager.GetUserId(user);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
                return;

            var existente = carritoPersistente.Items
                .FirstOrDefault(i => i.ProductoBaseId == productoBaseId);

            if (existente == null)
                return;

            if (cantidad <= 0)
                carritoPersistente.Items.Remove(existente);
            else
                existente.Cantidad = cantidad;

            await _context.SaveChangesAsync();
        }

        // =========================
        // VACIAR CARRITO
        // =========================
        public async Task VaciarAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // SESIÓN
            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
            {
                GuardarCarritoSesion(tiendaId, new CarritoSession
                {
                    TiendaId = tiendaId
                });

                return;
            }

            // BD
            var userId = _userManager.GetUserId(user);

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
                return;

            carritoPersistente.Items.Clear();

            await _context.SaveChangesAsync();
        }

        // =========================
        // FUSIONAR SESIÓN
        // =========================
        public async Task FusionarCarritoSesionAsync(int tiendaId)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !_httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated)
                return;

            var userId = _userManager.GetUserId(user);

            var carritoSesion = ObtenerCarritoSesion(tiendaId);

            if (!carritoSesion.Items.Any())
                return;

            var carritoPersistente = await _context.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.TiendaId == tiendaId);

            if (carritoPersistente == null)
            {
                carritoPersistente = new CarritoPersistente
                {
                    UserId = userId,
                    TiendaId = tiendaId
                };

                _context.Carritos.Add(carritoPersistente);
            }

            foreach (var item in carritoSesion.Items)
            {
                var existente = carritoPersistente.Items
                    .FirstOrDefault(i => i.ProductoBaseId == item.ProductoBaseId);

                if (existente == null)
                {
                    carritoPersistente.Items.Add(
                        new CarritoPersistenteItem
                        {
                            ProductoBaseId = item.ProductoBaseId,
                            Cantidad = item.Cantidad
                        });
                }
                else
                {
                    existente.Cantidad += item.Cantidad;
                }
            }

            await _context.SaveChangesAsync();

            _httpContextAccessor.HttpContext!.Session
                .Remove(SessionKey + tiendaId);
        }

        // =========================
        // HELPERS SESIÓN
        // =========================
        private CarritoSession ObtenerCarritoSesion(int tiendaId)
        {
            var session = _httpContextAccessor.HttpContext!.Session;

            var json = session.GetString(SessionKey + tiendaId);

            if (string.IsNullOrEmpty(json))
                return new CarritoSession { TiendaId = tiendaId };

            return JsonSerializer.Deserialize<CarritoSession>(json)
                   ?? new CarritoSession { TiendaId = tiendaId };
        }

        private void GuardarCarritoSesion(int tiendaId, CarritoSession carrito)
        {
            var session = _httpContextAccessor.HttpContext!.Session;

            session.SetString(
                SessionKey + tiendaId,
                JsonSerializer.Serialize(carrito));
        }

        // =========================
        // MAPEO BD -> DOMINIO
        // =========================
        private async Task<CarritoSession> MapCarritoAsync(
            CarritoPersistente carritoPersistente)
        {
            var ids = carritoPersistente.Items
                .Select(i => i.ProductoBaseId)
                .ToList();

            var productos = await _context.ProductosBase
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var carrito = new CarritoSession
            {
                TiendaId = carritoPersistente.TiendaId
            };

            foreach (var item in carritoPersistente.Items)
            {
                if (!productos.ContainsKey(item.ProductoBaseId))
                    continue;

                var p = productos[item.ProductoBaseId];

                carrito.Items.Add(new CarritoSessionItem
                {
                    ProductoBaseId = p.Id,
                    Nombre = p.Nombre,
                    PrecioUnitario = p.PrecioBase,
                    TipoProducto = p.TipoProducto,
                    Cantidad = item.Cantidad
                });
            }

            return carrito;
        }
    }

    // =========================
    // EXTENSIONES
    // =========================
    public static class CollectionExtensions
    {
        public static void RemoveWhere<T>(
            this ICollection<T> collection,
            Func<T, bool> predicate)
        {
            var items = collection
                .Where(predicate)
                .ToList();

            foreach (var i in items)
                collection.Remove(i);
        }
    }
}
