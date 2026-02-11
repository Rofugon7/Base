using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models.ViewModels
{
    public class PerfilViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } // Solo lectura generalmente

        [Display(Name = "Dirección de Envío")]
        public string? Direccion { get; set; }

        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; }

        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [Phone]
        [Display(Name = "Teléfono de Contacto")]
        public string? TelefonoContacto { get; set; }

        [Required(ErrorMessage = "El Dni/Cif es obligatorio")]
        [Display(Name = "Dni/Cif")]
        public string DniCif { get; set; }
    }
}