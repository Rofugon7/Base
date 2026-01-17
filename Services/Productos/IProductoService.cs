using BaseConLogin.Models;

namespace BaseConLogin.Services.Productos
{
    public interface IProductoService
    {
        Task<int> CrearProyectoAsync(Proyectos proyecto, int tiendaId);
        Task ActualizarProyectoAsync(Proyectos proyecto);

        Task DesactivarProyectoAsync(int proyectoId);

        Task<int> CrearProductoSimpleAsync(ProductoSimple producto, int tiendaId);
        Task<int> CrearProductoConfigurableAsync(ProductoConfigurable producto, int tiendaId);
    }
}
