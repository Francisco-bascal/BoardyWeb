using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class TableroDto
    {
        public int TableroId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
    }

    public class TableroDetalleDto
    {
        public int TableroId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string PropietarioNombre { get; set; } = string.Empty;

        public bool EsPropietario { get; set; }
    }

    public class CrearTableroDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarTableroDto : CrearTableroDto
    {
    }
}
