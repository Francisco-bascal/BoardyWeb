using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EstadosTareaController : ControladorBase
    {
        private readonly IEstadoTareaService _estadoTareaService;

        public EstadosTareaController(IEstadoTareaService estadoTareaService)
        {
            _estadoTareaService = estadoTareaService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return Vista("_Lista", await _estadoTareaService.ListarAsync(cancellationToken));
        }

        public IActionResult Crear()
        {
            return VistaFormulario(new CrearEstadoTareaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearEstadoTareaDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _estadoTareaService.CrearAsync(dto, cancellationToken);

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return VistaFormulario(dto);
            }

            return Responder(resultado, RedirectToAction(nameof(Index)));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var estado = await _estadoTareaService.ObtenerAsync(id, cancellationToken);
            var noValido = ResultadoNoValido(estado);
            if (noValido is not null)
            {
                return noValido;
            }

            ViewData["EstadoTareaId"] = id;
            return VistaFormulario(new ActualizarEstadoTareaDto { Nombre = estado.Datos!.Nombre, Descripcion = estado.Datos.Descripcion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarEstadoTareaDto dto, CancellationToken cancellationToken)
        {
            ViewData["EstadoTareaId"] = id;

            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _estadoTareaService.ActualizarAsync(id, dto, cancellationToken);
            var noValido = ResultadoNoValido(resultado);
            if (noValido is not null)
            {
                return noValido;
            }

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return VistaFormulario(dto);
            }

            return Responder(resultado, RedirectToAction(nameof(Index)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var resultado = await _estadoTareaService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }
    }
}
