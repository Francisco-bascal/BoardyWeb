namespace BoardyWeb.Models
{
    public class TareaEtiqueta
    {
        public int TareaEtiquetaId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public int EtiquetaId { get; set; }

        public Etiqueta Etiqueta { get; set; } = null!;

        public int TareaId { get; set; }

        public Tarea Tarea { get; set; } = null!;
    }
}
