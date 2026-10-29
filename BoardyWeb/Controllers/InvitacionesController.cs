using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class InvitacionesController : ControladorTableroBase
    {
        private readonly IInvitacionService _invitacionService;

        public InvitacionesController(IInvitacionService invitacionService, IAuthorizationService autorizacion)
            : base(autorizacion)
        {
            _invitacionService = invitacionService;
        }

        /// <summary>Invitaciones pendientes recibidas por el usuario actual.</summary>
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await _invitacionService.ListarRecibidasAsync(UsuarioId, cancellationToken));
        }

        /// <summary>Invitaciones enviadas desde un tablero.</summary>
        public async Task<IActionResult> DelTablero(int tableroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;
            return View(await _invitacionService.ListarDelTableroAsync(tableroId, cancellationToken));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tableroId, CrearInvitacionDto dto, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var destino = RedirectToAction(nameof(DelTablero), new { tableroId });

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(destino);
            }

            var resultado = await _invitacionService.CrearAsync(tableroId, dto, UsuarioId, cancellationToken);
            return Responder(resultado, destino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int id, CancellationToken cancellationToken)
        {
            var resultado = await _invitacionService.AceptarAsync(id, UsuarioId, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, CancellationToken cancellationToken)
        {
            var resultado = await _invitacionService.RechazarAsync(id, UsuarioId, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
        {
            var tableroId = await _invitacionService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _invitacionService.CancelarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(DelTablero), new { tableroId }));
        }
    }
}
