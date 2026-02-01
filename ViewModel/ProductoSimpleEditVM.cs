using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseConLogin.Models.ViewModels
{
    public class ProductoSimpleEditVM
    {
        public int ProductoBaseId { get; set; }

        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }

        public bool Activo { get; set; }
        public int Stock { get; set; }

        // =========================
        // IMAGEN ACTUAL (STRING)
        // =========================
        public string ImagenActual { get; set; } = "";

        // =========================
        // NUEVA IMAGEN (FILE)
        // =========================
        public IFormFile? NuevaImagen { get; set; }

        public bool EsNuevo { get; set; }
        public bool EsOferta { get; set; }

        public int CategoriaId { get; set; }
        public List<SelectListItem> Categorias { get; set; } = new();

        public string SKU { get; set; }

        public List<string> ImagenesActuales { get; set; } = new List<string>();
    }
}
