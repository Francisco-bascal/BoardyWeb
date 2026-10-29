using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface ITableroService
    {
        Task<IReadOnlyList<TableroDto>> ListarDelUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroDetalleDto>> ObtenerDetalleAsync(int tableroId, string usuarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroDto>> CrearAsync(CrearTableroDto dto, string usuarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<TableroDto>> ActualizarAsync(int tableroId, ActualizarTableroDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int tableroId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transfiere la propiedad del tablero a otro miembro que tenga el rol Administrador.
        /// Solo puede hacerlo el propietario actual, que pasa a ser Administrador.
        /// </summary>
        Task<ResultadoOperacion> TransferirPropiedadAsync(int tableroId, int tableroMiembroDestinoId, string usuarioActualId, CancellationToken cancellationToken = default);
    }
}
