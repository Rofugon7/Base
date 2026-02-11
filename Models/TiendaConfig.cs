using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class TiendaConfig
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La Razón Social es obligatoria")]
        [Display(Name = "Nombre de la Empresa")]
        public string NombreEmpresa { get; set; } = string.Empty;

        [Display(Name = "Nombre Comercial")]
        public string? NombreComercial { get; set; }

        [Display(Name = "Horario")]
        public string? Horario { get; set; }

        [Required(ErrorMessage = "El CIF/NIF es obligatorio")]
        public string CIF { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string EmailContacto { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email no válido")]
        public string EmailAdministrador { get; set; } = string.Empty;

        [Url(ErrorMessage = "La URL no es válida")]
        public string? WebURL { get; set; }

        // Configuración Facturación
        [Required(ErrorMessage = "El prefijo es necesario para Verifactu")]
        [Display(Name = "Prefijo Facturas")]
        public string PrefijoFactura { get; set; } = "FAC-";

        [Display(Name = "IVA por defecto (%)")]
        public decimal IvaPorDefecto { get; set; } = 21;

        // Branding
        public string? LogoPath { get; set; }

        [Display(Name = "Color Corporativo (Hex)")]
        public string ColorCorporativo { get; set; } = "#000000";

        [Display(Name = "Color Botones (Hex)")]
        public string ColorBotones { get; set; } = "#000000";

        [Display(Name = "Color Botones Texto (Hex)")]
        public string ColorBotonesTexto { get; set; } = "#000000";

        [Display(Name = "Color Enlaces (Hex)")]
        public string ColorEnlaces { get; set; } = "#000000";

        [Display(Name = "Color Textos (Hex)")]
        public string ColorTextos { get; set; } = "#000000";

        // Registro
        [Display(Name = "Registro Mercantil")]
        public string? RegistroMercantil { get; set; } = string.Empty;

        [Display(Name = "Tomo")]
        public string? Tomo { get; set; } = string.Empty;

        [Display(Name = "Libro")]
        public string? Libro { get; set; } = string.Empty;

        [Display(Name = "Folio")]
        public string? Folio { get; set; } = string.Empty;

        [Display(Name = "Inscripcion")]
        public string? Inscripcion { get; set; } = string.Empty;

        [Display(Name = "Actividad")]
        public string? Actividad { get; set; } = string.Empty;

        [Display(Name = "Representante")]
        public string? Representante { get; set; } = string.Empty;

        //RRSS
        [Display(Name = "Google Maps")]
        public string? Maps { get; set; } = string.Empty;

        [Display(Name = "Url Instagram")]
        public string? UrlInstagram { get; set; } = string.Empty;

        [Display(Name = "Url Facebook")]
        public string? UrlFacebook { get; set; } = string.Empty;

        [Display(Name = "Url X")]
        public string? UrlX { get; set; } = string.Empty;

        [Display(Name = "Url TikTok")]
        public string? UrlTikTok { get; set; } = string.Empty;

        [Display(Name = "Whatsapp")]
        public string? Whatsapp { get; set; } = string.Empty;

        [Display(Name = "Mapa incrustado")]
        public string? MapaIncrustado { get; set; } = string.Empty;
    }
}