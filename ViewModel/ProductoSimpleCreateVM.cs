using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models.ViewModels
{
    public class ProductoSimpleCreateVM
    {
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal PrecioBase { get; set; }

        [Range(0, 99999)]
        public int Stock { get; set; }

        // 📸 Imagen subida
        public IFormFile? Imagen { get; set; }
    }
}
