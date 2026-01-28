using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models.ViewModels
{
    public class ProductoSimpleEditVM
    {
        public int ProductoBaseId { get; set; }

        [Required, MaxLength(256)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [Required]
        public decimal PrecioBase { get; set; }

        public string? ImagenPrincipal { get; set; }

        public bool Activo { get; set; }

        [Required]
        public int Stock { get; set; }
    }
}
