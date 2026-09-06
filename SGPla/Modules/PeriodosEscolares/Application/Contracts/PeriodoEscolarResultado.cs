namespace SGPla.Modules.PeriodosEscolares.Application.Contracts;

public enum TipoResultadoPeriodoEscolar
{
    Exito,
    Validacion,
    NoEncontrado,
    Conflicto
}

public sealed record PeriodoEscolarResultado<T>(
    TipoResultadoPeriodoEscolar Tipo,
    T? Respuesta,
    string? Campo,
    string? Mensaje)
{
    public static PeriodoEscolarResultado<T> Exito(T respuesta)
    {
        return new(TipoResultadoPeriodoEscolar.Exito, respuesta, null, null);
    }

    public static PeriodoEscolarResultado<T> Error(
        TipoResultadoPeriodoEscolar tipo,
        string mensaje,
        string? campo = null)
    {
        return new(tipo, default, campo, mensaje);
    }
}
