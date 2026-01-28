public class ProductoSimpleVM
{
    public int ProductoBaseId { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string ImagenPrincipal { get; set; } = null!;
    public int Stock { get; set; }
    public bool Activo { get; set; }
    public bool EsNuevo { get; set; }
    public bool EsOferta { get; set; }
}