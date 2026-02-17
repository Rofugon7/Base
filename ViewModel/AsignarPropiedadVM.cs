namespace BaseConLogin.ViewModel
{
    public class AsignarPropiedadVM
    {
        public int PropiedadGenericaId { get; set; }
        public string NombrePropiedad { get; set; } = string.Empty;
        public int? CategoriaId { get; set; } // Categoría destino
        public bool AplicarATodos { get; set; } // Opcional: ¿Aplicar a toda la tienda?
        public int? ProductoId { get; set; }
    }
}