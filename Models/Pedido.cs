using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int TiendaId { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } // Pendiente, Pagado, Cancelado

        public string NombreCompleto { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Telefono { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public ICollection<PedidoItem> Items { get; set; }
        
        [Required]
        public string DniCif { get; set; }

        public decimal GastosEnvio { get; set; }
        public string TipoEnvioSeleccionado { get; set; }

        public enum MetodoPago
        {
            Tarjeta, // Para Redsys
            PayPal,
            Transferencia // Ingreso en cuenta
        }

    }
}
