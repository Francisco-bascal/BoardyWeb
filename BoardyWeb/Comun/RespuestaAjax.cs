namespace BoardyWeb.Comun
{
    /// <summary>
    /// Respuesta JSON uniforme para las acciones invocadas por AJAX.
    /// </summary>
    public class RespuestaAjax
    {
        public bool Exito { get; init; }

        public string? Mensaje { get; init; }

        public static RespuestaAjax Desde(ResultadoOperacion resultado) => new()
        {
            Exito = resultado.Exito,
            Mensaje = resultado.Mensaje ?? (resultado.Errores.Count > 0 ? string.Join(" ", resultado.Errores.Values) : null)
        };

        public static RespuestaAjax Error(string mensaje) => new() { Mensaje = mensaje };
    }
}
