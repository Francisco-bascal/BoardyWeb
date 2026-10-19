namespace BoardyWeb.Models
{
    public class Tablero
    {
        public int TableroId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public int PropietarioId { get; set; }

        public Usuario Propietario { get; set; } = null!;

        public ICollection<ListaTarea> ListasTarea { get; set; } = new List<ListaTarea>();

        public ICollection<TableroMiembro> Miembros { get; set; } = new List<TableroMiembro>();

        public ICollection<InvitacionTablero> Invitaciones { get; set; } = new List<InvitacionTablero>();
    }
}
