namespace BaseConLogin.Models
{
    public class ProductoPropiedad
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int? PropiedadMaestraId { get; set; } // Nulo si es una propiedad única "ad-hoc"

        public string Nombre { get; set; } = null!; // Se copia de la Maestra o se inventa si es particular
        public decimal Valor { get; set; }
        public string Operacion { get; set; } // "Suma", "Resta", "Multiplicacion"
        public int Orden { get; set; }
        public bool EsConfigurable { get; set; } // Sobreescribe la de la maestra si se desea

        public virtual ProductoBase  Producto { get; set; } = null!;
    }
}
