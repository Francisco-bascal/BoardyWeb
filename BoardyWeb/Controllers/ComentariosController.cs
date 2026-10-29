using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class ComentariosController : ControladorTableroBase
    {
        private readonly IComentarioService _comentarioService;
        private readonly ITareaService _tareaService;

        public ComentariosController(
            IComentarioService comentarioService,
            ITareaService tareaService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _comentarioService = comentarioService;
            _tareaService = tareaService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tareaId, CrearComentarioDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _tareaService.ObtenerTableroIdAsync(tareaId, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.ColaborarEnTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(IrATarea(tareaId));
            }

            var resultado = await _comentarioService.CrearAsync(tareaId, dto, UsuarioId, cancellationToken);
            return Responder(resultado, IrATarea(tareaId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, int tareaId, CancellationToken cancellationToken)
        {
            var tableroId = await _comentarioService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.ColaborarEnTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var puedeModerar = await TienePermisoAsync(tableroId!.Value, PoliticasTablero.AdministrarTablero);
            var resultado = await _comentarioService.EliminarAsync(id, UsuarioId, puedeModerar, cancellationToken);
            return Responder(resultado, IrATarea(tareaId));
        }
    }
}
