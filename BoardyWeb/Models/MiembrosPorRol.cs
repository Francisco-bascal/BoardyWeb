namespace BoardyWeb.Models
{
    public class MiembrosPorRol
    {
        public int MiembroPorRolId { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public int TableroMiembroId { get; set; }

        public TableroMiembro TableroMiembro { get; set; } = null!;

        public int RolMiembroId { get; set; }

        public RolMiembro RolMiembro { get; set; } = null!;
    }
}
