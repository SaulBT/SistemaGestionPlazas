using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Contracts;

namespace SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Ports;

public interface IConsultarProgramasEducativosService
{
    Task<ProgramaEducativoResultado<ProgramasEducativosResponse>> ConsultarAsync(
        ConsultarProgramasEducativosQuery query,
        CancellationToken cancellationToken);
}
