using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.Models
{
    public class ListaTarea : AuditableEntity
    {
        public int ListaTareaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(150, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(1000, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        public int TableroId { get; set; }

        public Tablero Tablero { get; set; } = null!;

        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
