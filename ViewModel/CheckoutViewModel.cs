using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [ValidateNever]
        public CarritoSession Carrito { get; set; }

        // Datos de Envío/Facturación
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public string Ciudad { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio")]
        [Display(Name = "Código Postal")]
        public string CodigoPostal { get; set; }

        public int TiendaId { get; set; } // Añade esta propiedad
    }
}