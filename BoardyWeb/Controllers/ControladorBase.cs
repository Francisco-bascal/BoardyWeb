using System.Security.Claims;
using BoardyWeb.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardyWeb.Controllers
{
    /// <summary>
    /// Comportamiento común de los controladores autenticados: usuario actual y
    /// traducción de <see cref="ResultadoOperacion"/> a respuestas MVC.
    /// </summary>
    [Authorize]
    public abstract class ControladorBase : Controller
    {
        protected string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Devuelve 404 o 403 si el resultado lo indica; null en cualquier otro caso.</summary>
        protected IActionResult? ResultadoNoValido(ResultadoOperacion resultado)
        {
            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            return resultado.SinPermiso ? Forbid() : null;
        }

        /// <summary>Copia los errores del resultado al ModelState para volver a mostrar el formulario.</summary>
        protected void AgregarErrores(ResultadoOperacion resultado)
        {
            foreach (var (campo, mensaje) in resultado.Errores)
            {
                ModelState.AddModelError(campo, mensaje);
            }

            if (resultado.Mensaje is not null)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
            }
        }

        /// <summary>Deja el mensaje del resultado en TempData para mostrarlo tras la redirección.</summary>
        protected void Notificar(ResultadoOperacion resultado)
        {
            TempData[resultado.Exito ? "Exito" : "Error"] = resultado.Mensaje ?? string.Join(" ", resultado.Errores.Values);
        }

        /// <summary>Para acciones que solo redirigen: traduce 404/403, notifica y redirige a <paramref name="destino"/>.</summary>
        protected IActionResult Responder(ResultadoOperacion resultado, IActionResult destino)
        {
            var noValido = ResultadoNoValido(resultado);
            if (noValido is not null)
            {
                return noValido;
            }

            Notificar(resultado);
            return destino;
        }

        protected IActionResult DatosInvalidos(IActionResult destino)
        {
            TempData["Error"] = "Los datos enviados no son válidos.";
            return destino;
        }

        protected IActionResult IrATablero(int tableroId) =>
            RedirectToAction("Detalle", "Tableros", new { id = tableroId });

        protected IActionResult IrATarea(int tareaId) =>
            RedirectToAction("Detalle", "Tareas", new { id = tareaId });
    }
}
