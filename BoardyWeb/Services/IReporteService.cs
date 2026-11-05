using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IReporteService
    {
        byte[] GenerarTablero(TableroDetalleDto tablero, IReadOnlyList<ListaTareaDto> listas, IReadOnlyList<TareaDto> tareas);

        byte[] GenerarMisTareas(string nombreUsuario, IReadOnlyList<TareaDto> tareas);

        byte[] GenerarAuditoria(string nombreTablero, IReadOnlyList<RegistroAuditoriaDto> registros);
    }
}
