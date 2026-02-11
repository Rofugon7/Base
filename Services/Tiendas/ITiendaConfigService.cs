using BaseConLogin.Models;

namespace BaseConLogin.Services.Tiendas
{
    public interface ITiendaConfigService
    {
        Task<TiendaConfig> GetConfigAsync();
    }
}
