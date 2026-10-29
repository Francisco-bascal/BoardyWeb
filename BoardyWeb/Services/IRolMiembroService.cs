using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IRolMiembroService
    {
        Task<IReadOnlyList<RolMiembroDto>> ListarAsync(CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<RolMiembroDto>> ObtenerAsync(int rolMiembroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<RolMiembroDto>> CrearAsync(CrearRolMiembroDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<RolMiembroDto>> ActualizarAsync(int rolMiembroId, ActualizarRolMiembroDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int rolMiembroId, CancellationToken cancellationToken = default);
    }
}
