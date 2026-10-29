using Microsoft.AspNetCore.Authorization;

namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Exige que el usuario tenga, en el tablero evaluado, alguno de los roles indicados.
    /// </summary>
    public class RolTableroRequirement : IAuthorizationRequirement
    {
        public RolTableroRequirement(params string[] rolesPermitidos)
        {
            RolesPermitidos = rolesPermitidos;
        }

        public IReadOnlyCollection<string> RolesPermitidos { get; }
    }
}
