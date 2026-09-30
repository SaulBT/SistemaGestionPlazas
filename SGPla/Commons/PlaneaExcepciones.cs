namespace SGPla.Commons
{
    public sealed class PlaneaRespuestaInvalidaException(string mensaje) : Exception(mensaje);
    public sealed class SincronizacionEnCursoException(string codigoPeriodo)
        : Exception($"Ya hay una sincronización en curso para el periodo {codigoPeriodo}.");
}
