using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Contracts;

namespace SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Ports;

public interface IConsultarProgramaEducativoService
{
    Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> ConsultarAsync(
        ConsultarProgramaEducativoQuery query,
        CancellationToken cancellationToken);
}
