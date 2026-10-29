using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class EtiquetasController : ControladorTableroBase
    {
        private readonly IEtiquetaService _etiquetaService;

        public EtiquetasController(IEtiquetaService etiquetaService, IAuthorizationService autorizacion)
            : base(autorizacion)
        {
            _etiquetaService = etiquetaService;
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
            return Vista("_Lista", await _etiquetaService.ListarAsync(tableroId, cancellationToken));
        }

        public async Task<IActionResult> Crear(int tableroId)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new CrearEtiquetaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tableroId, CrearEtiquetaDto dto, CancellationToken cancellationToken)
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

            var resultado = await _etiquetaService.CrearAsync(tableroId, dto, cancellationToken);
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
            var tableroId = await _etiquetaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var etiqueta = (await _etiquetaService.ListarAsync(tableroId!.Value, cancellationToken))
                .First(e => e.EtiquetaId == id);

            ViewData["EtiquetaId"] = id;
            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new ActualizarEtiquetaDto
            {
                Nombre = etiqueta.Nombre,
                Descripcion = etiqueta.Descripcion,
                Color = etiqueta.Color
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarEtiquetaDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _etiquetaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["EtiquetaId"] = id;
            ViewData["TableroId"] = tableroId;

            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _etiquetaService.ActualizarAsync(id, dto, cancellationToken);
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
            var tableroId = await _etiquetaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _etiquetaService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index), new { tableroId }));
        }
    }
}
