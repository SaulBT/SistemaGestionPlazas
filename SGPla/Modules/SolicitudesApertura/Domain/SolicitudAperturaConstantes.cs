namespace SGPla.Modules.SolicitudesApertura.Domain;

public static class SolicitudAperturaConstantes
{
    public const int TAMANIO_MAXIMO_ARCHIVO = 5 * 1024 * 1024;

    public const int TAMANIO_MAXIMO_SOLICITUD_HTTP = 6 * 1024 * 1024;

    public const string EXTENSION_ARCHIVO_OFICIO = ".pdf";

    public const string TIPO_ARCHIVO_OFICIO = "application/pdf";

    public const string ESTADO_PENDIENTE = "Pendiente";

    public const string ESTADO_ACEPTADA = "Aceptada";

    public const string ESTADO_RECHAZADA = "Rechazada";

    public const string CARPETA_ARCHIVOS_OFICIO = "solicitudes-apertura";
}
