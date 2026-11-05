using BoardyWeb.Autorizacion;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    [Authorize(Roles = RolesGlobales.Administrador)]
    public class PlataformaController : ControladorBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRegistroErrorService _registroErrorService;

        public PlataformaController(IUsuarioService usuarioService, IRegistroErrorService registroErrorService)
        {
            _usuarioService = usuarioService;
            _registroErrorService = registroErrorService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            ViewData["UsuarioActualId"] = UsuarioId;
            return Vista("_ListaUsuarios", await _usuarioService.ListarAsync(cancellationToken));
        }

        public async Task<IActionResult> Logs(int pagina = 1, CancellationToken cancellationToken = default)
        {
            return View(await _registroErrorService.ListarAsync(pagina, cancellationToken));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(string id)
        {
            var resultado = await _usuarioService.AlternarEstadoAsync(id, UsuarioId);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarAdministrador(string id)
        {
            var resultado = await _usuarioService.AlternarAdministradorAsync(id, UsuarioId);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }
    }
}
