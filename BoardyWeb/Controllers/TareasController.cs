using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Models.ViewModels;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoardyWeb.Controllers
{
    public class TareasController : ControladorTableroBase
    {
        private readonly ITareaService _tareaService;
        private readonly IListaTareaService _listaTareaService;
        private readonly IEstadoTareaService _estadoTareaService;
        private readonly IComentarioService _comentarioService;
        private readonly IEtiquetaService _etiquetaService;
        private readonly IMiembroTableroService _miembroService;

        public TareasController(
            ITareaService tareaService,
            IListaTareaService listaTareaService,
            IEstadoTareaService estadoTareaService,
            IComentarioService comentarioService,
            IEtiquetaService etiquetaService,
            IMiembroTableroService miembroService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _miembroService = miembroService;
            _tareaService = tareaService;
            _listaTareaService = listaTareaService;
            _estadoTareaService = estadoTareaService;
            _comentarioService = comentarioService;
            _etiquetaService = etiquetaService;
        }

        public async Task<IActionResult> Detalle(int id, CancellationToken cancellationToken)
        {
            var (error, tableroId) = await AutorizarTareaAsync(id, PoliticasTablero.VerTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var tarea = await _tareaService.ObtenerAsync(id, cancellationToken);
            var noValido = ResultadoNoValido(tarea);
            if (noValido is not null)
            {
                return noValido;
            }

            var modelo = new TareaDetalleViewModel
            {
                Tarea = tarea.Datos!,
                Comentarios = await _comentarioService.ListarPorTareaAsync(id, cancellationToken),
                Estados = await _estadoTareaService.ListarAsync(cancellationToken),
                EtiquetasDelTablero = await _etiquetaService.ListarAsync(tableroId, cancellationToken),
                MiembrosDelTablero = await _miembroService.ListarAsync(tableroId, cancellationToken),
                PuedeColaborar = await TienePermisoAsync(tableroId, PoliticasTablero.ColaborarEnTablero),
                PuedeAdministrar = await TienePermisoAsync(tableroId, PoliticasTablero.AdministrarTablero)
            };

            return Vista("_Detalle", modelo);
        }

        public async Task<IActionResult> Crear(int listaTareaId, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(listaTareaId, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            await CargarSelectoresAsync(cancellationToken);
            ViewData["ListaTareaId"] = listaTareaId;
            return VistaFormulario(new CrearTareaDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int listaTareaId, CrearTareaDto dto, CancellationToken cancellationToken)
        {
            var tableroId = await _listaTareaService.ObtenerTableroIdAsync(listaTareaId, cancellationToken);
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            if (!ModelState.IsValid)
            {
                return await VolverAFormularioCrearAsync(listaTareaId, dto, cancellationToken);
            }

            var resultado = await _tareaService.CrearAsync(listaTareaId, dto, cancellationToken);
            var noValido = ResultadoNoValido(resultado);
            if (noValido is not null)
            {
                return noValido;
            }

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return await VolverAFormularioCrearAsync(listaTareaId, dto, cancellationToken);
            }

            return Responder(resultado, IrATablero(tableroId!.Value));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.AdministrarTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var tarea = await _tareaService.ObtenerAsync(id, cancellationToken);
            var noValido = ResultadoNoValido(tarea);
            if (noValido is not null)
            {
                return noValido;
            }

            await CargarSelectoresAsync(cancellationToken);
            ViewData["TareaId"] = id;

            return VistaFormulario(new ActualizarTareaDto
            {
                Titulo = tarea.Datos!.Titulo,
                Descripcion = tarea.Datos.Descripcion,
                FechaVencimiento = tarea.Datos.FechaVencimiento,
                Prioridad = tarea.Datos.Prioridad,
                EstadoTareaId = tarea.Datos.EstadoTareaId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarTareaDto dto, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.AdministrarTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            if (!ModelState.IsValid)
            {
                return await VolverAFormularioEditarAsync(id, dto, cancellationToken);
            }

            var resultado = await _tareaService.ActualizarAsync(id, dto, cancellationToken);
            var noValido = ResultadoNoValido(resultado);
            if (noValido is not null)
            {
                return noValido;
            }

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return await VolverAFormularioEditarAsync(id, dto, cancellationToken);
            }

            return Responder(resultado, IrATarea(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var (error, tableroId) = await AutorizarTareaAsync(id, PoliticasTablero.AdministrarTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var resultado = await _tareaService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, IrATablero(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mover(int id, MoverTareaDto dto, CancellationToken cancellationToken)
        {
            var (error, tableroId) = await AutorizarTareaAsync(id, PoliticasTablero.ColaborarEnTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(IrATablero(tableroId));
            }

            var resultado = await _tareaService.MoverAsync(id, dto, cancellationToken);
            return Responder(resultado, IrATablero(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoTareaDto dto, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.ColaborarEnTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(IrATarea(id));
            }

            var resultado = await _tareaService.CambiarEstadoAsync(id, dto, cancellationToken);
            return Responder(resultado, IrATarea(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarEtiqueta(int id, int etiquetaId, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.ColaborarEnTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var resultado = await _etiquetaService.AsignarATareaAsync(id, etiquetaId, cancellationToken);
            return Responder(resultado, IrATarea(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarEtiqueta(int id, int etiquetaId, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.ColaborarEnTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var resultado = await _etiquetaService.QuitarDeTareaAsync(id, etiquetaId, cancellationToken);
            return Responder(resultado, IrATarea(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarMiembro(int id, int tableroMiembroId, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.AdministrarTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var resultado = await _tareaService.AsignarMiembroAsync(id, tableroMiembroId, cancellationToken);
            return Responder(resultado, IrATarea(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarMiembro(int id, int tableroMiembroId, CancellationToken cancellationToken)
        {
            var (error, _) = await AutorizarTareaAsync(id, PoliticasTablero.AdministrarTablero, cancellationToken);
            if (error is not null)
            {
                return error;
            }

            var resultado = await _tareaService.QuitarMiembroAsync(id, tableroMiembroId, cancellationToken);
            return Responder(resultado, IrATarea(id));
        }

        private async Task<(IActionResult? Error, int TableroId)> AutorizarTareaAsync(int tareaId, string politica, CancellationToken cancellationToken)
        {
            var tableroId = await _tareaService.ObtenerTableroIdAsync(tareaId, cancellationToken);
            var error = await ValidarAccesoAsync(tableroId, politica);
            return (error, tableroId ?? 0);
        }

        private async Task CargarSelectoresAsync(CancellationToken cancellationToken)
        {
            var estados = await _estadoTareaService.ListarAsync(cancellationToken);
            ViewData["Estados"] = new SelectList(estados, nameof(EstadoTareaDto.EstadoTareaId), nameof(EstadoTareaDto.Nombre));
        }

        private async Task<IActionResult> VolverAFormularioCrearAsync(int listaTareaId, CrearTareaDto dto, CancellationToken cancellationToken)
        {
            await CargarSelectoresAsync(cancellationToken);
            ViewData["ListaTareaId"] = listaTareaId;
            return VistaFormulario(dto);
        }

        private async Task<IActionResult> VolverAFormularioEditarAsync(int tareaId, ActualizarTareaDto dto, CancellationToken cancellationToken)
        {
            await CargarSelectoresAsync(cancellationToken);
            ViewData["TareaId"] = tareaId;
            return VistaFormulario(dto);
        }
    }
}
