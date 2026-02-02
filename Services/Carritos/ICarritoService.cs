using BaseConLogin.Models;
using System.Threading.Tasks;

namespace BaseConLogin.Services.Carritos
{
    public interface ICarritoService
    {
        // Obtener el carrito completo (mapeado a CarritoSession)
        Task<CarritoSession> ObtenerCarritoAsync(int tiendaId);

        // Acciones principales
        Task AñadirProductoAsync(int tiendaId, int productoBaseId, int cantidad = 1);
        Task ActualizarCantidadAsync(int tiendaId, int productoBaseId, int cantidad);
        Task EliminarProductoAsync(int tiendaId, int productoBaseId);
        Task VaciarAsync(int tiendaId);

        // Gestión de sesión y login
        Task FusionarCarritoSesionAsync(int tiendaId);

        // Métodos de conteo (para el Badge del menú)
        Task<int> ObtenerCantidadItemsAsync(int tiendaId);
        Task<int> ObtenerCantidadItemsAsync(string userId);

        Task LimpiarCarritoAsync(int tiendaId);
    }
}