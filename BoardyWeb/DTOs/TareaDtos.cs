using System.ComponentModel.DataAnnotations;
using BoardyWeb.Models;

namespace BoardyWeb.DTOs
{
    public class TareaDto
    {
        public int TareaId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime? FechaVencimiento { get; set; }

        public PrioridadTarea Prioridad { get; set; }

        public int Orden { get; set; }

        public int ListaTareaId { get; set; }

        public string ListaNombre { get; set; } = string.Empty;

        public int TableroId { get; set; }

        public string TableroNombre { get; set; } = string.Empty;

        public int EstadoTareaId { get; set; }

        public string EstadoNombre { get; set; } = string.Empty;

        public IReadOnlyList<EtiquetaDto> Etiquetas { get; set; } = new List<EtiquetaDto>();

        public IReadOnlyList<TableroMiembroDto> Miembros { get; set; } = new List<TableroMiembroDto>();
    }

    public class CrearTareaDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, ErrorMessage = "El título no puede superar los {1} caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Fecha de vencimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        [EnumDataType(typeof(PrioridadTarea), ErrorMessage = "La prioridad seleccionada no es válida.")]
        [Display(Name = "Prioridad")]
        public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

        [Display(Name = "Estado")]
        [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar un estado.")]
        public int EstadoTareaId { get; set; }
    }

    public class ActualizarTareaDto : CrearTareaDto
    {
    }

    public class MoverTareaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar una lista.")]
        public int ListaTareaId { get; set; }
    }

    public class CambiarEstadoTareaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar un estado.")]
        public int EstadoTareaId { get; set; }
    }
}
