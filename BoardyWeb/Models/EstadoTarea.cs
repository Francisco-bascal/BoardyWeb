namespace BoardyWeb.Models
{
    public class EstadoTarea
    {
        public int EstadoTareaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
