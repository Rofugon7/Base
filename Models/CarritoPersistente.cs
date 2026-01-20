using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class CarritoPersistente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Usuario propietario del carrito

        [Required]
        public int TiendaId { get; set; } // Carrito por tienda

        // Relación 1:N con los items
        public ICollection<CarritoPersistenteItem> Items { get; set; } = new List<CarritoPersistenteItem>();
    }

    public class CarritoPersistenteItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarritoPersistenteId { get; set; } // FK hacia CarritoPersistente

        public CarritoPersistente CarritoPersistente { get; set; } = null!;

        [Required]
        public int ProductoBaseId { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public TipoProducto TipoProducto { get; set; }
    }
}
