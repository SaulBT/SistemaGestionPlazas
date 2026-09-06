namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;

public sealed record CrearSolicitudAperturaCommand(
    int IdExperienciaEducativa,
    string? Seccion,
    int IdModalidad,
    int CantidadSolicitantes,
    string? Justificacion,
    string NombreOriginalArchivo,
    long TamanioArchivo,
    byte[] ContenidoArchivo);
