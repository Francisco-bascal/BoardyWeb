using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _context;
        private readonly IFotoPerfilService _fotoPerfilService;

        public UsuarioService(UserManager<Usuario> userManager, AppDbContext context, IFotoPerfilService fotoPerfilService)
        {
            _userManager = userManager;
            _context = context;
            _fotoPerfilService = fotoPerfilService;
        }

        public async Task<IReadOnlyList<UsuarioAdministracionDto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            var administradores = (await _userManager.GetUsersInRoleAsync(RolesGlobales.Administrador))
                .Select(u => u.Id)
                .ToHashSet();

            var usuarios = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Email != null) // las cuentas eliminadas quedan anonimizadas, sin correo
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .Select(u => new UsuarioAdministracionDto
                {
                    UsuarioId = u.Id,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email ?? string.Empty,
                    FechaRegistro = u.FechaRegistro,
                    Activo = u.Estado
                })
                .ToListAsync(cancellationToken);

            usuarios.ForEach(u => u.EsAdministrador = administradores.Contains(u.UsuarioId));
            return usuarios;
        }

        public async Task<ResultadoOperacion> AlternarEstadoAsync(string usuarioId, string usuarioActualId)
        {
            var (usuario, error) = await ObtenerOtroUsuarioAsync(usuarioId, usuarioActualId);
            if (error is not null)
            {
                return error;
            }

            usuario!.Estado = !usuario.Estado;

            // Cambiar el sello guarda al usuario y cierra sus sesiones abiertas.
            var resultado = await _userManager.UpdateSecurityStampAsync(usuario);

            return resultado.Succeeded
                ? ResultadoOperacion.Exitoso(usuario.Estado ? "Usuario activado." : "Usuario desactivado.")
                : ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        public async Task<ResultadoOperacion> AlternarAdministradorAsync(string usuarioId, string usuarioActualId)
        {
            var (usuario, error) = await ObtenerOtroUsuarioAsync(usuarioId, usuarioActualId);
            if (error is not null)
            {
                return error;
            }

            var eraAdministrador = await _userManager.IsInRoleAsync(usuario!, RolesGlobales.Administrador);

            var resultado = eraAdministrador
                ? await _userManager.RemoveFromRoleAsync(usuario!, RolesGlobales.Administrador)
                : await _userManager.AddToRoleAsync(usuario!, RolesGlobales.Administrador);

            if (!resultado.Succeeded)
            {
                return ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
            }

            await _userManager.UpdateSecurityStampAsync(usuario!);

            return ResultadoOperacion.Exitoso(eraAdministrador ? "Se quitó el rol Administrador." : "Se asignó el rol Administrador.");
        }

        public async Task<ResultadoOperacion> EliminarCuentaAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            if (await _context.Tableros.AnyAsync(t => t.PropietarioId == usuario.Id, cancellationToken))
            {
                return ResultadoOperacion.Fallo("Sos propietario de al menos un tablero: transferí la propiedad o eliminalo antes de borrar tu cuenta.");
            }

            var esAdministrador = await _userManager.IsInRoleAsync(usuario, RolesGlobales.Administrador);
            if (esAdministrador && (await _userManager.GetUsersInRoleAsync(RolesGlobales.Administrador)).Count == 1)
            {
                return ResultadoOperacion.Fallo("Sos el único administrador de la plataforma: asigná otro antes de borrar tu cuenta.");
            }

            if (usuario.FotoPerfil is not null)
            {
                await _fotoPerfilService.QuitarAsync(usuario);
            }

            _context.TareaMiembros.RemoveRange(await _context.TareaMiembros.Where(m => m.TableroMiembro.UsuarioId == usuario.Id).ToListAsync(cancellationToken));
            _context.TableroMiembros.RemoveRange(await _context.TableroMiembros.Where(m => m.UsuarioId == usuario.Id).ToListAsync(cancellationToken));
            _context.InvitacionTableros.RemoveRange(await _context.InvitacionTableros
                .Where(i => i.UsuarioInvitadoId == usuario.Id || i.UsuarioInvitadorId == usuario.Id)
                .ToListAsync(cancellationToken));

            if (esAdministrador)
            {
                await _userManager.RemoveFromRoleAsync(usuario, RolesGlobales.Administrador);
            }

            usuario.Nombre = "Usuario";
            usuario.Apellido = "eliminado";
            usuario.UserName = $"eliminado-{Guid.NewGuid():N}";
            usuario.Email = null;
            usuario.EmailConfirmed = false;
            usuario.PhoneNumber = null;
            usuario.PasswordHash = null;
            usuario.Estado = false;

            // Cambiar el sello guarda todo lo anterior y cierra las sesiones abiertas.
            var resultado = await _userManager.UpdateSecurityStampAsync(usuario);

            return resultado.Succeeded
                ? ResultadoOperacion.Exitoso("Tu cuenta fue eliminada.")
                : ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        private async Task<(Usuario? Usuario, ResultadoOperacion? Error)> ObtenerOtroUsuarioAsync(string usuarioId, string usuarioActualId)
        {
            if (usuarioId == usuarioActualId)
            {
                return (null, ResultadoOperacion.Fallo("No podés aplicar esta acción sobre tu propia cuenta."));
            }

            var usuario = await _userManager.FindByIdAsync(usuarioId);

            return usuario is null
                ? (null, ResultadoOperacion.NoExiste("El usuario no existe."))
                : (usuario, null);
        }
    }
}
