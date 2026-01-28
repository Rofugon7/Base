using BaseConLogin.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Tiendas
{
    public class TiendaResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TiendaResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var host = context.Request.Host.Host.ToLowerInvariant();

            var tienda = await db.Tiendas
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Activa &&
                    t.Dominio.ToLower() == host);

            if (tienda != null)
            {
                context.Items["TIENDA_ID"] = tienda.Id;
            }
            else
            {
                context.Items.Remove("TIENDA_ID");
            }

            await _next(context);
        }

    }
}
