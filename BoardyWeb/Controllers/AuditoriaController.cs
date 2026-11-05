using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoardyWeb.Controllers
{
    public class AuditoriaController : ControladorTableroBase
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly IMiembroTableroService _miembroService;

        public AuditoriaController(
            IAuditoriaService auditoriaService,
            IMiembroTableroService miembroService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _auditoriaService = auditoriaService;
            _miembroService = miembroService;
        }

        public async Task<IActionResult> Index(int tableroId, string? usuarioId, int pagina = 1, CancellationToken cancellationToken = default)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            // shortcut: el filtro lista a los miembros actuales; un ex miembro solo se ve sin filtrar.
            var miembros = await _miembroService.ListarAsync(tableroId, cancellationToken);

            ViewData["TableroId"] = tableroId;
            ViewData["UsuarioId"] = usuarioId;
            ViewData["Miembros"] = new SelectList(miembros, nameof(TableroMiembroDto.UsuarioId), nameof(TableroMiembroDto.NombreCompleto), usuarioId);

            return View(await _auditoriaService.ListarAsync(tableroId, usuarioId, pagina, cancellationToken));
        }
    }
}
