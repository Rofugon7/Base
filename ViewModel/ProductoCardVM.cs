namespace BaseConLogin.ViewModel
{
    public class ProductoCardVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public decimal Precio { get; set; }

        public string Imagen { get; set; }

        public bool EsNuevo { get; set; }

        public bool EsOferta { get; set; }

        // Cada producto individual tiene sus propias propiedades
        public List<PropiedadFilaVM> Propiedades { get; set; } = new List<PropiedadFilaVM>();

        public decimal CalcularPrecioFinal()
        {
            decimal precioFinal = Precio;

            if (Propiedades != null && Propiedades.Any())
            {
                var propiedadesOrdenadas = Propiedades.OrderBy(p => p.Orden).ToList();
                foreach (var prop in propiedadesOrdenadas)
                {
                    switch (prop.Operacion)
                    {
                        case "Suma": precioFinal += prop.Valor; break;
                        case "Resta": precioFinal -= prop.Valor; break;
                        case "Multiplicacion": precioFinal *= prop.Valor; break;
                    }
                }
            }

            // Redondeo exacto a 2 decimales para la visualización final
            return Math.Round(precioFinal, 2, MidpointRounding.AwayFromZero);
        }
    }

}
