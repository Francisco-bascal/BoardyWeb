using BoardyWeb.Models;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Estados base que tiene cada tablero. Son inmutables: de ellos dependen Kanban, el semáforo de
    /// vencimiento y el panel de usuario.
    /// </summary>
    public static class EstadosTareaSistema
    {
        public const string Pendiente = "Pendiente";
        public const string EnProgreso = "En progreso";
        public const string Finalizada = "Finalizada";

        public static readonly string[] Todos = { Pendiente, EnProgreso, Finalizada };

        // Crea los 3 estados base de un tablero nuevo, en el orden de Todos.
        public static List<EstadoTarea> CrearBase() => new()
        {
            new EstadoTarea { Nombre = Pendiente, Descripcion = "La tarea todavía no comenzó." },
            new EstadoTarea { Nombre = EnProgreso, Descripcion = "La tarea está siendo trabajada." },
            new EstadoTarea { Nombre = Finalizada, Descripcion = "La tarea fue completada." }
        };
    }
}
