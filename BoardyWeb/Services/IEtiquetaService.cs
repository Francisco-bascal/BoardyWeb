using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IEtiquetaService
    {
        Task<IReadOnlyList<EtiquetaDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int etiquetaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<EtiquetaDto>> CrearAsync(int tableroId, CrearEtiquetaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<EtiquetaDto>> ActualizarAsync(int etiquetaId, ActualizarEtiquetaDto dto, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> EliminarAsync(int etiquetaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> AsignarATareaAsync(int tareaId, int etiquetaId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> QuitarDeTareaAsync(int tareaId, int etiquetaId, CancellationToken cancellationToken = default);
    }
}
