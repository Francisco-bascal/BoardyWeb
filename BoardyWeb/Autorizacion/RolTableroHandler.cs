using System.Security.Claims;
using BoardyWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Evalúa RolTableroRequirement consultando los roles del usuario en el tablero.
    /// El recurso de autorización es el TableroId (int).
    /// </summary>
    public class RolTableroHandler : AuthorizationHandler<RolTableroRequirement, int>
    {
        private readonly AppDbContext _contexto;

        public RolTableroHandler(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext contextoAutorizacion,
            RolTableroRequirement requisito,
            int tableroId)
        {
            var usuarioId = contextoAutorizacion.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId is null)
            {
                return;
            }

            var tieneRol = await _contexto.MiembrosPorRol.AnyAsync(m =>
                m.TableroMiembro.TableroId == tableroId
                && m.TableroMiembro.UsuarioId == usuarioId
                && requisito.RolesPermitidos.Contains(m.RolMiembro.Nombre));

            if (tieneRol)
            {
                contextoAutorizacion.Succeed(requisito);
            }
        }
    }
}
