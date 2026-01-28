using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Tiendas
{
       public class TiendaMenuService : ITiendaMenuService
        {
            private readonly ApplicationDbContext _context;
            private readonly IMemoryCache _cache;
            private readonly TimeSpan _cacheTiempo = TimeSpan.FromMinutes(10);

            public TiendaMenuService(ApplicationDbContext context, IMemoryCache cache)
            {
                _context = context;
                _cache = cache;
            }

            public async Task<(Tienda Tienda, List<Proyectos> Proyectos, List<ProductoBase> Productos)> ObtenerMenuAsync(int tiendaId)
            {
                string cacheKey = $"Menu_Tienda_{tiendaId}";

                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return (ValueTuple<Tienda, List<Proyectos>, List<ProductoBase>>)cached!;
                }

                var tienda = await _context.Tiendas.FindAsync(tiendaId);
                if (tienda == null)
                    throw new InvalidOperationException("Tienda no encontrada");

                var proyectos = await _context.Proyectos
                    .Include(p => p.Producto)
                    .Where(p => p.Producto.TiendaId == tiendaId && p.Revisado)
                    .ToListAsync();

                var productos = await _context.ProductosBase
                    .Where(p => p.TiendaId == tiendaId && p.Activo)
                    .ToListAsync();

                var menu = (tienda, proyectos, productos);

                _cache.Set(cacheKey, menu, _cacheTiempo);

                return menu;
            }

            public void InvalidarCache(int tiendaId)
            {
                string cacheKey = $"Menu_Tienda_{tiendaId}";
                _cache.Remove(cacheKey);
            }
        }
}
