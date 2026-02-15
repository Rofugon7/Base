namespace BaseConLogin.ViewModel
{
    public class SearchResultVM
    {
        public string? Query { get; set; }
        public int? CategoriaId { get; set; }

        public List<ProductoCardVM> Productos { get; set; }

        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }


    }

}
