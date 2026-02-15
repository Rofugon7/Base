using BaseConLogin.Models.interfaces;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class PropiedadExtendidaMaestra : ITenantEntity
    {
        public int Id { get; set; }
        public int TiendaId { get; set; } // Siguiendo tu patrón Multi-tenant
        public Tienda Tienda { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool EsConfigurablePorDefecto { get; set; } // Si el cliente la ve en la web
    }
}