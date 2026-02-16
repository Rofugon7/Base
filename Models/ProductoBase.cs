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
            // Si la lista de la base de datos tiene elementos, usamos esa.
            // Si no, miramos si hay algo en la lista temporal del VM.
            var fuentePropiedades = (PropiedadesExtendidas != null && PropiedadesExtendidas.Any())
                ? PropiedadesExtendidas.Select(p => new { p.Valor, p.Operacion, p.Orden }).ToList()
                : Propiedades.Select(p => new { p.Valor, p.Operacion, p.Orden }).ToList();

            if (fuentePropiedades.Any())
            {
                var propiedadesOrdenadas = fuentePropiedades.OrderBy(p => p.Orden).ToList();

                foreach (var prop in propiedadesOrdenadas)
                {
                    // Normalizamos el string de operación por si acaso (evitar fallos de mayúsculas)
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

            // Redondeo final a 2 decimales para dinero
            return Math.Round(precioFinal, 2, MidpointRounding.AwayFromZero);
        }
    }

}
