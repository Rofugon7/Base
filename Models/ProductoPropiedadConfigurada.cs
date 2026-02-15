using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class ProductoPropiedadConfigurada
    {
        public int Id { get; set; }

        [Required]
        public int ProductoBaseId { get; set; }
        public ProductoBase Producto { get; set; } = null!;

        // Relación opcional con la maestra
        public int? PropiedadMaestraId { get; set; }
        public PropiedadExtendidaMaestra? PropiedadMaestra { get; set; }

        [Required, MaxLength(100)]
        public string NombreEnProducto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Required]
        public string Operacion { get; set; } // "Suma", "Resta", "Multiplicacion"

        [Required]
        public int Orden { get; set; }

        public bool EsConfigurable { get; set; } // Si el cliente puede verla/tocarla en la web
    }
}