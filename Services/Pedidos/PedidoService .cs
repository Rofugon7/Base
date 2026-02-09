using BaseConLogin.Data; // Tu ApplicationDbContext
using BaseConLogin.Models;
using BaseConLogin.Models.enumerados;
using BaseConLogin.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Pedidos
{
    public class PedidoService : IPedidoService
    {
        private readonly ApplicationDbContext _context;

        public PedidoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearPedidoDesdeCarritoAsync(string userId, int tiendaId, CarritoSession carrito, CheckoutViewModel datosEnvio)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear el objeto Pedido
                var nuevoPedido = new Pedido
                {
                    UserId = userId,
                    TiendaId = tiendaId,
                    Fecha = DateTime.Now,
                    Total = carrito.Total,
                    Estado = EstadoPedido.Pendiente, // Se cambiará a "Pagado" tras la simulación de pago
                    NombreCompleto = datosEnvio.NombreCompleto,
                    Direccion = datosEnvio.Direccion,
                    Ciudad = datosEnvio.Ciudad,
                    CodigoPostal = datosEnvio.CodigoPostal,
                    Telefono = datosEnvio.Telefono,
                    Items = new List<PedidoItem>()
                };

                // 2. Mapear items del carrito a PedidoItems y actualizar stock
                foreach (var item in carrito.Items)
                {
                    // Buscamos el producto en DB para descontar stock
                    var producto = await _context.ProductoSimples.FindAsync(item.ProductoBaseId);
                    if (producto != null)
                    {
                        if (producto.Stock < item.Cantidad)
                            throw new Exception($"Stock insuficiente para el producto: {item.Nombre}");

                        producto.Stock -= item.Cantidad;
                    }

                    nuevoPedido.Items.Add(new PedidoItem
                    {
                        ProductoBaseId = item.ProductoBaseId,
                        NombreProducto = item.Nombre,
                        PrecioUnitario = item.PrecioUnitario,
                        Cantidad = item.Cantidad
                    });
                }

                // 3. Guardar en la base de datos
                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync();

                // 4. Confirmar transacción
                await transaction.CommitAsync();

                return nuevoPedido.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error al procesar el pedido: " + ex.Message);
            }
        }

        public async Task<Pedido> ObtenerPedidoPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}