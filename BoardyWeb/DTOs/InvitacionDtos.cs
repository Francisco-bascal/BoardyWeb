using BoardyWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class InvitacionTableroDto
    {
        public int InvitacionTableroId { get; set; }

        public int TableroId { get; set; }

        public string TableroNombre { get; set; } = string.Empty;

        public string UsuarioInvitadorNombre { get; set; } = string.Empty;

        public string UsuarioInvitadoNombre { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public EstadoInvitacion Estado { get; set; }

        public DateTime FechaInvitacion { get; set; }
    }

    public class CrearInvitacionDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "El mensaje no puede superar los {1} caracteres.")]
        [Display(Name = "Mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
