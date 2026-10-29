using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardyWeb.Models
{
    public class Comentario : AuditableEntity
    {
        public int ComentarioId { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [Display(Name = "Contenido")]
        [StringLength(2000, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Contenido { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = null!;

        public Usuario Usuario { get; set; } = null!;

        public int TareaId { get; set; }

        public Tarea Tarea { get; set; } = null!;
    }
}
