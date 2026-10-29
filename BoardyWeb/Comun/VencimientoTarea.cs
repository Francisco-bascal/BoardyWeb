namespace BoardyWeb.Comun
{
    public enum EstadoVencimiento
    {
        /// <summary>Sin semáforo (tarea finalizada).</summary>
        Neutro,
        /// <summary>La fecha ya pasó.</summary>
        Vencida,
        /// <summary>Vence hoy.</summary>
        Hoy,
        /// <summary>Quedan entre 1 y 3 días.</summary>
        Proxima,
        /// <summary>Quedan más de 3 días.</summary>
        Holgada
    }

    public static class VencimientoTarea
    {
        public const string NombreEstadoFinalizado = "Finalizada";

        private const int DiasProximaAVencer = 3;

        public static (EstadoVencimiento Estado, string Descripcion) Calcular(DateTime fechaVencimiento, bool finalizada, DateTime? hoy = null)
        {
            if (finalizada)
            {
                return (EstadoVencimiento.Neutro, "Tarea finalizada");
            }

            var dias = (fechaVencimiento.Date - (hoy ?? DateTime.Today).Date).Days;

            return dias switch
            {
                < 0 => (EstadoVencimiento.Vencida, dias == -1 ? "Venció ayer" : $"Venció hace {-dias} días"),
                0 => (EstadoVencimiento.Hoy, "Vence hoy"),
                1 => (EstadoVencimiento.Proxima, "Vence mañana"),
                <= DiasProximaAVencer => (EstadoVencimiento.Proxima, $"Vence en {dias} días"),
                _ => (EstadoVencimiento.Holgada, $"Vence en {dias} días")
            };
        }
    }
}
