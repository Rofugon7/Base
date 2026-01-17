using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class UsuarioTienda
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        [Required]
        public int TiendaId { get; set; }
        public Tienda Tienda { get; set; } = null!;
    }
}
