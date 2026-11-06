using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class InvitacionService : IInvitacionService
    {
        private readonly AppDbContext _context;
        private readonly IAuditoriaService _auditoria;
        private readonly IEmailSender _correo;
        private readonly ILogger<InvitacionService> _logger;

        public InvitacionService(AppDbContext context, IAuditoriaService auditoria, IEmailSender correo, ILogger<InvitacionService> logger)
        {
            _context = context;
            _auditoria = auditoria;
            _correo = correo;
            _logger = logger;
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

            // Regla de usuario activo: solo se invita a un usuario existente y activo.
            if (usuarioInvitadoId is null)
            {
                return ResultadoOperacion<InvitacionTableroDto>.Fallo(nameof(dto.Email), "No existe un usuario activo con ese email.");
            }

            // No invitación de miembros: el usuario no puede pertenecer ya al tablero.
            if (await _context.TableroMiembros.AnyAsync(m => m.TableroId == tableroId && m.UsuarioId == usuarioInvitadoId, cancellationToken))
            {
                return ResultadoOperacion<InvitacionTableroDto>.Fallo(nameof(dto.Email), "El usuario ya es miembro del tablero.");
            }

            // Regla de invitación única: no puede tener otra pendiente en el tablero.
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
            _auditoria.Registrar(tableroId, $"Invitó a {dto.Email} al tablero.");
            await _context.SaveChangesAsync(cancellationToken);

            var resultado = await Consultar()
                .Where(i => i.InvitacionTableroId == invitacion.InvitacionTableroId)
                .Select(Proyectar())
                .FirstAsync(cancellationToken);

            // shortcut: envío en segundo plano para no demorar la pantalla; si la app se reinicia justo ahí, ese aviso se pierde (la invitación no).
            _ = Task.Run(() => AvisarPorCorreoAsync(dto.Email.Trim(), resultado), CancellationToken.None);

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

            // Regla de rol inicial: quien acepta entra como Lector, y ese rol debe existir.
            if (rolLectorId is null)
            {
                return ResultadoOperacion.Fallo("No está configurado el rol Lector.");
            }

            // No aceptación duplicada: el usuario no puede ser ya miembro.
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

            _auditoria.Registrar(invitacion.TableroId, "Aceptó la invitación al tablero.");
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
            _auditoria.Registrar(invitacion.TableroId, "Rechazó la invitación al tablero.");
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
            _auditoria.Registrar(invitacion.TableroId, "Canceló una invitación al tablero.");
            await _context.SaveChangesAsync(cancellationToken);

            return ResultadoOperacion.Exitoso("Invitación cancelada.");
        }

        // La invitación ya está guardada y se ve en la app: un fallo del correo no debe anularla.
        private async Task AvisarPorCorreoAsync(string email, InvitacionTableroDto invitacion)
        {
            try
            {
                await _correo.SendEmailAsync(
                    email,
                    $"Te invitaron al tablero {invitacion.TableroNombre}",
                    $"<p>{WebUtility.HtmlEncode(invitacion.UsuarioInvitadorNombre)} te invitó al tablero <strong>{WebUtility.HtmlEncode(invitacion.TableroNombre)}</strong>.</p>" +
                    "<p>Ingresá a Boardy y entrá a <strong>Invitaciones</strong> para responder.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de la invitación {InvitacionId}.", invitacion.InvitacionTableroId);
            }
        }

        // Regla de invitación pendiente: solo se responde o cancela si sigue pendiente.
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
