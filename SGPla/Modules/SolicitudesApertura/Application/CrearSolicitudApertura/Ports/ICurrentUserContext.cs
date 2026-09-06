namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

public interface ICurrentUserContext
{
    bool EstaAutenticado { get; }

    bool EsCoordinadorEa { get; }

    string? Correo { get; }
}
