using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class Tienda
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int TipoNegocio { get; set; }
        // 1 = Proyectos
        // 2 = Productos simples
        // 3 = Productos configurables

        [MaxLength(200)]
        public string? Dominio { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<ProductoBase> Productos { get; set; } = new List<ProductoBase>();
        public ICollection<UsuarioTienda> UsuariosTiendas { get; set; } = new List<UsuarioTienda>();
    }

}



