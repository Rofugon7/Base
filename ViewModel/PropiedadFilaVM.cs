namespace BaseConLogin.ViewModel
{
    public class PropiedadFilaVM
    {
        public int? PropiedadMaestraId { get; set; }
        public string NombreEnProducto { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Operacion { get; set; } // Suma, Resta, Multiplicacion
        public int Orden { get; set; }
        public bool EsConfigurable { get; set; }
    }
}
