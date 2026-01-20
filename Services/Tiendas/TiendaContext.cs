using Microsoft.AspNetCore.Http;

namespace BaseConLogin.Services.Tiendas
{
    public class TiendaContext : ITiendaContext
    {
        private const string SessionKey = "TIENDA_ID";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TiendaContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int TiendaId
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                    return 0;

                return session.GetInt32(SessionKey) ?? 0;
            }
        }

        public bool HayTienda => TiendaId > 0;

        public void EstablecerTienda(int tiendaId)
        {
            var session = _httpContextAccessor.HttpContext?.Session
                ?? throw new InvalidOperationException("Sesión no disponible.");

            session.SetInt32(SessionKey, tiendaId);
        }

        public void LimpiarTienda()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(SessionKey);
        }
    }
}
