using BaseConLogin.Models;

public class ProductoImagen
{
    public int Id { get; set; }

    public int ProductoBaseId { get; set; }
    public ProductoBase ProductoBase { get; set; }

    public string Ruta { get; set; }

    public bool EsPrincipal { get; set; }
}
