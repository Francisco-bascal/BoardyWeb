namespace BoardyWeb.Models
{
    public class Comentario
    {
        public int ComentarioId { get; set; }

        public string Contenido { get; set; } = string.Empty;

        public DateTime FechaComentario { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public int TareaId { get; set; }

        public Tarea Tarea { get; set; } = null!;
    }
}
