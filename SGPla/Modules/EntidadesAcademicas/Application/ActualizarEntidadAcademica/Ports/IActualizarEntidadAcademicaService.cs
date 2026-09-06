using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;

namespace SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Ports;

public interface IActualizarEntidadAcademicaService
{
    Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> ActualizarAsync(
        ActualizarEntidadAcademicaCommand command,
        CancellationToken cancellationToken);
}
