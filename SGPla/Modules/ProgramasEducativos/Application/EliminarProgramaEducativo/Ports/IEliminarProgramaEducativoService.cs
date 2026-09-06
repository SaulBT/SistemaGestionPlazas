using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Contracts;

namespace SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Ports;

public interface IEliminarProgramaEducativoService
{
    Task<ProgramaEducativoResultado<bool>> EliminarAsync(
        EliminarProgramaEducativoCommand command,
        CancellationToken cancellationToken);
}
