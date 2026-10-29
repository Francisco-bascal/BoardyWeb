using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class TableroMiembroDto
    {
        public int TableroMiembroId { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool EsPropietario { get; set; }

        public IReadOnlyList<RolMiembroDto> Roles { get; set; } = new List<RolMiembroDto>();
    }

    public class AgregarMiembroDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar un rol.")]
        [Display(Name = "Rol")]
        public int RolMiembroId { get; set; }
    }

    public class AsignarRolDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar un rol.")]
        public int RolMiembroId { get; set; }
    }
}
