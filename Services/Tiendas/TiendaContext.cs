using BaseConLogin.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Tiendas
{
    public class TiendaContext : ITiendaContext
    {
        private const string TIENDA_ID_KEY = "TIENDA_ID";
        private const string TIENDA_SLUG_KEY = "TIENDA_SLUG";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public TiendaContext(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        private ISession? Session =>
            _httpContextAccessor.HttpContext?.Session;

        // ============================
        // PROPIEDADES
        // ============================

        public int TiendaId =>
            ObtenerTiendaIdOpcional() ?? 0;

        public string? Slug =>
            ObtenerSlug();

        public bool HayTienda =>
            ObtenerTiendaIdOpcional().HasValue;


        // ============================
        // MÉTODOS
        // ============================

        public void EstablecerTienda(int tiendaId, string slug)
        {
            var session = Session;

            if (session == null)
                return;

            session.SetInt32(TIENDA_ID_KEY, tiendaId);
            session.SetString(TIENDA_SLUG_KEY, slug);
        }

        public void LimpiarTienda()
        {
            var session = Session;

            if (session == null)
                return;

            session.Remove(TIENDA_ID_KEY);
            session.Remove(TIENDA_SLUG_KEY);
        }

        public int ObtenerTiendaId()
        {
            var id = ObtenerTiendaIdOpcional();

            if (!id.HasValue)
                throw new InvalidOperationException("No hay tienda establecida en el contexto.");

            return id.Value;
        }

        public int? ObtenerTiendaIdOpcional()
        {
            return Session?.GetInt32(TIENDA_ID_KEY);
        }

        public string? ObtenerSlug()
        {
            return Session?.GetString(TIENDA_SLUG_KEY);
        }

        public async Task<int?> ObtenerTiendaIdUsuarioAsync(string userId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var tienda = await context.UsuariosTiendas
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TiendaId)
                .FirstOrDefaultAsync();

            return tienda == 0 ? null : tienda;
        }
    }
}
