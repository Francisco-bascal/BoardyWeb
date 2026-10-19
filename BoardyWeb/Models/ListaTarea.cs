namespace BoardyWeb.Models
{
    public class ListaTarea
    {
        public int ListaTareaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int TableroId { get; set; }

        public Tablero Tablero { get; set; } = null!;

        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
