using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Carritos;
using Microsoft.EntityFrameworkCore; // <- Asegúrate de tener esto

namespace BaseConLogin.Services.Pedidos
{
    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarritoService _carritoService;

        public PedidoService(ApplicationDbContext context, ICarritoService carritoService)
        {
            _context = context;
            _carritoService = carritoService;
        }

        public async Task<int> CrearPedidoDesdeCarritoAsync(int tiendaId, string userId)
        {
            var carrito = await _carritoService.ObtenerCarritoAsync(tiendaId);

            if (carrito == null || !carrito.Items.Any())
                throw new Exception("Carrito vacío");

            // Crear pedido
            var pedido = new Pedido
            {
                UserId = userId,
                TiendaId = tiendaId,
                Fecha = DateTime.UtcNow,
                Estado = "Pendiente",
                Total = carrito.Items.Sum(i => i.PrecioUnitario * i.Cantidad)
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Crear items del pedido
            foreach (var item in carrito.Items)
            {
                // Si necesitas traer ProductoSimple desde DB:
                var productoSimple = await _context.ProductoSimples
                    .FirstOrDefaultAsync(p => p.ProductoBaseId == item.ProductoBaseId);

                var pedidoItem = new PedidoItem
                {
                    PedidoId = pedido.Id,
                    ProductoBaseId = item.ProductoBaseId,
                    NombreProducto = item.Nombre,
                    PrecioUnitario = item.PrecioUnitario,
                    Cantidad = item.Cantidad
                };

                _context.PedidoItems.Add(pedidoItem);
            }

            await _context.SaveChangesAsync();

            // Vaciar carrito
            await _carritoService.VaciarAsync(tiendaId);

            return pedido.Id;
        }
    }
}
