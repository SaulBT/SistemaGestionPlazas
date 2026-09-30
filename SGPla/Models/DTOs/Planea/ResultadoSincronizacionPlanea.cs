namespace SGPla.Models.DTOs.Planea
{
    public sealed record ResultadoSincronizacionPlanea(string CodigoPeriodo, string Estado, ResumenAplicacionPlanea? Resumen, string? Mensaje);
}
