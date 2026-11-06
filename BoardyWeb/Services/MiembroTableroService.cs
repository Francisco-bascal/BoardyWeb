using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class MiembroTableroService : IMiembroTableroService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public MiembroTableroService(AppDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<IReadOnlyList<TableroMiembroDto>> ListarAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await ConsultarMiembros(tableroId).ToListAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<TableroMiembroDto>> AgregarAsync(int tableroId, AgregarMiembroDto dto, CancellationToken cancellationToken = default)
        {
            var email = dto.Email.Trim().ToUpperInvariant();

            var usuarioId = await _context.Users
                .Where(u => u.NormalizedEmail == email && u.Estado)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            // Regla de usuario activo: solo se agrega a un usuario existente y activo.
            if (usuarioId is null)
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo(nameof(dto.Email), "No existe un usuario activo con ese email.");
            }

            // No duplicación de miembros: el usuario no puede pertenecer ya al tablero.
            if (await _context.TableroMiembros.AnyAsync(m => m.TableroId == tableroId && m.UsuarioId == usuarioId, cancellationToken))
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo(nameof(dto.Email), "El usuario ya es miembro del tablero.");
            }

            var errorRol = await ValidarRolAsignableAsync(dto.RolMiembroId, cancellationToken);
            if (errorRol is not null)
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo(nameof(dto.RolMiembroId), errorRol);
            }

            var ahora = DateTime.UtcNow;
            var miembro = new TableroMiembro
            {
                TableroId = tableroId,
                UsuarioId = usuarioId,
                FechaAsignacion = ahora,
                Roles = new List<MiembrosPorRol>
                {
                    new() { RolMiembroId = dto.RolMiembroId, FechaAsignacion = ahora }
                }
            };

            _context.TableroMiembros.Add(miembro);
            _auditoria.Registrar(tableroId, $"Agregó a {dto.Email} como miembro.");
            await _context.SaveChangesAsync(cancellationToken);

            return await ObtenerMiembroAsync(tableroId, miembro.TableroMiembroId, "Miembro agregado correctamente.", cancellationToken);
        }

        public async Task<ResultadoOperacion<TableroMiembroDto>> CambiarRolAsync(int tableroId, int tableroMiembroId, AsignarRolDto dto, CancellationToken cancellationToken = default)
        {
            var miembro = await _context.TableroMiembros
                .Include(m => m.Roles)
                .FirstOrDefaultAsync(m => m.TableroMiembroId == tableroMiembroId && m.TableroId == tableroId, cancellationToken);

            if (miembro is null)
            {
                return ResultadoOperacion<TableroMiembroDto>.NoExiste("El miembro no existe en el tablero.");
            }

            // No cambio de rol del propietario: debe transferir la propiedad.
            if (await EsPropietarioAsync(tableroId, miembro.UsuarioId, cancellationToken))
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo("No se puede cambiar el rol del propietario. Transferí la propiedad para dejar de serlo.");
            }

            // No reasignación del mismo rol: el rol nuevo debe ser distinto.
            if (miembro.Roles.Any(r => r.RolMiembroId == dto.RolMiembroId))
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo(nameof(dto.RolMiembroId), "El miembro ya tiene ese rol.");
            }

            var errorRol = await ValidarRolAsignableAsync(dto.RolMiembroId, cancellationToken);
            if (errorRol is not null)
            {
                return ResultadoOperacion<TableroMiembroDto>.Fallo(nameof(dto.RolMiembroId), errorRol);
            }

            // Un miembro tiene un único rol: el nuevo reemplaza al anterior en un solo SaveChanges.
            _context.MiembrosPorRol.RemoveRange(miembro.Roles);
            miembro.Roles.Add(new MiembrosPorRol { RolMiembroId = dto.RolMiembroId, FechaAsignacion = DateTime.UtcNow });
            var nombreMiembro = await _context.Users.Where(u => u.Id == miembro.UsuarioId).Select(u => u.Nombre + " " + u.Apellido).FirstAsync(cancellationToken);
            var nombreRol = await _context.RolMiembros.Where(r => r.RolMiembroId == dto.RolMiembroId).Select(r => r.Nombre).FirstAsync(cancellationToken);
            _auditoria.Registrar(tableroId, $"Cambió el rol de {nombreMiembro} a {nombreRol}.");
            await _context.SaveChangesAsync(cancellationToken);

            return await ObtenerMiembroAsync(tableroId, tableroMiembroId, "Rol actualizado correctamente.", cancellationToken);
        }

        public async Task<ResultadoOperacion> QuitarAsync(int tableroId, int tableroMiembroId, CancellationToken cancellationToken = default)
        {
            var miembro = await _context.TableroMiembros
                .Include(m => m.TareasAsignadas)
                .FirstOrDefaultAsync(m => m.TableroMiembroId == tableroMiembroId && m.TableroId == tableroId, cancellationToken);

            if (miembro is null)
            {
                return ResultadoOperacion.NoExiste("El miembro no existe en el tablero.");
            }

            // No expulsión del propietario: el tablero siempre conserva a su dueño.
            if (await EsPropietarioAsync(tableroId, miembro.UsuarioId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("No se puede quitar al propietario del tablero.");
            }

            // Las asignaciones a tareas tienen borrado restringido, por lo que se quitan junto con el miembro.
            _context.TareaMiembros.RemoveRange(miembro.TareasAsignadas);
            _context.TableroMiembros.Remove(miembro);
            var nombreMiembro = await _context.Users.Where(u => u.Id == miembro.UsuarioId).Select(u => u.Nombre + " " + u.Apellido).FirstAsync(cancellationToken);
            _auditoria.Registrar(tableroId, $"Quitó a {nombreMiembro} del tablero.");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Miembro quitado correctamente.");
        }

        private IQueryable<TableroMiembroDto> ConsultarMiembros(int tableroId, int? tableroMiembroId = null)
        {
            return _context.TableroMiembros
                .AsNoTracking()
                .Where(m => m.TableroId == tableroId && (tableroMiembroId == null || m.TableroMiembroId == tableroMiembroId))
                .OrderBy(m => m.Usuario.Nombre)
                .Select(m => new TableroMiembroDto
                {
                    TableroMiembroId = m.TableroMiembroId,
                    UsuarioId = m.UsuarioId,
                    NombreCompleto = m.Usuario.Nombre + " " + m.Usuario.Apellido,
                    Email = m.Usuario.Email ?? string.Empty,
                    EsPropietario = m.Tablero.PropietarioId == m.UsuarioId,
                    FotoPerfil = m.Usuario.FotoPerfil,
                    Roles = m.Roles
                        .Select(r => new RolMiembroDto
                        {
                            RolMiembroId = r.RolMiembroId,
                            Nombre = r.RolMiembro.Nombre,
                            Descripcion = r.RolMiembro.Descripcion
                        })
                        .ToList()
                });
        }

        private async Task<ResultadoOperacion<TableroMiembroDto>> ObtenerMiembroAsync(int tableroId, int tableroMiembroId, string mensaje, CancellationToken cancellationToken)
        {
            var dto = await ConsultarMiembros(tableroId, tableroMiembroId).FirstAsync(cancellationToken);
            return ResultadoOperacion<TableroMiembroDto>.Exitoso(dto, mensaje);
        }

        private Task<bool> EsPropietarioAsync(int tableroId, string usuarioId, CancellationToken cancellationToken)
        {
            return _context.Tableros.AnyAsync(t => t.TableroId == tableroId && t.PropietarioId == usuarioId, cancellationToken);
        }

        // Devuelve el motivo si el rol no puede asignarse; null si es válido.
        private async Task<string?> ValidarRolAsignableAsync(int rolMiembroId, CancellationToken cancellationToken)
        {
            var nombreRol = await _context.RolMiembros
                .Where(r => r.RolMiembroId == rolMiembroId)
                .Select(r => r.Nombre)
                .FirstOrDefaultAsync(cancellationToken);

            // Regla de rol existente: el rol elegido debe estar en el catálogo.
            if (nombreRol is null)
            {
                return "El rol seleccionado no existe.";
            }

            // No asignación de Propietario: ese rol solo se obtiene por transferencia.
            return nombreRol == RolesTablero.Propietario ? "El rol Propietario no puede asignarse a otros miembros." : null;
        }
    }
}
