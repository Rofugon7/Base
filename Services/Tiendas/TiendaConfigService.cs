using BaseConLogin.Data;
using BaseConLogin.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Services.Tiendas
{
    public class TiendaConfigService : ITiendaConfigService
    {
        private readonly ApplicationDbContext _context;

        public TiendaConfigService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TiendaConfig> GetConfigAsync()
        {
            // Buscamos el primer registro de configuración
            var config = await _context.TiendaConfigs.FirstOrDefaultAsync();

            // Si la base de datos está vacía, devolvemos un objeto con datos base 
            // para evitar errores de referencia nula en la web
            if (config == null)
            {
                return new TiendaConfig
                {
                    NombreEmpresa = "Nombre Empresa S.L.",
                    CIF = "B00000000",
                    EmailContacto = "admin@tuweb.com"
                };
            }

            return config;
        }
    }
}