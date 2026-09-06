namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;

public sealed record ArchivoOficioParaGuardar(
    byte[] Contenido,
    string NombreOriginal,
    string Tipo);

public sealed record ArchivoOficioGuardado(
    string NombreOriginal,
    string Ruta,
    string Tipo,
    long Tamanio);
