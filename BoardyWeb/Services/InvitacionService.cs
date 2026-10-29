using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class InvitacionService : IInvitacionService
    {
        private readonly AppDbContext _context;

        public InvitacionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InvitacionTableroDto>> ListarRecibidasAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            return await Consultar()
                .Where(i => i.UsuarioInvitadoId == usuarioId && i.Estado == EstadoInvitacion.Pendiente)
                .OrderByDescending(i => i.FechaInvitacion)
                .Select(Proyectar())
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InvitacionTableroDto>> ListarDelTableroAsync(int tableroId, CancellationToken cancellationToken = default)
        {
            return await Consultar()
                .Where(i => i.TableroId == tableroId)
                .OrderByDescending(i => i.FechaInvitacion)
                .Select(Proyectar())
                .ToListAsync(cancellationToken);
        }

        public async Task<int?> ObtenerTableroIdAsync(int invitacionTableroId, CancellationToken cancellationToken = default)
        {
            return await _context.InvitacionTableros
                .AsNoTracking()
                .Where(i => i.InvitacionTableroId == invitacionTableroId)
                .Select(i => (int?)i.TableroId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResultadoOperacion<InvitacionTableroDto>> CrearAsync(int tableroId, CrearInvitacionDto dto, string usuarioInvitadorId, CancellationToken cancellationToken = default)
        {
            if (!await _context.Tableros.AnyAsync(t => t.TableroId == tableroId, cancellationToken))
            {
                return ResultadoOperacion<InvitacionTableroDto>.NoExiste("El tablero no existe.");
            }

            var email = dto.Email.Trim().ToUpperInvariant();

            var usuarioInvitadoId = await _context.Users
                .Where(u => u.NormalizedEmail == email && u.Estado)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (usuarioInvitadoId is null)
            {
                return ResultadoOperacion<InvitacionTableroDto>.Fallo(nameof(dto.Email), "No existe un usuario activo con ese email.");
            }

            if (await _context.TableroMiembros.AnyAsync(m => m.TableroId == tableroId && m.UsuarioId == usuarioInvitadoId, cancellationToken))
            {
                return ResultadoOperacion<InvitacionTableroDto>.Fallo(nameof(dto.Email), "El usuario ya es miembro del tablero.");
            }

            if (await _context.InvitacionTableros.AnyAsync(i =>
                    i.TableroId == tableroId && i.UsuarioInvitadoId == usuarioInvitadoId && i.Estado == EstadoInvitacion.Pendiente, cancellationToken))
            {
                return ResultadoOperacion<InvitacionTableroDto>.Fallo(nameof(dto.Email), "El usuario ya tiene una invitación pendiente para este tablero.");
            }

            var invitacion = new InvitacionTablero
            {
                TableroId = tableroId,
                UsuarioInvitadoId = usuarioInvitadoId,
                UsuarioInvitadorId = usuarioInvitadorId,
                Mensaje = dto.Mensaje.Trim(),
                Estado = EstadoInvitacion.Pendiente,
                FechaInvitacion = DateTime.UtcNow
            };

            _context.InvitacionTableros.Add(invitacion);
            await _context.SaveChangesAsync(cancellationToken);

            var resultado = await Consultar()
                .Where(i => i.InvitacionTableroId == invitacion.InvitacionTableroId)
                .Select(Proyectar())
                .FirstAsync(cancellationToken);

            return ResultadoOperacion<InvitacionTableroDto>.Exitoso(resultado, "Invitación enviada correctamente.");
        }

        public async Task<ResultadoOperacion> AceptarAsync(int invitacionTableroId, string usuarioId, CancellationToken cancellationToken = default)
        {
            var invitacion = await _context.InvitacionTableros
                .FirstOrDefaultAsync(i => i.InvitacionTableroId == invitacionTableroId && i.UsuarioInvitadoId == usuarioId, cancellationToken);

            var error = ValidarPendiente(invitacion);
            if (error is not null)
            {
                return error;
            }

            var rolLectorId = await _context.RolMiembros
                .Where(r => r.Nombre == RolesTablero.Lector)
                .Select(r => (int?)r.RolMiembroId)
                .FirstOrDefaultAsync(cancellationToken);

            if (rolLectorId is null)
            {
                return ResultadoOperacion.Fallo("No está configurado el rol Lector.");
            }

            if (await _context.TableroMiembros.AnyAsync(m => m.TableroId == invitacion!.TableroId && m.UsuarioId == usuarioId, cancellationToken))
            {
                return ResultadoOperacion.Fallo("Ya sos miembro de este tablero.");
            }

            var ahora = DateTime.UtcNow;

            // El alta del miembro y el cambio de estado se persisten juntos en un único SaveChanges.
            invitacion!.Estado = EstadoInvitacion.Aceptada;
            _context.TableroMiembros.Add(new TableroMiembro
            {
                TableroId = invitacion.TableroId,
                UsuarioId = usuarioId,
                FechaAsignacion = ahora,
                Roles = new List<MiembrosPorRol>
                {
                    new() { RolMiembroId = rolLectorId.Value, FechaAsignacion = ahora }
                }
            });

            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Invitación aceptada correctamente.");
        }

        public async Task<ResultadoOperacion> RechazarAsync(int invitacionTableroId, string usuarioId, CancellationToken cancellationToken = default)
        {
            var invitacion = await _context.InvitacionTableros
                .FirstOrDefaultAsync(i => i.InvitacionTableroId == invitacionTableroId && i.UsuarioInvitadoId == usuarioId, cancellationToken);

            var error = ValidarPendiente(invitacion);
            if (error is not null)
            {
                return error;
            }

            invitacion!.Estado = EstadoInvitacion.Rechazada;
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Invitación rechazada.");
        }

        public async Task<ResultadoOperacion> CancelarAsync(int invitacionTableroId, CancellationToken cancellationToken = default)
        {
            var invitacion = await _context.InvitacionTableros
                .FirstOrDefaultAsync(i => i.InvitacionTableroId == invitacionTableroId, cancellationToken);

            var error = ValidarPendiente(invitacion);
            if (error is not null)
            {
                return error;
            }

            invitacion!.Estado = EstadoInvitacion.Cancelada;
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Invitación cancelada.");
        }

        private static ResultadoOperacion? ValidarPendiente(InvitacionTablero? invitacion)
        {
            if (invitacion is null)
            {
                return ResultadoOperacion.NoExiste("La invitación no existe.");
            }

            return invitacion.Estado == EstadoInvitacion.Pendiente
                ? null
                : ResultadoOperacion.Fallo("La invitación ya no está pendiente.");
        }

        private IQueryable<InvitacionTablero> Consultar() => _context.InvitacionTableros.AsNoTracking();

        private static System.Linq.Expressions.Expression<Func<InvitacionTablero, InvitacionTableroDto>> Proyectar() =>
            i => new InvitacionTableroDto
            {
                InvitacionTableroId = i.InvitacionTableroId,
                TableroId = i.TableroId,
                TableroNombre = i.Tablero.Nombre,
                UsuarioInvitadorNombre = i.UsuarioInvitador.Nombre + " " + i.UsuarioInvitador.Apellido,
                UsuarioInvitadoNombre = i.UsuarioInvitado.Nombre + " " + i.UsuarioInvitado.Apellido,
                Mensaje = i.Mensaje,
                Estado = i.Estado,
                FechaInvitacion = i.FechaInvitacion
            };
    }
}
