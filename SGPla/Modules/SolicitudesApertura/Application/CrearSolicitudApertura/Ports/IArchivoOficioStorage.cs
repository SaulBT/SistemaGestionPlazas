using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;

namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

public interface IArchivoOficioStorage
{
    Task<ArchivoOficioGuardado> GuardarAsync(
        ArchivoOficioParaGuardar archivo,
        CancellationToken cancellationToken);

    Task EliminarAsync(string ruta, CancellationToken cancellationToken);
}
