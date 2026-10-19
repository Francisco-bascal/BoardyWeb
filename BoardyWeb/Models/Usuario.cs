namespace BoardyWeb.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public string Rol { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public ICollection<Tablero> TablerosPropios { get; set; } = new List<Tablero>();

        public ICollection<TableroMiembro> TablerosMiembro { get; set; } = new List<TableroMiembro>();

        public ICollection<InvitacionTablero> InvitacionesEnviadas { get; set; } = new List<InvitacionTablero>();

        public ICollection<InvitacionTablero> InvitacionesRecibidas { get; set; } = new List<InvitacionTablero>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
