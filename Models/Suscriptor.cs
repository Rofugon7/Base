using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Suscriptor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        public DateTime FechaSuscripcion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
    }
}
