using BoardyWeb.DTOs;

namespace BoardyWeb.Models.ViewModels
{
    public class TareaDetalleViewModel
    {
        public TareaDto Tarea { get; set; } = null!;

        public IReadOnlyList<ComentarioDto> Comentarios { get; set; } = new List<ComentarioDto>();

        public IReadOnlyList<EstadoTareaDto> Estados { get; set; } = new List<EstadoTareaDto>();

        public IReadOnlyList<EtiquetaDto> EtiquetasDelTablero { get; set; } = new List<EtiquetaDto>();

        public IReadOnlyList<TableroMiembroDto> MiembrosDelTablero { get; set; } = new List<TableroMiembroDto>();

        public string UsuarioActualId { get; set; } = string.Empty;

        /// <summary>Puede comentar (Propietario, Administrador u Operario).</summary>
        public bool PuedeComentar { get; set; }

        /// <summary>Puede cambiar estado y etiquetas (Operario solo si está asignado a la tarea).</summary>
        public bool PuedeTrabajarEnTarea { get; set; }

        public bool PuedeAdministrar { get; set; }
    }
}
