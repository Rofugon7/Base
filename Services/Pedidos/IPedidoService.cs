using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;

namespace BaseConLogin.Services.Pedidos
{
    public interface IPedidoService
    {
        Task<int> CrearPedidoDesdeCarritoAsync(string userId, int tiendaId, CarritoSession carrito, CheckoutViewModel datosEnvio);
        Task<Pedido> ObtenerPedidoPorIdAsync(int id);
    }
}