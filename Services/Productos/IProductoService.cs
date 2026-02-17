using BaseConLogin.Models;

namespace BaseConLogin.Services.Productos
{
    public interface IProductoService
    {
        Task<int> CrearProyectoAsync(Proyectos proyecto, int tiendaId);
        Task ActualizarProyectoAsync(Proyectos proyecto);

        Task DesactivarProyectoAsync(int proyectoId);

        Task<int> CrearProductoSimpleAsync(ProductoBase producto, int tiendaId);
        Task<int> CrearProductoConfigurableAsync(ProductoConfigurable producto, int tiendaId);

        Task AsignarPropiedadACategoria(int propiedadGenericaId, int categoriaId);

        Task AsignarPropiedadAProducto(PropiedadGenerica pg, ProductoBase producto);
    }
}
