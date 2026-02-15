using BaseConLogin.Models;
using BaseConLogin.Models.ViewModels;

namespace BaseConLogin.ViewModel
{
    public class HomeIndexVM
    {
        public List<ProductoSimpleVM> Productos { get; set; } = new();
        public List<ProductoSimpleVM> Destacados { get; set; } = new();
        public List<ProductoSimpleVM> UltimasUnidades { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        public List<Categoria> Categorias { get; set; } = new();

        public int? CategoriaSeleccionada { get; set; }
        public string? Busqueda { get; set; }


    }



}
