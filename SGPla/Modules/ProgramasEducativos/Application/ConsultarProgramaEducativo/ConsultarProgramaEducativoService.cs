using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;

namespace SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo;

public sealed class ConsultarProgramaEducativoService : IConsultarProgramaEducativoService
{
    private readonly IProgramaEducativoRepository _repository;

    public ConsultarProgramaEducativoService(IProgramaEducativoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> ConsultarAsync(
        ConsultarProgramaEducativoQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IdProgramaEducativo <= 0)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                "El ID del programa educativo no es válido.",
                "idProgramaEducativo");
        }

        var registro = await _repository.ObtenerPorIdAsync(
            query.IdProgramaEducativo,
            cancellationToken);

        if (registro is null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "El programa educativo indicado no existe.");
        }

        var planesEstudio = await _repository.ObtenerPlanesEstudioAsync(
            registro.IdProgramaEducativo,
            cancellationToken);

        return ProgramaEducativoResultado<ProgramaEducativoResponse>.Exito(
            Mapear(registro, planesEstudio));
    }

    private static ProgramaEducativoResponse Mapear(
        ProgramaEducativoRegistro registro,
        IReadOnlyList<PlanEstudioRegistro> planesEstudio)
    {
        return new ProgramaEducativoResponse(
            registro.IdProgramaEducativo,
            registro.Nombre,
            registro.Campus,
            registro.IdEntidadAcademica,
            registro.NombreEntidadAcademica,
            registro.IdAreaAcademica,
            registro.NombreAreaAcademica,
            registro.Region,
            planesEstudio.Select(plan => new PlanEstudioResponse(
                plan.IdPlanEstudios,
                plan.Nombre,
                plan.Modalidad,
                plan.IdArchivo,
                plan.NombreArchivo,
                plan.TipoArchivo,
                plan.TamanioArchivo,
                plan.CantidadExperienciasEducativas)).ToList());
    }
}
