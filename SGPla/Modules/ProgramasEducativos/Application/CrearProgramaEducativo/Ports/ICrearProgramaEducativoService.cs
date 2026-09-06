using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Contracts;

namespace SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Ports;

public interface ICrearProgramaEducativoService
{
    Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> CrearAsync(
        CrearProgramaEducativoCommand command,
        CancellationToken cancellationToken);
}
