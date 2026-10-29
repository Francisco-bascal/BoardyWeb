using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class EstadoTareaDto
    {
        public int EstadoTareaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CantidadTareas { get; set; }
    }

    public class CrearEstadoTareaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarEstadoTareaDto : CrearEstadoTareaDto
    {
    }

    public class EtiquetaDto
    {
        public int EtiquetaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Color { get; set; } = "#6C757D";

        public int CantidadTareas { get; set; }
    }

    public class CrearEtiquetaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El color es obligatorio")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe tener el formato hexadecimal #RRGGBB.")]
        [Display(Name = "Color")]
        public string Color { get; set; } = "#6C757D";
    }

    public class ActualizarEtiquetaDto : CrearEtiquetaDto
    {
    }

    public class RolMiembroDto
    {
        public int RolMiembroId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CantidadMiembros { get; set; }
    }

    public class CrearRolMiembroDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ActualizarRolMiembroDto : CrearRolMiembroDto
    {
    }
}
