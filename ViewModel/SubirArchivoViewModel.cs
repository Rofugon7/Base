using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // <--- FALTA ESTA LÍNEA para IFormFile

namespace BaseConLogin.ViewModel
{
    public class SubirArchivoViewModel
    {
        [Required(ErrorMessage = "Por favor, selecciona un archivo")]
        [Display(Name = "Archivo para imprimir")]
        public IFormFile Archivo { get; set; }

        [Display(Name = "Notas adicionales (Copias, acabado, etc.)")]
        public string? Notas { get; set; }
    }
}