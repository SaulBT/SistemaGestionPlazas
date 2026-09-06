namespace SGPla.Modules.ProgramasEducativos.Application.Contracts;

public enum TipoResultadoProgramaEducativo
{
    Exito,
    Validacion,
    NoEncontrado,
    Conflicto
}

public sealed record ProgramaEducativoResultado<T>(
    TipoResultadoProgramaEducativo Tipo,
    T? Respuesta,
    string? Campo,
    string? Mensaje)
{
    public static ProgramaEducativoResultado<T> Exito(T respuesta)
    {
        return new(TipoResultadoProgramaEducativo.Exito, respuesta, null, null);
    }

    public static ProgramaEducativoResultado<T> Error(
        TipoResultadoProgramaEducativo tipo,
        string mensaje,
        string? campo = null)
    {
        return new(tipo, default, campo, mensaje);
    }
}
