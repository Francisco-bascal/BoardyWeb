namespace BoardyWeb.Models
{
    public class RegistroError
    {
        public int RegistroErrorId { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public string? UsuarioId { get; set; }

        public DateTime Fecha { get; set; }
    }
}
