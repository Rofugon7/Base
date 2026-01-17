using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Productos
{
    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearProyectoAsync(Proyectos proyecto, int tiendaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (proyecto.Producto == null)
                    throw new InvalidOperationException("El proyecto debe tener un ProductoBase asociado.");

                var productoBase = new ProductoBase
                {
                    TiendaId = tiendaId,
                    Nombre = proyecto.Producto.Nombre,
                    Descripcion = proyecto.Producto.Descripcion,
                    PrecioBase = 0,
                    TipoProducto = TipoProducto.Proyecto,
                    Activo = true
                };

                _context.ProductosBase.Add(productoBase);
                await _context.SaveChangesAsync();

                proyecto.ProductoBaseId = productoBase.Id;

                _context.Proyectos.Add(proyecto);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return productoBase.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ActualizarProyectoAsync(Proyectos proyecto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var proyectoDb = await _context.Proyectos
                    .Include(p => p.Producto)
                    .FirstOrDefaultAsync(p => p.Id == proyecto.Id);

                if (proyectoDb == null)
                    throw new InvalidOperationException("Proyecto no encontrado.");

                proyectoDb.Producto.Nombre = proyecto.Producto.Nombre;
                proyectoDb.Producto.Descripcion = proyecto.Producto.Descripcion;

                proyectoDb.Aportaciones = proyecto.Aportaciones;
                proyectoDb.FechaInicio = proyecto.FechaInicio;
                proyectoDb.FechaFin = proyecto.FechaFin;

                if (!string.IsNullOrEmpty(proyecto.ImagenPortada))
                    proyectoDb.ImagenPortada = proyecto.ImagenPortada;

                if (!string.IsNullOrEmpty(proyecto.ArchivoPdf))
                    proyectoDb.ArchivoPdf = proyecto.ArchivoPdf;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DesactivarProyectoAsync(int proyectoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var proyecto = await _context.Proyectos
                    .Include(p => p.Producto)
                    .FirstOrDefaultAsync(p => p.Id == proyectoId);

                if (proyecto == null)
                    throw new InvalidOperationException("Proyecto no encontrado.");

                // SOFT DELETE
                proyecto.Producto.Activo = false;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<int> CrearProductoSimpleAsync(ProductoSimple producto, int tiendaId)
            => throw new NotImplementedException();

        public Task<int> CrearProductoConfigurableAsync(ProductoConfigurable producto, int tiendaId)
            => throw new NotImplementedException();
    }
}
