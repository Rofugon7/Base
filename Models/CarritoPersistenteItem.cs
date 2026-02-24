using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class CarritoPersistenteItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarritoPersistenteId { get; set; }

        [Required]
        public int ProductoBaseId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        // Propiedades de navegación
        [ForeignKey("CarritoPersistenteId")]
        public virtual CarritoPersistente CarritoPersistente { get; set; }

        [ForeignKey("ProductoBaseId")]
        public virtual ProductoBase ProductoBase { get; set; }

        public decimal PrecioPersonalizado { get; set; }
        public string? DetallesOpciones { get; set; }
    }

}
