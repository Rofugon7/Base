namespace BaseConLogin.Models
{
    public class CarritoItem
    {
        public int ProductoBaseId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioUnitario { get; set; }

        public TipoProducto TipoProducto { get; set; }

        public int Cantidad { get; set; }
    }
}
