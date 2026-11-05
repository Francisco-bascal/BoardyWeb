namespace BoardyWeb.DTOs
{
    public class UsuarioAdministracionDto
    {
        public string UsuarioId { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; }

        public bool EsAdministrador { get; set; }
    }

    public class RegistroErrorDto
    {
        public string Mensaje { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public string? UsuarioId { get; set; }

        public DateTime Fecha { get; set; }
    }

    public class RegistroAuditoriaDto
    {
        public string UsuarioNombre { get; set; } = string.Empty;

        public string Accion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }
}
