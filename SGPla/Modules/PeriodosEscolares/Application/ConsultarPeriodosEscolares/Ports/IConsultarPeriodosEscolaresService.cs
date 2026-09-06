using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Contracts;

namespace SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Ports;

public interface IConsultarPeriodosEscolaresService
{
    Task<PeriodoEscolarResultado<PeriodosEscolaresResponse>> ConsultarAsync(
        ConsultarPeriodosEscolaresQuery query,
        CancellationToken cancellationToken);
}
