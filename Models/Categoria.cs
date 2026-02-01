using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TiendaId { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        // Opcional: fecha de alta para destacar nuevas categorías
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        // Navegación a la tienda
        public Tienda? Tienda { get; set; }

        // Productos asociados (opcional)
        public List<ProductoBase> Productos { get; set; } = new();


    }
}
