using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CerberusClassLibrary.Model.Navigation
{
    public class NavigationConfig
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("titulo")]
        [MaxLength(200)]
        public string Titulo { get; set; } = null!;

        [Column("url")]
        [MaxLength(2048)]
        public string? Url { get; set; }

        [Column("url_slug")]
        [MaxLength(300)]
        public string? UrlSlug { get; set; }

        [Column("id_padre")]
        public int? IdPadre { get; set; }

        [Column("orden")]
        public int Orden { get; set; }

        [Column("icono")]
        [MaxLength(100)]
        public string? Icono { get; set; }

        [Column("descripcion")]
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("abrir_nueva_ventana")]
        public bool AbrirNuevaVentana { get; set; }

        [Column("nivel")]
        public int Nivel { get; set; }


    }
}
