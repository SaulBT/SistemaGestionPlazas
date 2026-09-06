using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.Contracts;

namespace SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Ports;

public interface IActualizarPeriodoEscolarService
{
    Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> ActualizarAsync(
        ActualizarPeriodoEscolarCommand command,
        CancellationToken cancellationToken);
}
