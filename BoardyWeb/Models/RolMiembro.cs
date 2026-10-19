namespace BoardyWeb.Models
{
    public class RolMiembro
    {
        public int RolMiembroId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public ICollection<MiembrosPorRol> Miembros { get; set; } = new List<MiembrosPorRol>();
    }
}
