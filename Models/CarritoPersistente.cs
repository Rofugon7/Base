using BaseConLogin.Models.interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class CarritoPersistente : ITenantEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // FK a IdentityUser

        [Required]
        public int TiendaId { get; set; }   // FK a Tienda

        // Relación con Items
        public virtual ICollection<CarritoPersistenteItem> Items { get; set; } = new List<CarritoPersistenteItem>();

        public Tienda Tienda { get; set; } = null!;
    }


}
