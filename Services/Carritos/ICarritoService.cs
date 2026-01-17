using BaseConLogin.Models;

namespace BaseConLogin.Services.Carritos
{
    public interface ICarritoService
    {
        Task<Carrito> ObtenerCarritoAsync();
        Task AñadirProductoAsync(int productoBaseId, int cantidad = 1);
        Task EliminarProductoAsync(int productoBaseId);
        Task ActualizarCantidadAsync(int productoBaseId, int cantidad);
        Task VaciarAsync();
    }

}
