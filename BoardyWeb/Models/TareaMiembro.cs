namespace BoardyWeb.Models
{
    public class TareaMiembro
    {
        public int TareaMiembroId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public int TableroMiembroId { get; set; }

        public TableroMiembro TableroMiembro { get; set; } = null!;

        public int TareaId { get; set; }

        public Tarea Tarea { get; set; } = null!;
    }
}
