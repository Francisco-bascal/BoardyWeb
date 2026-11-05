using BoardyWeb.Comun;
using BoardyWeb.Models;

namespace BoardyWeb.Services
{
    public interface IFotoPerfilService
    {
        Task<ResultadoOperacion> GuardarAsync(Usuario usuario, IFormFile archivo, CancellationToken cancellationToken = default);

        Task<ResultadoOperacion> QuitarAsync(Usuario usuario);
    }
}
