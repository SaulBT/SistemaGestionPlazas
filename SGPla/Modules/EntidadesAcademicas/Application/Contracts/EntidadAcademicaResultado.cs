namespace SGPla.Modules.EntidadesAcademicas.Application.Contracts;

public enum TipoResultadoEntidadAcademica
{
    Exito,
    Validacion,
    NoEncontrado,
    Conflicto
}

public sealed record EntidadAcademicaResultado<T>(
    TipoResultadoEntidadAcademica Tipo,
    T? Respuesta,
    string? Campo,
    string? Mensaje)
{
    public static EntidadAcademicaResultado<T> Exito(T respuesta)
    {
        return new(TipoResultadoEntidadAcademica.Exito, respuesta, null, null);
    }

    public static EntidadAcademicaResultado<T> Error(
        TipoResultadoEntidadAcademica tipo,
        string mensaje,
        string? campo = null)
    {
        return new(tipo, default, campo, mensaje);
    }
}
