using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Contracts;

namespace SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Ports;

public interface IEliminarPeriodoEscolarService
{
    Task<PeriodoEscolarResultado<bool>> EliminarAsync(
        EliminarPeriodoEscolarCommand command,
        CancellationToken cancellationToken);
}
