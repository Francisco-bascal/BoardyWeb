using BoardyWeb.Autorizacion;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoardyWeb.Controllers
{
    public class MiembrosController : ControladorTableroBase
    {
        private readonly IMiembroTableroService _miembroService;
        private readonly IRolMiembroService _rolMiembroService;
        private readonly ITableroService _tableroService;

        public MiembrosController(
            IMiembroTableroService miembroService,
            IRolMiembroService rolMiembroService,
            ITableroService tableroService,
            IAuthorizationService autorizacion) : base(autorizacion)
        {
            _tableroService = tableroService;
            _miembroService = miembroService;
            _rolMiembroService = rolMiembroService;
        }

        public async Task<IActionResult> Index(int tableroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.VerTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            // El rol Propietario no se asigna desde los selectores: solo se obtiene transfiriendo la propiedad.
            var roles = (await _rolMiembroService.ListarAsync(cancellationToken))
                .Where(r => r.Nombre != RolesTablero.Propietario)
                .ToList();
            ViewData["TableroId"] = tableroId;
            ViewData["Roles"] = new SelectList(roles, nameof(RolMiembroDto.RolMiembroId), nameof(RolMiembroDto.Nombre));
            ViewData["PuedeAdministrar"] = await TienePermisoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            ViewData["PuedeGestionar"] = await TienePermisoAsync(tableroId, PoliticasTablero.GestionarTablero);

            return Vista("_Lista", await _miembroService.ListarAsync(tableroId, cancellationToken));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int tableroId, AgregarMiembroDto dto, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(VolverAIndex(tableroId));
            }

            var resultado = await _miembroService.AgregarAsync(tableroId, dto, cancellationToken);
            return Responder(resultado, VolverAIndex(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(int tableroId, int tableroMiembroId, AsignarRolDto dto, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            if (!ModelState.IsValid)
            {
                return DatosInvalidos(VolverAIndex(tableroId));
            }

            var resultado = await _miembroService.AsignarRolAsync(tableroId, tableroMiembroId, dto, cancellationToken);
            return Responder(resultado, VolverAIndex(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarRol(int tableroId, int tableroMiembroId, int rolMiembroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _miembroService.QuitarRolAsync(tableroId, tableroMiembroId, rolMiembroId, cancellationToken);
            return Responder(resultado, VolverAIndex(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quitar(int tableroId, int tableroMiembroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.AdministrarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _miembroService.QuitarAsync(tableroId, tableroMiembroId, cancellationToken);
            return Responder(resultado, VolverAIndex(tableroId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferirPropiedad(int tableroId, int tableroMiembroId, CancellationToken cancellationToken)
        {
            var acceso = await ValidarAccesoAsync(tableroId, PoliticasTablero.GestionarTablero);
            if (acceso is not null)
            {
                return acceso;
            }

            var resultado = await _tableroService.TransferirPropiedadAsync(tableroId, tableroMiembroId, UsuarioId, cancellationToken);
            return Responder(resultado, VolverAIndex(tableroId));
        }

        private IActionResult VolverAIndex(int tableroId) =>
            RedirectToAction(nameof(Index), new { tableroId });
    }
}
