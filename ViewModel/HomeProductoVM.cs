using BaseConLogin.Models;

namespace BaseConLogin.ViewModel
{
    public class HomeProductoVM
    {
        public ProductoSimple ProductoSimple { get; set; } = null!;
        public bool EsNuevo { get; set; }
        public bool EsOferta { get; set; }
    }
}
