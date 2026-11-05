using BoardyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class PanelController : ControladorBase
    {
        private readonly ITareaService _tareaService;
        private readonly IReporteService _reporteService;

        public PanelController(ITareaService tareaService, IReporteService reporteService)
        {
            _tareaService = tareaService;
            _reporteService = reporteService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await _tareaService.ListarAsignadasAsync(UsuarioId, cancellationToken));
        }

        public async Task<IActionResult> ReportePdf(CancellationToken cancellationToken)
        {
            var tareas = await _tareaService.ListarAsignadasAsync(UsuarioId, cancellationToken);
            var pdf = _reporteService.GenerarMisTareas(User.Identity!.Name!, tareas);
            return File(pdf, "application/pdf", "mis-tareas.pdf");
        }
    }
}
