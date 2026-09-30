using System.Globalization;

namespace SGPla.Commons
{
    /// <summary>
    /// Fechas de un periodo derivadas de su código, para periodos registrados sin fechas.
    /// Regla institucional: AAAA01 = agosto (AAAA-1) a enero (AAAA); AAAA51 = febrero a julio (AAAA).
    /// Ej. 202701 = 2026-08-01..2027-01-31 (PLANEA programa 202701 del 17/08/2026 al 02/12/2026).
    /// </summary>
    public static class PlaneaPeriodos
    {
        public static (DateOnly Inicio, DateOnly Fin)? FechasPorCodigo(string? codigo)
        {
            var valor = codigo?.Trim();
            if (valor is null || valor.Length != 6
                || !int.TryParse(valor.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var anio))
                return null;

            return valor[4..] switch
            {
                "01" => (new DateOnly(anio - 1, 8, 1), new DateOnly(anio, 1, 31)),
                "51" => (new DateOnly(anio, 2, 1), new DateOnly(anio, 7, 31)),
                _ => null
            };
        }
    }
}
