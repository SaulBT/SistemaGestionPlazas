using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;

namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

public interface ISolicitudAperturaRepository
{
    Task<CoordinadorEaContexto?> ObtenerCoordinadorEaAsync(
        string correo,
        CancellationToken cancellationToken);

    Task<ExperienciaEducativaContexto?> ObtenerExperienciaEducativaAsync(
        int idExperienciaEducativa,
        CancellationToken cancellationToken);

    Task<ModalidadContexto?> ObtenerModalidadAsync(
        int idModalidad,
        CancellationToken cancellationToken);

    Task<PeriodoContexto?> ObtenerPeriodoSiguienteAsync(
        DateOnly fechaActual,
        CancellationToken cancellationToken);

    Task<bool> ExisteSolicitudActivaAsync(
        SolicitudAperturaDuplicidadContexto contexto,
        CancellationToken cancellationToken);

    Task CrearAsync(
        SolicitudAperturaParaCrear solicitud,
        ArchivoOficioGuardado archivo,
        CancellationToken cancellationToken);
}
