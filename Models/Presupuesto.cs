using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Presupuesto
    {
        [Key]
        public int Id { get; set; }

        public string Cantidad { get; set; }
        public string DimensionPlana { get; set; }
        public string DimensionFinal { get; set; }
        public string CarasImpresas { get; set; }
        public string TipoPapel { get; set; }
        public string Acabado { get; set; }
        public string InformacionAdicional { get; set; }

        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellidos { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Telefono { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";
    }
}
