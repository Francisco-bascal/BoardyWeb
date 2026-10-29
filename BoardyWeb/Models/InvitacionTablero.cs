namespace BoardyWeb.Models
{
    public class InvitacionTablero
    {
        public int InvitacionTableroId { get; set; }

        public DateTime FechaInvitacion { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string UsuarioInvitadoId { get; set; } = null!;

        public Usuario UsuarioInvitado { get; set; } = null!;

        public string UsuarioInvitadorId { get; set; } = null!;

        public Usuario UsuarioInvitador { get; set; } = null!;

        public int TableroId { get; set; }

        public Tablero Tablero { get; set; } = null!;
    }
}
