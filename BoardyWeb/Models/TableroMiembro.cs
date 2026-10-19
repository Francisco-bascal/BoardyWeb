namespace BoardyWeb.Models
{
    public class TableroMiembro
    {
        public int TableroMiembroId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public int TableroId { get; set; }

        public Tablero Tablero { get; set; } = null!;

        public ICollection<MiembrosPorRol> Roles { get; set; } = new List<MiembrosPorRol>();

        public ICollection<TareaMiembro> TareasAsignadas { get; set; } = new List<TareaMiembro>();
    }
}
