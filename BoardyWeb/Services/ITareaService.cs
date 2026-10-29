using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface ITareaService
    {
        Task<IReadOnlyList<TareaDto>> ListarPorTableroAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> ObtenerAsync(int tareaId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int tareaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> CrearAsync(int listaTareaId, CrearTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> ActualizarAsync(int tareaId, ActualizarTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> MoverAsync(int tareaId, MoverTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> CambiarEstadoAsync(int tareaId, CambiarEstadoTareaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> AsignarMiembroAsync(int tareaId, int tableroMiembroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TareaDto>> QuitarMiembroAsync(int tareaId, int tableroMiembroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int tareaId, CancellationToken cancellationToken = default);
    }
}
