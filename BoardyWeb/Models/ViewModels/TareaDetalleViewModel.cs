using BoardyWeb.DTOs;

namespace BoardyWeb.Models.ViewModels
{
    public class TareaDetalleViewModel
    {
        public TareaDto Tarea { get; set; } = null!;

        public IReadOnlyList<ComentarioDto> Comentarios { get; set; } = new List<ComentarioDto>();

        public bool PuedeColaborar { get; set; }

        public bool PuedeAdministrar { get; set; }
    }
}
