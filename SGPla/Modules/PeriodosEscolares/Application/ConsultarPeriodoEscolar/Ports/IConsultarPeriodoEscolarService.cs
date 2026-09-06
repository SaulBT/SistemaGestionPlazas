using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Contracts;

namespace SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Ports;

public interface IConsultarPeriodoEscolarService
{
    Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> ConsultarAsync(
        ConsultarPeriodoEscolarQuery query,
        CancellationToken cancellationToken);
}
