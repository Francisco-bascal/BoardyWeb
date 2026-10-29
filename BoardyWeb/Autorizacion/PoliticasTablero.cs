namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Políticas de autorización por tablero. Se evalúan con IAuthorizationService
    /// usando el identificador del tablero (int) como recurso.
    /// Cada política incluye a los roles de mayor jerarquía.
    /// </summary>
    public static class PoliticasTablero
    {
        /// <summary>Cualquier miembro del tablero (Lector o superior).</summary>
        public const string VerTablero = nameof(VerTablero);

        /// <summary>Operario o superior: cambiar estado, etiquetar y comentar.</summary>
        public const string ColaborarEnTablero = nameof(ColaborarEnTablero);

        /// <summary>Administrador o superior: crear y administrar listas, tareas y miembros.</summary>
        public const string AdministrarTablero = nameof(AdministrarTablero);

        /// <summary>Solo el Propietario: editar o eliminar el tablero.</summary>
        public const string GestionarTablero = nameof(GestionarTablero);
    }
}
