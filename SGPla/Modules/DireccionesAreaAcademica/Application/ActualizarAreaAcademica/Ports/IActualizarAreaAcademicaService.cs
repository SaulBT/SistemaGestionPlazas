using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Ports;

public interface IActualizarAreaAcademicaService
{
    Task<AreaAcademicaResultado<AreaAcademicaResponse>> ActualizarAsync(
        ActualizarAreaAcademicaCommand command,
        CancellationToken cancellationToken);
}
