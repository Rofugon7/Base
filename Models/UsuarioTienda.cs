using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class UsuarioTienda
    {
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public int TiendaId { get; set; }
        public Tienda Tienda { get; set; } = null!;

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        public bool EsPrincipal { get; set; }
    }

}
