using BaseConLogin.Models.interfaces;
using BaseConLogin.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public enum TipoProducto
    {
        Proyecto = 1,
        Simple = 2,
        Configurable = 3
    }

    public class ProductoBase : ITenantEntity
    {
        public int Id { get; set; }

        [Required]
        public int TiendaId { get; set; }
        public Tienda Tienda { get; set; } = null!;

        [Required, MaxLength(256)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBase { get; set; }

        [Required]
        public TipoProducto TipoProducto { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(256)]
        public string? ImagenPrincipal { get; set; }

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        public bool Oferta { get; set; } = false;

        public int CategoriaId { get; set; }  // nullable si quieres permitir productos sin categoría
        public Categoria? Categoria { get; set; }

        public string? SKU { get; set; }
        public ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();

        public int Stock { get; set; }

        public ICollection<ProductoPropiedadConfigurada> PropiedadesExtendidas { get; set; } = new List<ProductoPropiedadConfigurada>();

        [NotMapped]
        public List<PropiedadFilaVM> Propiedades { get; set; } = new List<PropiedadFilaVM>();

        public decimal CalcularPrecioFinal()
        {
            decimal precioFinal = PrecioBase;

            // 1. Decidimos qué fuente de datos usar.
            // IMPORTANTE: Ahora filtramos para que solo entren en el cálculo las NO configurables
            var fuentePropiedades = (PropiedadesExtendidas != null && PropiedadesExtendidas.Any())
                ? PropiedadesExtendidas
                    .Where(p => !p.EsConfigurable) // <--- FILTRO CLAVE
                    .Select(p => new { p.Valor, p.Operacion, p.Orden }).ToList()
                : Propiedades
                    .Select(p => new { p.Valor, p.Operacion, p.Orden }).ToList();
            // Nota: Si 'Propiedades' es del VM, asegúrate de filtrar ahí también si es necesario

            if (fuentePropiedades.Any())
            {
                var propiedadesOrdenadas = fuentePropiedades.OrderBy(p => p.Orden).ToList();

                foreach (var prop in propiedadesOrdenadas)
                {
                    string operacion = prop.Operacion?.Trim();

                    switch (operacion)
                    {
                        case "Suma":
                            precioFinal += prop.Valor;
                            break;
                        case "Resta":
                            precioFinal -= prop.Valor;
                            break;
                        case "Multiplicacion":
                            precioFinal *= prop.Valor;
                            break;
                    }
                }
            }

            return Math.Round(precioFinal, 2, MidpointRounding.AwayFromZero);
        }
    }

}
