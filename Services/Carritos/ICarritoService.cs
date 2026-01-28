using BaseConLogin.Models;
using System.Threading.Tasks;

namespace BaseConLogin.Services.Carritos
{
    public interface ICarritoService
    {
        Task<CarritoSession> ObtenerCarritoAsync(int tiendaId);

        Task AñadirProductoAsync(int tiendaId, int productoBaseId, int cantidad = 1);

        Task EliminarProductoAsync(int tiendaId, int productoBaseId);

        Task ActualizarCantidadAsync(int tiendaId, int productoBaseId, int cantidad);

        Task VaciarAsync(int tiendaId);

        Task FusionarCarritoSesionAsync(int tiendaId);

        Task<int> ObtenerCantidadItemsAsync(int tiendaId);

        Task<int> ObtenerCantidadItemsAsync(string userId);
    }
}
