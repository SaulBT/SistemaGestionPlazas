namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;

public enum TipoResultadoCrearSolicitudApertura
{
    Exito,
    Validacion,
    NoEncontrado,
    Prohibido,
    Conflicto,
    ReglaNegocio
}

public sealed record CrearSolicitudAperturaResultado(
    TipoResultadoCrearSolicitudApertura Tipo,
    CrearSolicitudAperturaResponse? Respuesta = null,
    string? Mensaje = null,
    string? Campo = null)
{
    public bool EsExitoso => Tipo == TipoResultadoCrearSolicitudApertura.Exito;

    public static CrearSolicitudAperturaResultado Exito(
        CrearSolicitudAperturaResponse respuesta)
    {
        return new CrearSolicitudAperturaResultado(
            TipoResultadoCrearSolicitudApertura.Exito,
            respuesta);
    }

    public static CrearSolicitudAperturaResultado Error(
        TipoResultadoCrearSolicitudApertura tipo,
        string mensaje,
        string? campo = null)
    {
        return new CrearSolicitudAperturaResultado(tipo, Mensaje: mensaje, Campo: campo);
    }
}
