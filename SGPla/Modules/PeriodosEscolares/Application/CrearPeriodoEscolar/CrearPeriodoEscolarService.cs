using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Models;
using SGPla.Modules.PeriodosEscolares.Application.Ports;
using SGPla.Modules.PeriodosEscolares.Domain;

namespace SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar;

public sealed class CrearPeriodoEscolarService : ICrearPeriodoEscolarService
{
    private readonly IPeriodoEscolarRepository _repository;

    public CrearPeriodoEscolarService(IPeriodoEscolarRepository repository)
    {
        _repository = repository;
    }

    public async Task<PeriodoEscolarResultado<PeriodoEscolarResponse>> CrearAsync(
        CrearPeriodoEscolarCommand command,
        CancellationToken cancellationToken)
    {
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

        var codigo = PeriodoEscolarReglas.ConstruirCodigo(
            command.Anio!.Value,
            command.Periodo!.Trim());

        if (await _repository.ExistePorCodigoAsync(
                codigo,
                null,
                cancellationToken))
        {
            return PeriodoEscolarResultado<PeriodoEscolarResponse>.Error(
                TipoResultadoPeriodoEscolar.Conflicto,
                "Ya existe un periodo escolar con los mismos datos.");
        }

        var registro = await _repository.CrearAsync(
            new PeriodoEscolarParaCrear(codigo),
            cancellationToken);

        return PeriodoEscolarResultado<PeriodoEscolarResponse>.Exito(
            CrearRespuesta(registro));
    }

    private static PeriodoEscolarResponse CrearRespuesta(
        PeriodoEscolarRegistro registro)
    {
        var detalle = PeriodoEscolarReglas.CrearDetalle(
            registro.IdPeriodoEscolar,
            registro.Codigo);

        return new PeriodoEscolarResponse(
            detalle.IdPeriodoEscolar,
            detalle.Codigo,
            detalle.Periodo,
            detalle.Anio,
            detalle.PeriodoMostrar);
    }
}
