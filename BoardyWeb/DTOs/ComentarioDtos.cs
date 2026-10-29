using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class ComentarioDto
    {
        public int ComentarioId { get; set; }

        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public string UsuarioNombre { get; set; } = string.Empty;
    }

    public class CrearComentarioDto
    {
        [Required(ErrorMessage = "El contenido es obligatorio")]
        [StringLength(2000, ErrorMessage = "El contenido no puede superar los {1} caracteres.")]
        [Display(Name = "Contenido")]
        public string Contenido { get; set; } = string.Empty;
    }
}
