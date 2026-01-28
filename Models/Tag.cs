using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string Slug { get; set; }

        // Relación inversa con Proyectos
        public ICollection<ProyectoTag> ProyectoTags { get; set; } = new List<ProyectoTag>();
    }
}
