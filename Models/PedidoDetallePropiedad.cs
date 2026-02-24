using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class PedidoDetallePropiedad
    {
        [Key]
        public int Id { get; set; }

        // Vinculación con la línea específica del pedido
        [Required]
        public int PedidoDetalleId { get; set; }

        [ForeignKey("PedidoDetalleId")]
        public virtual PedidoItem PedidoDetalle { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombrePropiedad { get; set; } // Ej: "Tipo de Papel"

        [Required]
        [MaxLength(250)]
        public string ValorSeleccionado { get; set; } // Ej: "Cuché 300g"

        public decimal PrecioAplicado { get; set; } // El recargo que sumó esta opción

    }
}