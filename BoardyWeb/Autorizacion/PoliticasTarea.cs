namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Políticas de autorización sobre una tarea concreta. Se evalúan con IAuthorizationService
    /// usando el identificador de la tarea (int) como recurso.
    /// </summary>
    public static class PoliticasTarea
    {
        // Mover, cambiar estado y etiquetar: Propietario y Administrador siempre; Operario si está asignado.
        public const string TrabajarEnTarea = nameof(TrabajarEnTarea);
    }
}
