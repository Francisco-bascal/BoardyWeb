using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class ListasTareaController : ControladorTableroBase
    {
        private readonly IListaTareaService _listaTareaService;

        public ListasTareaController(IListaTareaService listaTareaService, IAuthorizationService autorizacion)
            : base(autorizacion)
        {
            _listaTareaService = listaTareaService;
        }

        public async Task<IActionResult> Crear(int tableroId)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new CrearListaTareaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tableroId, CrearListaTareaDto dto, CancellationToken cancellationToken)
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

            var resultado = await _listaTareaService.CrearAsync(tableroId, dto, cancellationToken);
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

            return Responder(resultado, IrATablero(tableroId));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var lista = (await _listaTareaService.ListarAsync(tableroId!.Value, cancellationToken))
                .First(l => l.ListaTareaId == id);

            ViewData["ListaTareaId"] = id;
            ViewData["TableroId"] = tableroId;
            return VistaFormulario(new ActualizarListaTareaDto { Nombre = lista.Nombre, Descripcion = lista.Descripcion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarListaTareaDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["ListaTareaId"] = id;
            ViewData["TableroId"] = tableroId;

            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _listaTareaService.ActualizarAsync(id, dto, cancellationToken);
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

            return Responder(resultado, IrATablero(tableroId!.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reordenar(int id, ReordenarDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(IrATablero(tableroId!.Value));
            }

            var resultado = await _listaTareaService.ReordenarAsync(id, dto, cancellationToken);
            return Responder(resultado, IrATablero(tableroId!.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(id, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _listaTareaService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, IrATablero(tableroId!.Value));
        }
    }
}
