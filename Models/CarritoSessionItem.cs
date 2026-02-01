namespace BaseConLogin.Models
{
    public class CarritoSessionItem
    {
        public int ProductoBaseId { get; set; }
        // Eliminamos ProductoSimple y TipoProducto para evitar errores de serialización
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
        public string ImagenRuta { get; set; } = string.Empty;
    }
}