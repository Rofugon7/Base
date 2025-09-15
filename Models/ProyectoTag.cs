using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class ProyectoTag
    {
        // Clave compuesta: ProyectoId + TagId
        public int ProyectoId { get; set; }
        public Proyectos Proyecto { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
