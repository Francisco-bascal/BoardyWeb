using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    /// <summary>
    /// Estados de tarea de un tablero: todos los miembros los ven; Propietario y Administrador los gestionan.
    /// </summary>
    public class EstadosTareaController : ControladorTableroBase
    {
        private readonly IEstadoTareaService _estadoTareaService;

        public EstadosTareaController(IEstadoTareaService estadoTareaService, IAuthorizationService autorizacion)
            : base(autorizacion)
        {
            _estadoTareaService = estadoTareaService;
        }

        public async Task<IActionResult> Index(int tableroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.VerTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;
            ViewData["PuedeAdministrar"] = await TienePermisoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            return Vista("_Lista", await _estadoTareaService.ListarAsync(tableroId, cancellationToken));
        }

        public async Task<IActionResult> Crear(int tableroId)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new CrearEstadoTareaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tableroId, CrearEstadoTareaDto dto, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;

            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _estadoTareaService.CrearAsync(tableroId, dto, cancellationToken);
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

            return Responder(resultado, RedirectToAction(nameof(Index), new { tableroId }));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var tableroId = await _estadoTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var estado = await _estadoTareaService.ObtenerAsync(id, cancellationToken);

            ViewData["EstadoTareaId"] = id;
            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new ActualizarEstadoTareaDto { Nombre = estado.Datos!.Nombre, Descripcion = estado.Datos.Descripcion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarEstadoTareaDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _estadoTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["EstadoTareaId"] = id;
            ViewData["TableroId"] = tableroId;

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

            return Responder(resultado, RedirectToAction(nameof(Index), new { tableroId }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var tableroId = await _estadoTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _estadoTareaService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index), new { tableroId }));
        }
    }
}
