using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IMiembroTableroService
    {
        Task<IReadOnlyList<TableroMiembroDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroMiembroDto>> AgregarAsync(int tableroId, AgregarMiembroDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroMiembroDto>> AsignarRolAsync(int tableroId, int tableroMiembroId, AsignarRolDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroMiembroDto>> QuitarRolAsync(int tableroId, int tableroMiembroId, int rolMiembroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> QuitarAsync(int tableroId, int tableroMiembroId, CancellationToken cancellationToken = default);
    }
}
