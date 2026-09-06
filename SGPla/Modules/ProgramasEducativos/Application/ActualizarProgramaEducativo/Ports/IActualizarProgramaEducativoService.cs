using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.Contracts;

namespace SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Ports;

public interface IActualizarProgramaEducativoService
{
    Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> ActualizarAsync(
        ActualizarProgramaEducativoCommand command,
        CancellationToken cancellationToken);
}
