using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class ReordenarDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La posición debe ser mayor o igual a 1.")]
        [Display(Name = "Posición")]
        public int NuevoOrden { get; set; }
    }
}
