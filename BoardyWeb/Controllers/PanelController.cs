using BoardyWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class PanelController : ControladorBase
    {
        private readonly ITareaService _tareaService;

        public PanelController(ITareaService tareaService)
        {
            _tareaService = tareaService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await _tareaService.ListarAsignadasAsync(UsuarioId, cancellationToken));
        }
    }
}
