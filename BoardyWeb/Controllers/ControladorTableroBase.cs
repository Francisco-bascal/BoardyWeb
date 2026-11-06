using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    /// <summary>
    /// Base de los controladores cuyas acciones dependen del rol del usuario en un tablero.
    /// </summary>
    public abstract class ControladorTableroBase : ControladorBase
    {
        private readonly IAuthorizationService _autorizacion;

        protected ControladorTableroBase(IAuthorizationService autorizacion)
        {
            _autorizacion = autorizacion;
        }

        // Evalúa una política sobre un tablero.
        protected async Task<bool> TienePermisoAsync(int tableroId, string politica)
        {
            var resultado = await _autorizacion.AuthorizeAsync(User, tableroId, politica);
            return resultado.Succeeded;
        }

        // Evalúa una política sobre una tarea.
        protected async Task<bool> TienePermisoSobreTareaAsync(int tareaId, string politica)
        {
            var resultado = await _autorizacion.AuthorizeAsync(User, tareaId, politica);
            return resultado.Succeeded;
        }

        // Devuelve 404 si el tablero no existe, 403 sin permiso y null si puede continuar.
        protected async Task<IActionResult?> ValidarAccesoAsync(int? tableroId, string politica)
        {
            if (tableroId is null)
            {
                return NotFound();
            }

            return await TienePermisoAsync(tableroId.Value, politica) ? null : Forbid();
        }
    }
}
