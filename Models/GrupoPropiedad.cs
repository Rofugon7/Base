namespace BaseConLogin.Models
{
    public class GrupoPropiedad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // Ej: "Tipo de Papel", "Color de Tinta"
        public int TiendaId { get; set; }
    }
}
