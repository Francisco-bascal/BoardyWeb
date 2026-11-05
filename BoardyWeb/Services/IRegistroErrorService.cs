using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IRegistroErrorService
    {
        Task RegistrarAsync(string mensaje, string detalle, string ruta, string? usuarioId, CancellationToken cancellationToken = default);

        Task<Pagina<RegistroErrorDto>> ListarAsync(int pagina, CancellationToken cancellationToken = default);
    }
}
