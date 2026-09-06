using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Contracts;

namespace SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Ports;

public interface ICrearPeriodoEscolarService
{
    Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> CrearAsync(
        CrearPeriodoEscolarCommand command,
        CancellationToken cancellationToken);
}
