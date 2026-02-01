using BaseConLogin.Models.interfaces;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public enum TipoProducto
    {
        Proyecto = 1,
        Simple = 2,
        Configurable = 3
    }

    public class ProductoBase : ITenantEntity
    {
        public int Id { get; set; }

        [Required]
        public int TiendaId { get; set; }
        public Tienda Tienda { get; set; } = null!;

        [Required, MaxLength(256)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [Required]
        public decimal PrecioBase { get; set; }

        [Required]
        public TipoProducto TipoProducto { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(256)]
        public string? ImagenPrincipal { get; set; }

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        public bool Oferta { get; set; } = false;

        public int CategoriaId { get; set; }  // nullable si quieres permitir productos sin categoría
        public Categoria? Categoria { get; set; }

        //public ProductoSimple? ProductoSimple { get; set; }
        //public ProductoVariable? ProductoVariable { get; set; }

        public string? SKU { get; set; }
        public ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();


    }

}
