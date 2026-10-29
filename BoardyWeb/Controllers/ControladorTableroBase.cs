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

        protected async Task<bool> TienePermisoAsync(int tableroId, string politica)
        {
            var resultado = await _autorizacion.AuthorizeAsync(User, tableroId, politica);
            return resultado.Succeeded;
        }

        /// <summary>
        /// Devuelve 404 si el tablero no pudo resolverse, 403 si el usuario no cumple la política;
        /// null si puede continuar.
        /// </summary>
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
