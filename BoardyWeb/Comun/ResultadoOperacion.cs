namespace BoardyWeb.Comun
{
    /// <summary>
    /// Resultado de una operación de negocio. Permite que los servicios comuniquen
    /// el resultado sin depender de ASP.NET Core ni de códigos HTTP.
    /// </summary>
    public class ResultadoOperacion
    {
        public bool Exito { get; protected init; }

        public bool NoEncontrado { get; protected init; }

        public bool SinPermiso { get; protected init; }

        public string? Mensaje { get; protected init; }

        public IReadOnlyDictionary<string, string> Errores { get; protected init; } =
            new Dictionary<string, string>();

        public static ResultadoOperacion Exitoso(string? mensaje = null) =>
            new() { Exito = true, Mensaje = mensaje };

        public static ResultadoOperacion Fallo(string mensaje) =>
            new() { Mensaje = mensaje };

        public static ResultadoOperacion Fallo(string campo, string mensaje) =>
            new() { Errores = new Dictionary<string, string> { [campo] = mensaje } };

        public static ResultadoOperacion NoExiste(string mensaje) =>
            new() { NoEncontrado = true, Mensaje = mensaje };

        public static ResultadoOperacion Prohibido(string mensaje) =>
            new() { SinPermiso = true, Mensaje = mensaje };
    }

    public class ResultadoOperacion<T> : ResultadoOperacion
    {
        public T? Datos { get; private init; }

        public static ResultadoOperacion<T> Exitoso(T datos, string? mensaje = null) =>
            new() { Exito = true, Datos = datos, Mensaje = mensaje };

        public static new ResultadoOperacion<T> Fallo(string mensaje) =>
            new() { Mensaje = mensaje };

        public static new ResultadoOperacion<T> Fallo(string campo, string mensaje) =>
            new() { Errores = new Dictionary<string, string> { [campo] = mensaje } };

        public static new ResultadoOperacion<T> NoExiste(string mensaje) =>
            new() { NoEncontrado = true, Mensaje = mensaje };

        public static new ResultadoOperacion<T> Prohibido(string mensaje) =>
            new() { SinPermiso = true, Mensaje = mensaje };
    }
}
