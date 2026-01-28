using BaseConLogin.Data;
using BaseConLogin.Models;
using BaseConLogin.Services.Tiendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BaseConLogin.Services.Tiendas
{
    public interface ITiendaMenuService
    {
        Task<(Tienda Tienda, List<Proyectos> Proyectos, List<ProductoBase> Productos)> ObtenerMenuAsync(int tiendaId);
        void InvalidarCache(int tiendaId);
    }

   
}

