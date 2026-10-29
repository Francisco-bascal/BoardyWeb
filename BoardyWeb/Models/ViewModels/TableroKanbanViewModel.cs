using BoardyWeb.DTOs;

namespace BoardyWeb.Models.ViewModels
{
    public class TableroKanbanViewModel
    {
        public TableroDetalleDto Tablero { get; set; } = null!;

        public IReadOnlyList<ListaTareaDto> Listas { get; set; } = new List<ListaTareaDto>();

        public IReadOnlyList<TareaDto> Tareas { get; set; } = new List<TareaDto>();

        public string UsuarioActualId { get; set; } = string.Empty;

        /// <summary>Propietario, Administrador u Operario (puede mover las tareas en las que trabaja).</summary>
        public bool PuedeColaborar { get; set; }

        public bool PuedeAdministrar { get; set; }

        public bool PuedeGestionar { get; set; }
    }
}
