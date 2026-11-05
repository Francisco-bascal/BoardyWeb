using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IAuditoriaService
    {
        /// <summary>
        /// Agrega el registro al contexto sin guardarlo: se persiste en el SaveChangesAsync de la
        /// operación, de modo que la acción y su registro quedan en la misma transacción.
        /// </summary>
        void Registrar(int tableroId, string accion);

        Task<Pagina<RegistroAuditoriaDto>> ListarAsync(int tableroId, string? usuarioId, int pagina, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RegistroAuditoriaDto>> ListarTodoAsync(int tableroId, string? usuarioId, CancellationToken cancellationToken = default);
    }
}
