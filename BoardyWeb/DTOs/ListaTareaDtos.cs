using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class ListaTareaDto
    {
        public int ListaTareaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int Orden { get; set; }

        public int TableroId { get; set; }
    }

    public class CrearListaTareaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarListaTareaDto : CrearListaTareaDto
    {
    }
}
