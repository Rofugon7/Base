namespace BaseConLogin.ViewModel
{
    public class CategoriaVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;
    }
}
