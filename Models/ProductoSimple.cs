using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class ProductoSimple
    {
        [Key]
        public int ProductoBaseId { get; set; }

        public ProductoBase Producto { get; set; } = null!;

        public int Stock { get; set; }
    }

}


