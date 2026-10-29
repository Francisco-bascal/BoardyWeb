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

        public bool PuedeColaborar { get; set; }

        public bool PuedeAdministrar { get; set; }
    }
}
