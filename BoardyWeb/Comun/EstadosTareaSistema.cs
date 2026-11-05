namespace BoardyWeb.Comun
{
    /// <summary>
    /// Estados de los que dependen Kanban, el semáforo de vencimiento y el panel de usuario.
    /// </summary>
    public static class EstadosTareaSistema
    {
        public const string Pendiente = "Pendiente";
        public const string EnProgreso = "En progreso";
        public const string Finalizada = "Finalizada";

        public static readonly string[] Todos = { Pendiente, EnProgreso, Finalizada };
    }
}
