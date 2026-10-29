namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Políticas de autorización sobre una tarea concreta. Se evalúan con IAuthorizationService
    /// usando el identificador de la tarea (int) como recurso.
    /// </summary>
    public static class PoliticasTarea
    {
        /// <summary>
        /// Mover la tarea, cambiar su estado y gestionar sus etiquetas: Propietario y Administrador en
        /// cualquier tarea; Operario solo en las tareas a las que está asignado.
        /// </summary>
        public const string TrabajarEnTarea = nameof(TrabajarEnTarea);
    }
}
