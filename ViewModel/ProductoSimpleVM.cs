using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using BaseConLogin.Models;

namespace BaseConLogin.Models.ViewModels
{
    public class ProductoSimpleVM
    {
        public int ProductoBaseId { get; set; } // Solo para Edit

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal PrecioBase { get; set; }

        [Range(0, 99999)]
        public int Stock { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public string SKU { get; set; }

        [Required]
        public int CategoriaId { get; set; }
        public List<SelectListItem> Categorias { get; set; } = new();

        // 📸 Imágenes
        public List<IFormFile> Imagenes { get; set; } = new List<IFormFile>(); // hasta 4 imágenes
        public int? PrincipalIndex { get; set; } = 0;// índice de la principal

        // Solo para Edit: mostrar imágenes existentes
        public List<ProductoImagen> ImagenesActuales { get; set; } = new();

        public string ImagenPrincipal { get; set; }

        public bool EsNuevo { get; set; }
        public bool EsOferta { get; set; }

        public List<int> ImagenesBorrar { get; set; } = new();

        public List<int?> SlotsIds { get; set; } = new(); // 4 posiciones






    }
}
