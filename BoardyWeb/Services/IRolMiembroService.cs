using BoardyWeb.Comun;
using BoardyWeb.DTOs;

namespace BoardyWeb.Services
{
    public interface IRolMiembroService
    {
        Task<IReadOnlyList<RolMiembroDto>> ListarAsync(CancellationToken cancellationToken = default);
    }
}
