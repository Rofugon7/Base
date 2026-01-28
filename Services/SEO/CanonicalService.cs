using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Http;

namespace BaseConLogin.Services.Seo
{
    public interface ICanonicalService
    {
        string Build(string path);
    }

    public class CanonicalService : ICanonicalService
    {
        private readonly IHttpContextAccessor _http;
        private readonly ITiendaContext _tiendaContext;

        public CanonicalService(
            IHttpContextAccessor http,
            ITiendaContext tiendaContext)
        {
            _http = http;
            _tiendaContext = tiendaContext;
        }

        public string Build(string path)
        {
            var request = _http.HttpContext!.Request;

            var slug = _tiendaContext.ObtenerSlug();

            var baseUrl =
                $"{request.Scheme}://{request.Host}/t/{slug}";

            return $"{baseUrl}/{path.TrimStart('/')}";
        }
    }
}
