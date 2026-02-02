using Microsoft.AspNetCore.Identity;

namespace BaseConLogin.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? TelefonoContacto { get; set; }
    }
}