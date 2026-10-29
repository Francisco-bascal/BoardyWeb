using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.Models
{
    public class Etiqueta
    {
        public int EtiquetaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El color es obligatorio")]
        [Display(Name = "Color")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe tener el formato hexadecimal #RRGGBB.")]
        public string Color { get; set; } = "#6C757D";

        public ICollection<TareaEtiqueta> Tareas { get; set; } = new List<TareaEtiqueta>();
    }
}
