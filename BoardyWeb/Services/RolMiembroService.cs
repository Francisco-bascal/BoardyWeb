using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class RolMiembroService : IRolMiembroService
    {
        private static readonly string[] RolesDelSistema =
        {
            RolesTablero.Propietario,
            RolesTablero.Administrador,
            RolesTablero.Operario,
            RolesTablero.Lector
        };

        private readonly AppDbContext _context;

        public RolMiembroService(AppDbContext context)
        {
            _context = context;
        }

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

        public async Task<ResultadoOperacion<RolMiembroDto>> ObtenerAsync(int rolMiembroId, CancellationToken cancellationToken = default)
        {
            var rol = await _context.RolMiembros
                .AsNoTracking()
                .Where(e => e.RolMiembroId == rolMiembroId)
                .Select(e => new RolMiembroDto
                {
                    RolMiembroId = e.RolMiembroId,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    CantidadMiembros = e.Miembros.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            return rol is null
                ? ResultadoOperacion<RolMiembroDto>.NoExiste("El rol de miembro no existe.")
                : ResultadoOperacion<RolMiembroDto>.Exitoso(rol);
        }

        public async Task<ResultadoOperacion<RolMiembroDto>> CrearAsync(CrearRolMiembroDto dto, CancellationToken cancellationToken = default)
        {
            var nombre = dto.Nombre.Trim();

            if (await _context.RolMiembros.AnyAsync(e => e.Nombre == nombre, cancellationToken))
            {
                return ResultadoOperacion<RolMiembroDto>.Fallo(nameof(dto.Nombre), "Ya existe un rol con ese nombre.");
            }

            var rol = new RolMiembro { Nombre = nombre, Descripcion = dto.Descripcion.Trim() };
            _context.RolMiembros.Add(rol);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<RolMiembroDto>.Exitoso(Mapear(rol, 0), "Rol creado correctamente.");
        }

        public async Task<ResultadoOperacion<RolMiembroDto>> ActualizarAsync(int rolMiembroId, ActualizarRolMiembroDto dto, CancellationToken cancellationToken = default)
        {
            var rol = await _context.RolMiembros.FirstOrDefaultAsync(e => e.RolMiembroId == rolMiembroId, cancellationToken);

            if (rol is null)
            {
                return ResultadoOperacion<RolMiembroDto>.NoExiste("El rol de miembro no existe.");
            }

            var nombre = dto.Nombre.Trim();

            if (await _context.RolMiembros.AnyAsync(e => e.Nombre == nombre && e.RolMiembroId != rolMiembroId, cancellationToken))
            {
                return ResultadoOperacion<RolMiembroDto>.Fallo(nameof(dto.Nombre), "Ya existe un rol con ese nombre.");
            }

            rol.Nombre = nombre;
            rol.Descripcion = dto.Descripcion.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            var cantidadMiembros = await _context.MiembrosPorRol.CountAsync(t => t.RolMiembroId == rolMiembroId, cancellationToken);
            return ResultadoOperacion<RolMiembroDto>.Exitoso(Mapear(rol, cantidadMiembros), "Rol actualizado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int rolMiembroId, CancellationToken cancellationToken = default)
        {
            var rol = await _context.RolMiembros.FirstOrDefaultAsync(e => e.RolMiembroId == rolMiembroId, cancellationToken);

            if (rol is null)
            {
                return ResultadoOperacion.NoExiste("El rol de miembro no existe.");
            }

            if (RolesDelSistema.Contains(rol.Nombre))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar un rol utilizado por el sistema.");
            }

            if (await _context.MiembrosPorRol.AnyAsync(t => t.RolMiembroId == rolMiembroId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede eliminar un rol que tiene miembros asignados.");
            }

            _context.RolMiembros.Remove(rol);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Rol eliminado correctamente.");
        }

        private static RolMiembroDto Mapear(RolMiembro rol, int cantidadMiembros) => new()
        {
            RolMiembroId = rol.RolMiembroId,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            CantidadMiembros = cantidadMiembros
        };
    }
}
