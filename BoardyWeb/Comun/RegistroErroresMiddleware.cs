using System.Security.Claims;
using BoardyWeb.Services;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Guarda las excepciones no controladas en RegistroError y las relanza, para que la
    /// respuesta de error siga a cargo del manejador de excepciones.
    /// </summary>
    public class RegistroErroresMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegistroErroresMiddleware> _logger;

        public RegistroErroresMiddleware(RequestDelegate siguiente, IServiceScopeFactory scopeFactory, ILogger<RegistroErroresMiddleware> logger)
        {
            _siguiente = siguiente;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            try
            {
                await _siguiente(contexto);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await GuardarAsync(contexto, ex);
                throw;
            }
        }

        private async Task GuardarAsync(HttpContext contexto, Exception excepcion)
        {
            try
            {
                // Scope nuevo: el DbContext de la petición puede haber quedado en un estado inválido.
                using var scope = _scopeFactory.CreateScope();
                var registroErrores = scope.ServiceProvider.GetRequiredService<IRegistroErrorService>();

                await registroErrores.RegistrarAsync(
                    excepcion.Message,
                    excepcion.ToString(),
                    $"{contexto.Request.Method} {contexto.Request.Path}",
                    contexto.User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            catch (Exception errorAlGuardar)
            {
                _logger.LogError(errorAlGuardar, "No se pudo guardar el error en la base de datos.");
            }
        }
    }
}
