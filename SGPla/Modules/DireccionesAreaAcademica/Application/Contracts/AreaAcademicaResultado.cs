namespace SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;

public enum TipoResultadoAreaAcademica
{
    Exito,
    Validacion,
    NoEncontrado
}

public sealed record AreaAcademicaResultado<T>(
    TipoResultadoAreaAcademica Tipo,
    T? Respuesta,
    string? Campo,
    string? Mensaje)
{
    public static AreaAcademicaResultado<T> Exito(T respuesta)
    {
        return new(TipoResultadoAreaAcademica.Exito, respuesta, null, null);
    }

    public static AreaAcademicaResultado<T> Error(
        TipoResultadoAreaAcademica tipo,
        string mensaje,
        string? campo = null)
    {
        return new(tipo, default, campo, mensaje);
    }
}
