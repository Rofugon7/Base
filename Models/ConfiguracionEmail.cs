namespace BaseConLogin.Models
{
    public class ConfiguracionEmail
    {
        public int Id { get; set; }
        public string ServidorSmtp { get; set; } // Ej: smtp.gmail.com
        public int Puerto { get; set; }          // Ej: 587
        public string Usuario { get; set; }      // Tu correo
        public string Password { get; set; }     // Contraseña de aplicación
        public string RemitenteNombre { get; set; } // Nombre que verá el cliente
        public bool UsarSsl { get; set; } = true;
    }
}
