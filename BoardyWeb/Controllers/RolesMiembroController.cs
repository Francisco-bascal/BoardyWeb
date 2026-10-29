using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesMiembroController : ControladorBase
    {
        private readonly IRolMiembroService _rolMiembroService;

        public RolesMiembroController(IRolMiembroService rolMiembroService)
        {
            _rolMiembroService = rolMiembroService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await _rolMiembroService.ListarAsync(cancellationToken));
        }

        public IActionResult Crear()
        {
            return View(new CrearRolMiembroDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearRolMiembroDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var resultado = await _rolMiembroService.CrearAsync(dto, cancellationToken);

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return View(dto);
            }

            Notificar(resultado);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
        {
            var rol = await _rolMiembroService.ObtenerAsync(id, cancellationToken);
            var noValido = ResultadoNoValido(rol);
            if (noValido is not null)
            {
                return noValido;
            }

            ViewData["RolMiembroId"] = id;
            return View(new ActualizarRolMiembroDto { Nombre = rol.Datos!.Nombre, Descripcion = rol.Datos.Descripcion });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ActualizarRolMiembroDto dto, CancellationToken cancellationToken)
        {
            ViewData["RolMiembroId"] = id;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var resultado = await _rolMiembroService.ActualizarAsync(id, dto, cancellationToken);
            var noValido = ResultadoNoValido(resultado);
            if (noValido is not null)
            {
                return noValido;
            }

            if (!resultado.Exito)
            {
                AgregarErrores(resultado);
                return View(dto);
            }

            Notificar(resultado);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            var resultado = await _rolMiembroService.EliminarAsync(id, cancellationToken);
            return Responder(resultado, RedirectToAction(nameof(Index)));
        }
    }
}
