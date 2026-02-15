using BaseConLogin.Models.interfaces;
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

        public decimal CalcularPrecioFinal()
        {
            decimal precioFinal = PrecioBase;

            // Ordenamos por el campo 'Orden' para que las operaciones se apliquen en secuencia
            var propiedadesOrdenadas = PropiedadesExtendidas.OrderBy(p => p.Orden).ToList();

            foreach (var prop in propiedadesOrdenadas)
            {
                switch (prop.Operacion)
                {
                    case "Suma":
                        precioFinal += prop.Valor;
                        break;
                    case "Resta":
                        precioFinal -= prop.Valor;
                        break;
                    case "Multiplicacion":
                        // Útil para porcentajes (ej: un valor de 1.10 incrementa un 10%)
                        precioFinal *= prop.Valor;
                        break;
                    default:
                        // Si no hay operación clara, por defecto sumamos
                        precioFinal += prop.Valor;
                        break;
                }
            }

            return precioFinal;
        }
    }

}
