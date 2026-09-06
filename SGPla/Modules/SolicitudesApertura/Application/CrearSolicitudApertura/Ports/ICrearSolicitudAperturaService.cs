using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;

namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

public interface ICrearSolicitudAperturaService
{
    Task<CrearSolicitudAperturaResultado> CrearAsync(
        CrearSolicitudAperturaCommand command,
        CancellationToken cancellationToken);
}
