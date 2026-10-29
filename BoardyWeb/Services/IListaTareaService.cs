using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IListaTareaService
    {
        Task<IReadOnlyList<ListaTareaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int listaTareaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<ListaTareaDto>> CrearAsync(int tableroId, CrearListaTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<ListaTareaDto>> ActualizarAsync(int listaTareaId, ActualizarListaTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> ReordenarAsync(int listaTareaId, ReordenarDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int listaTareaId, CancellationToken cancellationToken = default);
    }
}
