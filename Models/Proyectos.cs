using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseConLogin.Models
{
    public class Proyectos 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductoBaseId { get; set; }

        [ForeignKey(nameof(ProductoBaseId))]
        public ProductoBase Producto { get; set; } = null!;

        [StringLength(256)]
        public string? ImagenPortada { get; set; } // ruta del archivo en wwwroot/uploads

        public int? Aportaciones { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [StringLength(256)]
        public string? ArchivoPdf { get; set; } // ruta del archivo en wwwroot/uploads

        // ✅ Nuevos campos
        [Required]
        [MaxLength(350)]
        public string Subtitulo { get; set; } = string.Empty;

        public string? Autor { get; set; }

        public int? Categoria { get; set; } // FK opcional

        public int? Ubicacion { get; set; } // FK opcional

        [MaxLength(600)]
        public string? Adelanto { get; set; }

        [MaxLength(600)]
        public string? Recompensa { get; set; }


        [MaxLength(300)]
        public string? Calendario { get; set; }

        public bool Revisado { get; set; }

        // Relación con Tags
        [Required]
        public ICollection<ProyectoTag> ProyectoTags { get; set; } = new List<ProyectoTag>();

        // 🛠 Función para procesar archivos 
        public void ProcesarArchivos(string? nuevaImagen, string? nuevoPdf)
        {
            if (!string.IsNullOrEmpty(nuevaImagen))
            {
                ImagenPortada = nuevaImagen;
            }

            if (!string.IsNullOrEmpty(nuevoPdf))
            {
                ArchivoPdf = nuevoPdf;
            }
        }
    }
}
