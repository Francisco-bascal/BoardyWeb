namespace BoardyWeb.Models
{
    public class Etiqueta
    {
        public int EtiquetaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public ICollection<TareaEtiqueta> Tareas { get; set; } = new List<TareaEtiqueta>();
    }
}
