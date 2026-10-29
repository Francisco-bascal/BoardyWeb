using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardyWeb.Models
{
    public class Tablero : AuditableEntity
    {
        public int TableroId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(150, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(1000, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public string PropietarioId { get; set; } = null!;

        public Usuario Propietario { get; set; } = null!;

        public ICollection<ListaTarea> ListasTarea { get; set; } = new List<ListaTarea>();

        public ICollection<TableroMiembro> Miembros { get; set; } = new List<TableroMiembro>();

        public ICollection<InvitacionTablero> Invitaciones { get; set; } = new List<InvitacionTablero>();
    }
}
