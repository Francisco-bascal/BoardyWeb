using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IComentarioService
    {
        Task<IReadOnlyList<ComentarioDto>> ListarPorTareaAsync(int tareaId, CancellationToken cancellationToken = default);

        Task<int?> ObtenerTableroIdAsync(int comentarioId, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion<ComentarioDto>> CrearAsync(int tareaId, CrearComentarioDto dto, string usuarioId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina el comentario. Solo su autor puede eliminarlo, salvo que <paramref name="puedeModerar"/> sea verdadero.
        /// </summary>
        Task<ResultadoOperacion> EliminarAsync(int comentarioId, string usuarioId, bool puedeModerar, CancellationToken cancellationToken = default);
    }
}
