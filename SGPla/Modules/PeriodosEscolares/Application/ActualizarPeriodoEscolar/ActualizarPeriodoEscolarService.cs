using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.Models;
using SGPla.Modules.PeriodosEscolares.Application.Ports;
using SGPla.Modules.PeriodosEscolares.Domain;

namespace SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar;

public sealed class ActualizarPeriodoEscolarService : IActualizarPeriodoEscolarService
{
    private readonly IPeriodoEscolarRepository _repository;

    public ActualizarPeriodoEscolarService(IPeriodoEscolarRepository repository)
    {
        _repository = repository;
    }

    public async Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> ActualizarAsync(
        ActualizarPeriodoEscolarCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdPeriodoEscolar <= 0)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.Validacion,
                "El ID del periodo escolar no es válido.",
                "idPeriodoEscolar");
        }

        var validacion = PeriodoEscolarReglas.ValidarDatos(
            command.Anio,
            command.Periodo);

        if (validacion is not null)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var existente = await _repository.ObtenerPorIdAsync(
            command.IdPeriodoEscolar,
            cancellationToken);

        if (existente is null)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.NoEncontrado,
                "El periodo escolar indicado no existe.");
        }

        var codigo = PeriodoEscolarReglas.ConstruirCodigo(
            command.Anio!.Value,
            command.Periodo!.Trim());

        if (await _repository.ExistePorCodigoAsync(
                codigo,
                command.IdPeriodoEscolar,
                cancellationToken))
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.Conflicto,
                "Ya existe un periodo escolar con los mismos datos.");
        }

        var actualizado = await _repository.ActualizarAsync(
            new PeriodoEscolarParaActualizar(
                command.IdPeriodoEscolar,
                codigo),
            cancellationToken);

        if (actualizado is null)
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.NoEncontrado,
                "El periodo escolar indicado no existe.");
        }

        var detalle = PeriodoEscolarReglas.CrearDetalle(
            actualizado.IdPeriodoEscolar,
            actualizado.Codigo);

        return PeriodoEscolarResultado<PeriodoEscolarResponse>.Exito(
            new PeriodoEscolarResponse(
                detalle.IdPeriodoEscolar,
                detalle.Codigo,
                detalle.Periodo,
                detalle.Anio,
                detalle.PeriodoMostrar));
    }
}
