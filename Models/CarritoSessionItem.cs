namespace BaseConLogin.Models
{
    public class CarritoSessionItem
    {
        public int ProductoBaseId { get; set; }

        public ProductoSimple ProductoSimple { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public TipoProducto TipoProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }
}
