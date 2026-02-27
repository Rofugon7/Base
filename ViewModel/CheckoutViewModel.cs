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

        [Required(ErrorMessage = "El Dni/Cif es obligatorio")]
        public string DniCif { get; set; }

        public decimal GastosEnvio { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total => Subtotal + GastosEnvio;

        // Opciones de configuración que vienen de la BD
        public bool PermitirRecogidaTienda { get; set; }
        public decimal PrecioEstandar { get; set; }
        public decimal PrecioUrgente { get; set; }
        public decimal UmbralEnvioGratis { get; set; }

        // El tipo que el usuario selecciona (puedes usar un string o enum)
        // Valores: "Estandar", "Urgente", "Recogida"
        public string TipoEnvioSeleccionado { get; set; } = "Estandar";

        public decimal PrecioEnvioSeleccionado { get; set; } // Lo que viene del input hidden

        // Propiedad calculada para mostrar siempre la suma real
        public decimal TotalFinal => ((Carrito?.Items?.Sum(i => i.PrecioUnitario * i.Cantidad)) ?? 0) + GastosEnvio;
    }
}