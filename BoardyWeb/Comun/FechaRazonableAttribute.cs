using System.ComponentModel.DataAnnotations;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Rechaza fechas con año fuera de un rango razonable (por ejemplo, un año tipeado a medias como 0026).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FechaRazonableAttribute : ValidationAttribute
    {
        public const int AnioMinimo = 2000;
        public const int AnioMaximo = 2100;

        public FechaRazonableAttribute()
        {
            ErrorMessage = "La fecha de {0} debe estar entre los años 2000 y 2100.";
        }

        public override bool IsValid(object? value)
        {
            return value is not DateTime fecha || (fecha.Year >= AnioMinimo && fecha.Year <= AnioMaximo);
        }
    }
}
