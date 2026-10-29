using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BoardyWeb.Models
{
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        [StringLength(50, ErrorMessage = "El {0} no puede superar los {1} caracteres.")]
        public string Apellido { get; set; } = null!;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public ICollection<Tablero> TablerosPropios { get; set; } = new List<Tablero>();

        public ICollection<TableroMiembro> TablerosMiembro { get; set; } = new List<TableroMiembro>();

        public ICollection<InvitacionTablero> InvitacionesEnviadas { get; set; } = new List<InvitacionTablero>();

        public ICollection<InvitacionTablero> InvitacionesRecibidas { get; set; } = new List<InvitacionTablero>();

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
