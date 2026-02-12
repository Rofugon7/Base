using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class TrabajoImpresion
    {
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public ApplicationUser? Usuario { get; set; }
        public string NombreArchivoOriginal { get; set; }
        public string NombreArchivoServidor { get; set; }
        public string RutaFisica { get; set; }
        public string NifLimpio { get; set; }
        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completado
        public string? Notas { get; set; }
        public bool ArchivoEliminado { get; set; } = false; // Para el historial
    }
}
