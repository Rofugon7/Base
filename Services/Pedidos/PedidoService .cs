using BaseConLogin.Data;
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
                    Estado = EstadoPedido.Pendiente,
                    NombreCompleto = datosEnvio.NombreCompleto,
                    Direccion = datosEnvio.Direccion,
                    Ciudad = datosEnvio.Ciudad,
                    CodigoPostal = datosEnvio.CodigoPostal,
                    Telefono = datosEnvio.Telefono,
                    DniCif = datosEnvio.DniCif,
                    Items = new List<PedidoItem>()
                };

                // Añadimos el pedido primero para que EF genere el ID
                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync();

                // 2. Mapear items del carrito a PedidoItems y sus Propiedades
                foreach (var item in carrito.Items)
                {
                    // Descontar stock
                    var producto = await _context.ProductosBase.FindAsync(item.ProductoBaseId);
                    if (producto != null)
                    {
                        if (producto.Stock < item.Cantidad)
                            throw new Exception($"Stock insuficiente para el producto: {item.Nombre}");

                        producto.Stock -= item.Cantidad;
                    }

                    // Creamos el Item del Pedido
                    var nuevoItem = new PedidoItem
                    {
                        PedidoId = nuevoPedido.Id,
                        ProductoBaseId = item.ProductoBaseId,
                        NombreProducto = item.Nombre,
                        PrecioUnitario = item.PrecioUnitario,
                        Cantidad = item.Cantidad
                    };

                    _context.PedidoItems.Add(nuevoItem);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el ID del item

                    // 3. NUEVO: Guardar las propiedades elegidas (Configuración dinámica)
                    if (!string.IsNullOrEmpty(item.OpcionesSeleccionadas))
                    {
                        var propiedadElegida = new PedidoDetallePropiedad
                        {
                            PedidoDetalleId = nuevoItem.Id, // Vinculamos a la línea de pedido
                            NombrePropiedad = "Configuración",
                            ValorSeleccionado = item.OpcionesSeleccionadas,
                            PrecioAplicado = 0 // El precio extra ya va incluido en el PrecioUnitario del item
                        };
                        _context.PedidoDetallePropiedades.Add(propiedadElegida);
                    }
                }

                // Guardamos las propiedades y confirmamos
                await _context.SaveChangesAsync();
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
                    .ThenInclude(i => i.PropiedadesElegidas) // Incluimos las opciones para la vista de resumen
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}