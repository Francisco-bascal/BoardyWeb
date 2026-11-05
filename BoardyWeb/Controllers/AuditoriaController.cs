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
        private readonly ITableroService _tableroService;
        private readonly IReporteService _reporteService;

        public AuditoriaController(
            IAuditoriaService auditoriaService,
            IMiembroTableroService miembroService,
            ITableroService tableroService,
            IReporteService reporteService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _auditoriaService = auditoriaService;
            _miembroService = miembroService;
            _tableroService = tableroService;
            _reporteService = reporteService;
        }

        public async Task<IActionResult> ReportePdf(int tableroId, string? usuarioId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var tablero = await _tableroService.ObtenerDetalleAsync(tableroId, UsuarioId, cancellationToken);
            var noValido = ResultadoNoValido(tablero);
            if (noValido is not null)
            {
                return noValido;
            }

            var registros = await _auditoriaService.ListarTodoAsync(tableroId, usuarioId, cancellationToken);
            var pdf = _reporteService.GenerarAuditoria(tablero.Datos!.Nombre, registros);
            return File(pdf, "application/pdf", $"auditoria-tablero-{tableroId}.pdf");
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
