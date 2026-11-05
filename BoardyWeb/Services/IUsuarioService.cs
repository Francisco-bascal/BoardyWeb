using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IUsuarioService
    {
        Task<IReadOnlyList<UsuarioAdministracionDto>> ListarAsync(CancellationToken cancellationToken = default);

        /// <summary>Activa o desactiva la cuenta. Nadie puede aplicárselo a sí mismo.</summary>
        Task<ResultadoOperacion> AlternarEstadoAsync(string usuarioId, string usuarioActualId);

        /// <summary>Da o quita el rol global Administrador. Nadie puede aplicárselo a sí mismo.</summary>
        Task<ResultadoOperacion> AlternarAdministradorAsync(string usuarioId, string usuarioActualId);
    }
}
