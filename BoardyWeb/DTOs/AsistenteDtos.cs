using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.DTOs
{
    public class TurnoAsistenteDto
    {
        public bool EsUsuario { get; set; }

        public string Texto { get; set; } = string.Empty;
    }

    public class PreguntaAsistenteDto
    {
        [Required(ErrorMessage = "Escribí una pregunta.")]
        [StringLength(1000, ErrorMessage = "La pregunta no puede superar los {1} caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        public List<TurnoAsistenteDto> Historial { get; set; } = new();
    }

    public class RespuestaAsistenteDto
    {
        public string Texto { get; set; } = string.Empty;

        public bool OfrecerReportePdf { get; set; }
    }
}
