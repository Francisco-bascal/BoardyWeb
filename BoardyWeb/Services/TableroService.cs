using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class TableroService : ITableroService
    {
        private readonly AppDbContext _context;

        public TableroService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TableroDto>> ListarDelUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            return await _context.Tableros
                .AsNoTracking()
                .Where(t => t.Miembros.Any(m => m.UsuarioId == usuarioId))
                .OrderBy(t => t.Nombre)
                .Select(t => new TableroDto
                {
                    TableroId = t.TableroId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<TableroDetalleDto>> ObtenerDetalleAsync(int tableroId, string usuarioId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros
                .AsNoTracking()
                .Where(t => t.TableroId == tableroId)
                .Select(t => new TableroDetalleDto
                {
                    TableroId = t.TableroId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    PropietarioNombre = t.Propietario.Nombre + " " + t.Propietario.Apellido,
                    EsPropietario = t.PropietarioId == usuarioId
                })
                .FirstOrDefaultAsync(cancellationToken);

            return tablero is null
                ? ResultadoOperacion<TableroDetalleDto>.NoExiste("El tablero no existe.")
                : ResultadoOperacion<TableroDetalleDto>.Exitoso(tablero);
        }

        public async Task<ResultadoOperacion<TableroDto>> CrearAsync(CrearTableroDto dto, string usuarioId, CancellationToken cancellationToken = default)
        {
            var rolPropietarioId = await _context.RolMiembros
                .Where(r => r.Nombre == RolesTablero.Propietario)
                .Select(r => (int?)r.RolMiembroId)
                .FirstOrDefaultAsync(cancellationToken);

            if (rolPropietarioId is null)
            {
                return ResultadoOperacion<TableroDto>.Fallo("No está configurado el rol Propietario.");
            }

            var ahora = DateTime.UtcNow;
            var tablero = new Tablero
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                PropietarioId = usuarioId
            };

            tablero.Miembros.Add(new TableroMiembro
            {
                UsuarioId = usuarioId,
                FechaAsignacion = ahora,
                Roles = new List<MiembrosPorRol>
                {
                    new() { RolMiembroId = rolPropietarioId.Value, FechaAsignacion = ahora }
                }
            });

            _context.Tableros.Add(tablero);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TableroDto>.Exitoso(Mapear(tablero), "Tablero creado correctamente.");
        }

        public async Task<ResultadoOperacion<TableroDto>> ActualizarAsync(int tableroId, ActualizarTableroDto dto, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion<TableroDto>.NoExiste("El tablero no existe.");
            }

            tablero.Nombre = dto.Nombre.Trim();
            tablero.Descripcion = dto.Descripcion.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion<TableroDto>.Exitoso(Mapear(tablero), "Tablero actualizado correctamente.");
        }

        public async Task<ResultadoOperacion> EliminarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion.NoExiste("El tablero no existe.");
            }

            // Listas, tareas, miembros, etiquetas e invitaciones se eliminan en cascada en la base de datos.
            _context.Tableros.Remove(tablero);
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Tablero eliminado correctamente.");
        }

        public async Task<ResultadoOperacion> TransferirPropiedadAsync(int tableroId, int tableroMiembroDestinoId, string usuarioActualId, CancellationToken cancellationToken = default)
        {
            var tablero = await _context.Tableros.FirstOrDefaultAsync(t => t.TableroId == tableroId, cancellationToken);

            if (tablero is null)
            {
                return ResultadoOperacion.NoExiste("El tablero no existe.");
            }

            if (tablero.PropietarioId != usuarioActualId)
            {
                return ResultadoOperacion.Prohibido("Solo el propietario puede transferir la propiedad del tablero.");
            }

            var destino = await _context.TableroMiembros
                .Include(m => m.Roles).ThenInclude(r => r.RolMiembro)
                .FirstOrDefaultAsync(m => m.TableroMiembroId == tableroMiembroDestinoId && m.TableroId == tableroId, cancellationToken);

            if (destino is null)
            {
                return ResultadoOperacion.NoExiste("El miembro no pertenece al tablero.");
            }

            if (destino.UsuarioId == tablero.PropietarioId)
            {
                return ResultadoOperacion.Fallo("Ese usuario ya es el propietario del tablero.");
            }

            if (!destino.Roles.Any(r => r.RolMiembro.Nombre == RolesTablero.Administrador))
            {
                return ResultadoOperacion.Fallo("Solo se puede transferir la propiedad a un miembro con el rol Administrador.");
            }

            var rolPropietario = await _context.RolMiembros.FirstOrDefaultAsync(r => r.Nombre == RolesTablero.Propietario, cancellationToken);
            var rolAdministrador = await _context.RolMiembros.FirstOrDefaultAsync(r => r.Nombre == RolesTablero.Administrador, cancellationToken);

            if (rolPropietario is null || rolAdministrador is null)
            {
                return ResultadoOperacion.Fallo("No están configurados los roles Propietario y Administrador.");
            }

            var actual = await _context.TableroMiembros
                .Include(m => m.Roles)
                .FirstAsync(m => m.TableroId == tableroId && m.UsuarioId == usuarioActualId, cancellationToken);

            var ahora = DateTime.UtcNow;

            // Cada miembro tiene un único rol. Todo se persiste en un único SaveChanges, de modo que
            // nunca queda un tablero sin propietario: el nuevo propietario pasa a Propietario y el
            // anterior a Administrador.
            tablero.PropietarioId = destino.UsuarioId;

            _context.MiembrosPorRol.RemoveRange(destino.Roles);
            destino.Roles.Add(new MiembrosPorRol { RolMiembroId = rolPropietario.RolMiembroId, FechaAsignacion = ahora });

            _context.MiembrosPorRol.RemoveRange(actual.Roles);
            actual.Roles.Add(new MiembrosPorRol { RolMiembroId = rolAdministrador.RolMiembroId, FechaAsignacion = ahora });

            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("La propiedad del tablero fue transferida. Ahora tenés el rol Administrador.");
        }

        private static TableroDto Mapear(Tablero tablero) => new()
        {
            TableroId = tablero.TableroId,
            Nombre = tablero.Nombre,
            Descripcion = tablero.Descripcion,
            FechaCreacion = tablero.FechaCreacion
        };
    }
}
