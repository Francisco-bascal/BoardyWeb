using Microsoft.AspNetCore.Authorization;

namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Exige que el usuario tenga, en el tablero evaluado, alguno de los roles indicados.
    /// </summary>
    public class RolTableroRequirement : IAuthorizationRequirement
    {
        // Recibe los roles que cumplen el requisito.
        public RolTableroRequirement(params string[] rolesPermitidos)
        {
            RolesPermitidos = rolesPermitidos;
        }

        public IReadOnlyCollection<string> RolesPermitidos { get; }
    }
}
