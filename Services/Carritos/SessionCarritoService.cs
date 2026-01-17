using System.Text.Json;
using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Carritos
{
    public class SessionCarritoService : ICarritoService
    {
        private const string SessionKey = "CARRITO";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public SessionCarritoService(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<Carrito> ObtenerCarritoAsync()
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            var json = session.GetString(SessionKey);

            if (string.IsNullOrEmpty(json))
                return new Carrito();

            return JsonSerializer.Deserialize<Carrito>(json)!;
        }

        public async Task AñadirProductoAsync(int productoBaseId, int cantidad = 1)
        {
            if (cantidad < 1)
                cantidad = 1;

            var producto = await _context.ProductosBase
                .Where(p => p.Activo && p.Id == productoBaseId)
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

            var carrito = await ObtenerCarritoAsync();

            var item = carrito.Items
                .FirstOrDefault(i => i.ProductoBaseId == producto.Id);

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

            await GuardarCarritoAsync(carrito);
        }

        public async Task EliminarProductoAsync(int productoBaseId)
        {
            var carrito = await ObtenerCarritoAsync();
            carrito.Items.RemoveAll(i => i.ProductoBaseId == productoBaseId);
            await GuardarCarritoAsync(carrito);
        }

        public async Task VaciarAsync()
        {
            await GuardarCarritoAsync(new Carrito());
        }

        public async Task ActualizarCantidadAsync(int productoBaseId, int cantidad)
        {
            if (cantidad < 1)
                cantidad = 1;

            var carrito = await ObtenerCarritoAsync();
            var item = carrito.Items.FirstOrDefault(i => i.ProductoBaseId == productoBaseId);

            if (item == null)
                return;

            item.Cantidad = cantidad;
            await GuardarCarritoAsync(carrito);
        }


        // =========================
        // PRIVADO
        // =========================
        private async Task GuardarCarritoAsync(Carrito carrito)
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            var json = JsonSerializer.Serialize(carrito);
            session.SetString(SessionKey, json);
        }
    }
}
