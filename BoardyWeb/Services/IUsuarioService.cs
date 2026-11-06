using BoardyWeb.Comun;
using BoardyWeb.DTOs;
using BoardyWeb.Models;

namespace BoardyWeb.Services
{
    public interface IUsuarioService
    {
        Task<IReadOnlyList<UsuarioAdministracionDto>> ListarAsync(CancellationToken cancellationToken = default);

        /// <summary>Activa o desactiva la cuenta. Nadie puede aplicárselo a sí mismo.</summary>
        Task<ResultadoOperacion> AlternarEstadoAsync(string usuarioId, string usuarioActualId);

        /// <summary>Da o quita el rol global Administrador. Nadie puede aplicárselo a sí mismo.</summary>
        Task<ResultadoOperacion> AlternarAdministradorAsync(string usuarioId, string usuarioActualId);

        /// <summary>
        /// Elimina la cuenta propia: quita membresías e invitaciones y anonimiza al usuario, de modo que
        /// sus comentarios y su auditoría se conservan. No se permite a propietarios de tableros
        /// ni al único administrador global.
        /// </summary>
        Task<ResultadoOperacion> EliminarCuentaAsync(Usuario usuario, CancellationToken cancellationToken = default);
    }
}
