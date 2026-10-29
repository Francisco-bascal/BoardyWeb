using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IInvitacionService
    {
        Task<IReadOnlyList<InvitacionTableroDto>> ListarRecibidasAsync(string usuarioId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InvitacionTableroDto>> ListarDelTableroAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int invitacionTableroId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<InvitacionTableroDto>> CrearAsync(int tableroId, CrearInvitacionDto dto, string usuarioInvitadorId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> AceptarAsync(int invitacionTableroId, string usuarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> RechazarAsync(int invitacionTableroId, string usuarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> CancelarAsync(int invitacionTableroId, CancellationToken cancellationToken = default);
    }
}
