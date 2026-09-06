using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Ports;

namespace SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar;

public sealed class EliminarPeriodoEscolarService : IEliminarPeriodoEscolarService
{
    private readonly IPeriodoEscolarRepository _repository;

    public EliminarPeriodoEscolarService(IPeriodoEscolarRepository repository)
    {
        _repository = repository;
    }

    public async Task<PeriodoEscolarResultado<bool>> EliminarAsync(
        EliminarPeriodoEscolarCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdPeriodoEscolar <= 0)
        {
            return PeriodoEscolarResultado<bool>.Error(
                TipoResultadoPeriodoEscolar.Validacion,
                "El ID del periodo escolar no es válido.",
                "idPeriodoEscolar");
        }

        var existe = await _repository.ObtenerPorIdAsync(
            command.IdPeriodoEscolar,
            cancellationToken);

        if (existe is null)
        {
            return PeriodoEscolarResultado<bool>.Error(
                TipoResultadoPeriodoEscolar.NoEncontrado,
                "El periodo escolar indicado no existe.");
        }

        if (await _repository.TieneRelacionesAsync(
                command.IdPeriodoEscolar,
                cancellationToken))
        {
            return PeriodoEscolarResultado<bool>.Error(
                TipoResultadoPeriodoEscolar.Conflicto,
                "El periodo escolar no puede eliminarse porque tiene relaciones con otros registros.");
        }

        var eliminado = await _repository.EliminarAsync(
            command.IdPeriodoEscolar,
            cancellationToken);

        if (!eliminado)
        {
            return PeriodoEscolarResultado<bool>.Error(
                TipoResultadoPeriodoEscolar.NoEncontrado,
                "El periodo escolar indicado no existe.");
        }

        return PeriodoEscolarResultado<bool>.Exito(true);
    }
}
