using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.ViewModel
{
    [NotMapped]
    public class PropiedadFilaVM
    {
        public int Id { get; set; }
        public int? GrupoPropiedadId { get; set; }
        public string NombreEnProducto { get; set; } = string.Empty;
        public string NombrePropiedad { get; set; }
        public decimal Valor { get; set; }
        public string Operacion { get; set; } // Suma, Resta, Multiplicacion
        public int Orden { get; set; }
        public bool EsConfigurable { get; set; }
    }

}
