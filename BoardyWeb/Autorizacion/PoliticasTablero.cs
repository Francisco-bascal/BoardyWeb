namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Políticas de autorización por tablero. Se evalúan con IAuthorizationService
    /// usando el identificador del tablero (int) como recurso.
    /// Cada política incluye a los roles de mayor jerarquía.
    /// </summary>
    public static class PoliticasTablero
    {
        // Cualquier miembro del tablero (Lector o superior).
        public const string VerTablero = nameof(VerTablero);

        // Operario o superior: cambiar estado, etiquetar y comentar.
        public const string ColaborarEnTablero = nameof(ColaborarEnTablero);

        // Administrador o superior: crear y administrar listas, tareas y miembros.
        public const string AdministrarTablero = nameof(AdministrarTablero);

        // Solo el Propietario: editar o eliminar el tablero.
        public const string GestionarTablero = nameof(GestionarTablero);
    }
}
