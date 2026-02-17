using BaseConLogin.Models.interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class PropiedadGenerica : ITenantEntity
    {
        public int Id { get; set; }

        [Required]
        public int TiendaId { get; set; }
        public Tienda Tienda { get; set; } = null!;

        [Required, MaxLength(100)]
        public string NombreInterno { get; set; } = string.Empty; // Para tu gestión

        [Required, MaxLength(100)]
        public string NombreEnProducto { get; set; } = string.Empty; // Lo que ve el cliente

        [Column(TypeName = "decimal(18,8)")]
        public decimal Valor { get; set; }

        [Required]
        public string Operacion { get; set; } = "Suma"; // Suma, Resta, Multiplicacion

        public bool IsDeleted { get; set; } = false; // Borrado lógico


    }
}