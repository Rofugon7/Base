using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class ProductoConfigurable
    {
        [Key]
        public int ProductoBaseId { get; set; }

        public ProductoBase Producto { get; set; } = null!;

        public ICollection<ProductoPropiedadConfigurada> PropiedadesExtendidas { get; set; } = new List<ProductoPropiedadConfigurada>();

        public int Stock { get; set; }
    }

}
