namespace BoardyWeb.Models
{
    public class Tarea
    {
        public int TareaId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public string Prioridad { get; set; } = string.Empty;

        public int ListaTareaId { get; set; }

        public ListaTarea ListaTarea { get; set; } = null!;

        public int EstadoTareaId { get; set; }

        public EstadoTarea EstadoTarea { get; set; } = null!;

        public ICollection<TareaMiembro> MiembrosAsignados { get; set; } = new List<TareaMiembro>();

        public ICollection<TareaEtiqueta> Etiquetas { get; set; } = new List<TareaEtiqueta>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
