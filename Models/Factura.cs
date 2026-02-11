using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Factura
    {
        public int Id { get; set; }

        [Required]
        public string NumeroFactura { get; set; } // Ejemplo: EXP-2024-0001

        public DateTime FechaEmision { get; set; }

        // Relación con el Pedido (para saber de dónde viene)
        public int PedidoId { get; set; }
        public virtual Pedido Pedido { get; set; }

        // Datos del cliente en el momento de facturar (importante por si cambia de dirección)
        public string NombreCliente { get; set; }
        public string DniCie { get; set; } // Necesario para facturas legales
        public string DireccionFacturacion { get; set; }

        // Totales
        public decimal BaseImponible { get; set; }
        public decimal IvaPorcentaje { get; set; } = 21; // Por defecto 21%
        public decimal TotalIva { get; set; }
        public decimal TotalFactura { get; set; }

        // Veri*factu Fields
        public string HashCertificado { get; set; }
        public string QRUrl { get; set; }

        // Relación con las líneas
        public virtual ICollection<FacturaLinea> Lineas { get; set; } = new List<FacturaLinea>();

        public string HashAnterior { get; set; } // Huella de la factura previa
        public string HashActual { get; set; }   // Huella generada para esta factura

        // Tipo de factura: Normal o Rectificativa
        public bool EsRectificativa { get; set; } = false;

        // Referencia a la factura que estamos corrigiendo
        public int? FacturaOriginalId { get; set; }
        public virtual Factura FacturaOriginal { get; set; }

        public string MotivoRectificacion { get; set; } // Ejemplo: "Devolución de mercancía"
    }
}