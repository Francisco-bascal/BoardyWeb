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
            //Obtención del usuario
            var usuarioId = contextoAutorizacion.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId is null)
            {
                return;
            }

            //Obtención del tablero
            var tableroId = await _contexto.Tareas
                .Where(t => t.TareaId == tareaId)
                .Select(t => (int?)t.ListaTarea.TableroId)
                .FirstOrDefaultAsync();

            if (tableroId is null)
            {
                return;
            }

            //Se obtienen los roles de ese usuario en ese tablero
            var roles = await _contexto.MiembrosPorRol
                .Where(m => m.TableroMiembro.TableroId == tableroId && m.TableroMiembro.UsuarioId == usuarioId)
                .Select(m => m.RolMiembro.Nombre)
                .ToListAsync();

            //Si es propietario o admin puede siempre
            if (roles.Contains(RolesTablero.Propietario) || roles.Contains(RolesTablero.Administrador))
            {
                contextoAutorizacion.Succeed(requisito);
                return;
            }

            //Si es operario solo puede si está asignado a la tarea
            if (roles.Contains(RolesTablero.Operario)
                && await _contexto.TareaMiembros.AnyAsync(m => m.TareaId == tareaId && m.TableroMiembro.UsuarioId == usuarioId))
            {
                contextoAutorizacion.Succeed(requisito);
            }
        }
    }
}
