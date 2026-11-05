using BoardyWeb.Comun;
using BoardyWeb.DTOs;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BoardyWeb.Controllers
{
    public class AsistenteController : ControladorBase
    {
        public const string PoliticaLimite = "asistente";

        private readonly IAsistenteService _asistenteService;

        public AsistenteController(IAsistenteService asistenteService)
        {
            _asistenteService = asistenteService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(PoliticaLimite)]
        public async Task<IActionResult> Preguntar([FromBody] PreguntaAsistenteDto pregunta, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(RespuestaAjax.Error("La pregunta no es válida."));
            }

            var resultado = await _asistenteService.PreguntarAsync(UsuarioId, pregunta, cancellationToken);

            return resultado.Exito
                ? Json(resultado.Datos)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, RespuestaAjax.Desde(resultado));
        }
    }
}
