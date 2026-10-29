using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Models.ViewModels;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    public class TablerosController : ControladorTableroBase
    {
        private readonly ITableroService _tableroService;
        private readonly IListaTareaService _listaTareaService;
        private readonly ITareaService _tareaService;

        public TablerosController(
            ITableroService tableroService,
            IListaTareaService listaTareaService,
            ITareaService tareaService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _tableroService = tableroService;
            _listaTareaService = listaTareaService;
            _tareaService = tareaService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return Vista("_Lista", await _tableroService.ListarDelUsuarioAsync(UsuarioId, cancellationToken));
        }

        public async Task<IActionResult> Detalle(int id, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(id, PoliticasTablero.VerTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var tablero = await _tableroService.ObtenerDetalleAsync(id, UsuarioId, cancellationToken);
            var noValido = ResultadoNoValido(tablero);
            if (noValido is not null)
            {
                return noValido;
            }

            var modelo = new TableroKanbanViewModel
            {
                Tablero = tablero.Datos!,
                Listas = await _listaTareaService.ListarAsync(id, cancellationToken),
                Tareas = await _tareaService.ListarPorTableroAsync(id, cancellationToken),
                PuedeColaborar = await TienePermisoAsync(id, PoliticasTablero.ColaborarEnTablero),
                PuedeAdministrar = await TienePermisoAsync(id, PoliticasTablero.AdministrarTablero),
                PuedeGestionar = await TienePermisoAsync(id, PoliticasTablero.GestionarTablero)
            };

            return Vista("_Kanban", modelo);
        }

        public IActionResult Crear()
        {
            return VistaFormulario(new CrearTableroDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearTableroDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _tableroService.CrearAsync(dto, UsuarioId, cancellationToken);

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return VistaFormulario(dto);
            }

            return Responder(resultado, IrATablero(resultado.Datos!.TableroId));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(id, PoliticasTablero.GestionarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var tablero = await _tableroService.ObtenerDetalleAsync(id, UsuarioId, cancellationToken);
            var noValido = ResultadoNoValido(tablero);
            if (noValido is not null)
            {
                return noValido;
            }

            ViewData["TableroId"] = id;
            return VistaFormulario(new ActualizarTableroDto { Nombre = tablero.Datos!.Nombre, Descripcion = tablero.Datos.Descripcion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarTableroDto dto, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(id, PoliticasTablero.GestionarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            ViewData["TableroId"] = id;

            if (!ModelState.IsValid)
            {
                return VistaFormulario(dto);
            }

            var resultado = await _tableroService.ActualizarAsync(id, dto, cancellationToken);
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

            return Responder(resultado, IrATablero(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(id, PoliticasTablero.GestionarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _tableroService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }
    }
}
