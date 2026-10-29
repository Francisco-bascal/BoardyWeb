using Microsoft.AspNetCore.Authorization;

namespace BoardyWeb.Autorizacion
{
    /// <summary>
    /// Exige poder trabajar sobre una tarea: ser Propietario/Administrador de su tablero o ser Operario
    /// asignado a la tarea.
    /// </summary>
    public class TrabajoEnTareaRequirement : IAuthorizationRequirement
    {
    }
}
