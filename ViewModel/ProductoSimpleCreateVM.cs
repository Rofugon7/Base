using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IFormFile ImagenPrincipal { get; set; }
        public IFormFile Imagen { get; set; }

        // Nuevas propiedades para varias imágenes
        public List<IFormFile> Imagenes { get; set; } = new List<IFormFile>();
        public int? PrincipalIndex { get; set; } // índice de la imagen principal
        public int CategoriaId { get; set; }
        public List<SelectListItem> Categorias { get; set; } = new();

        public string SKU { get; set; }
    }
}
