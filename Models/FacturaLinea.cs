namespace BaseConLogin.Models
{
    public class FacturaLinea
    {
        public int Id { get; set; }

        public int FacturaId { get; set; }
        public virtual Factura Factura { get; set; }

        // Copiamos los datos del producto aquí
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}