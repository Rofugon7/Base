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

        // El ApplicationDbContext NO debe venir en el constructor, 
        // sino aquí en el InvokeAsync (DI por método)
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var host = context.Request.Host.Host.ToLowerInvariant();

            // Usamos un try-catch preventivo para evitar que la app caiga si hay problemas de red
            try
            {
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
                    // Opcional: Podrías asignar un ID por defecto o manejar el error
                    context.Items.Remove("TIENDA_ID");
                }
            }
            catch (Exception)
            {
                // Si la DB falla, nos aseguramos de no dejar basura en Items
                context.Items.Remove("TIENDA_ID");
            }

            await _next(context);
        }
    }
}