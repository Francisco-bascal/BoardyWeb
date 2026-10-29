using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.Models
{
    public class Tarea : AuditableEntity
    {
        public int TareaId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [Display(Name = "Título")]
        [StringLength(200, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(2000, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Fecha de vencimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Required(ErrorMessage = "La prioridad es obligatoria")]
        [Display(Name = "Prioridad")]
        [StringLength(50, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Prioridad { get; set; } = string.Empty;

        public int ListaTareaId { get; set; }

        public ListaTarea ListaTarea { get; set; } = null!;

        public int EstadoTareaId { get; set; }

        public EstadoTarea EstadoTarea { get; set; } = null!;

        public ICollection<TareaMiembro> MiembrosAsignados { get; set; } = new List<TareaMiembro>();

        public ICollection<TareaEtiqueta> Etiquetas { get; set; } = new List<TareaEtiqueta>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
