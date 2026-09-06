namespace SGPla.Modules.Articulos.Application.Contracts;

public enum TipoResultadoArticulo
{
    Exito,
    Validacion,
    NoEncontrado,
    Conflicto
}

public sealed record ArticuloResultado<T>(
    TipoResultadoArticulo Tipo,
    T? Respuesta = default,
    string? Mensaje = null,
    string? Campo = null)
{
    public static ArticuloResultado<T> Exito(T respuesta) =>
        new(TipoResultadoArticulo.Exito, respuesta);

    public static ArticuloResultado<T> Error(
        TipoResultadoArticulo tipo,
        string mensaje,
        string? campo = null) =>
        new(tipo, Mensaje: mensaje, Campo: campo);
}
