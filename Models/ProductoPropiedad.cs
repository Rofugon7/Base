using BaseConLogin.Models;
using BaseConLogin.Models.interfaces;
using System.ComponentModel.DataAnnotations;

public class ProductoPropiedad : ITenantEntity
{
    public int Id { get; set; }

    [Required]
    public int TiendaId { get; set; }
    public Tienda Tienda { get; set; } = null!;

    [Required]
    public int ProductoId { get; set; }
    public ProductoBase Producto { get; set; } = null!;

    [Required]
    public int PropiedadGenericaId { get; set; }
    public PropiedadGenerica PropiedadGenerica { get; set; } = null!;

    // Para opciones de cliente: ¿Esta opción está marcada por defecto?
    public bool EsPredeterminada { get; set; }

    // Para ordenar visualmente las opciones
    public int Orden { get; set; }
}