using System.Security.Claims;
using BoardyWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Evalúa TrabajoEnTareaRequirement. El recurso de autorización es el TareaId (int).
    /// </summary>
    public class TrabajoEnTareaHandler : AuthorizationHandler<TrabajoEnTareaRequirement, int>
    {
        private readonly AppDbContext _contexto;

        public TrabajoEnTareaHandler(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext contextoAutorizacion,
            TrabajoEnTareaRequirement requisito,
            int tareaId)
        {
            var usuarioId = contextoAutorizacion.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId is null)
            {
                return;
            }

            var tableroId = await _contexto.Tareas
                .Where(t => t.TareaId == tareaId)
                .Select(t => (int?)t.ListaTarea.TableroId)
                .FirstOrDefaultAsync();

            if (tableroId is null)
            {
                return;
            }

            var roles = await _contexto.MiembrosPorRol
                .Where(m => m.TableroMiembro.TableroId == tableroId && m.TableroMiembro.UsuarioId == usuarioId)
                .Select(m => m.RolMiembro.Nombre)
                .ToListAsync();

            if (roles.Contains(RolesTablero.Propietario) || roles.Contains(RolesTablero.Administrador))
            {
                contextoAutorizacion.Succeed(requisito);
                return;
            }

            if (roles.Contains(RolesTablero.Operario)
                && await _contexto.TareaMiembros.AnyAsync(m => m.TareaId == tareaId && m.TableroMiembro.UsuarioId == usuarioId))
            {
                contextoAutorizacion.Succeed(requisito);
            }
        }
    }
}
