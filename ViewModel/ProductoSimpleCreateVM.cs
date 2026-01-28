using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models.ViewModels
{
    public class ProductoSimpleCreateVM
    {
        // ProductoBase

        [Required]
        [MaxLength(256)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, 999999)]
        public decimal PrecioBase { get; set; }

        [MaxLength(256)]
        public string? ImagenPrincipal { get; set; }


        // ProductoSimple

        [Required]
        [Range(0, 100000)]
        public int Stock { get; set; }
    }
}
