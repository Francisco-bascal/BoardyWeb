using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IEstadoTareaService
    {
        Task<IReadOnlyList<EstadoTareaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int estadoTareaId, CancellationToken cancellationToken = default);

        Task<int> ContarSinListaAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<EstadoTareaDto>> ObtenerAsync(int estadoTareaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<EstadoTareaDto>> CrearAsync(int tableroId, CrearEstadoTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<EstadoTareaDto>> ActualizarAsync(int estadoTareaId, ActualizarEstadoTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int estadoTareaId, CancellationToken cancellationToken = default);
    }
}
