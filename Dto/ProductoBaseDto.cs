using BaseConLogin.Models;

namespace BaseConLogin.Dto
{
    public class ProductoBaseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
    }
}
