namespace BaseConLogin.ViewModels
{
    public class ProductoDetalleViewModel
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public string TipoProducto { get; set; } = string.Empty;

        // Proyecto
        public int? Aportaciones { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // Flags de vista
        public bool EsProyecto => TipoProducto == "Proyecto";
        public bool EsSimple => TipoProducto == "Simple";
        public bool EsConfigurable => TipoProducto == "Configurable";
    }
}
