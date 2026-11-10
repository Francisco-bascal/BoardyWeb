using BoardyWeb.Data;
using BoardyWeb.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Roles de miembro de tablero: son fijos (Propietario, Administrador, Operario y Lector) y solo se listan.
    /// </summary>
    public class RolMiembroService : IRolMiembroService
    {
        private readonly AppDbContext _context;

        public RolMiembroService(AppDbContext context)
        {
            _context = context;
        }

        // Roles para elegir al agregar o cambiar un miembro.
        public async Task<IReadOnlyList<RolMiembroDto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return await _context.RolMiembros
                .AsNoTracking()
                .OrderBy(e => e.RolMiembroId)
                .Select(e => new RolMiembroDto
                {
                    RolMiembroId = e.RolMiembroId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    CantidadMiembros = e.Miembros.Count
                })
                .ToListAsync(cancellationToken);
        }
    }
}
