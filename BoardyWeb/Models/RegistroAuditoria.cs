namespace BoardyWeb.Models
{
    public class RegistroAuditoria
    {
        public int RegistroAuditoriaId { get; set; }

        public string Accion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public int TableroId { get; set; }

        public Tablero Tablero { get; set; } = null!;

        public string UsuarioId { get; set; } = null!;

        public Usuario Usuario { get; set; } = null!;
    }
}
