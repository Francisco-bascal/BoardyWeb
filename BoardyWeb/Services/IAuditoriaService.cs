using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(int tableroId, string usuarioId, string accion, CancellationToken cancellationToken = default);

        Task<Pagina<RegistroAuditoriaDto>> ListarAsync(int tableroId, string? usuarioId, int pagina, CancellationToken cancellationToken = default);
    }
}
