using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IAsistenteService
    {
        Task<ResultadoOperacion<RespuestaAsistenteDto>> PreguntarAsync(string usuarioId, PreguntaAsistenteDto pregunta, CancellationToken cancellationToken = default);
    }
}
