using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.DTOs;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuarioService(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IReadOnlyList<UsuarioAdministracionDto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            var administradores = (await _userManager.GetUsersInRoleAsync(RolesGlobales.Administrador))
                .Select(u => u.Id)
                .ToHashSet();

            var usuarios = await _userManager.Users
                .AsNoTracking()
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
