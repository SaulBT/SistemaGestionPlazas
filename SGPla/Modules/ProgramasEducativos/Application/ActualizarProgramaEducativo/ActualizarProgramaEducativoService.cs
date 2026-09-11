using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo;

public sealed class ActualizarProgramaEducativoService : IActualizarProgramaEducativoService
{
    private readonly IProgramaEducativoRepository _repository;
    private readonly ProcesadorPlanesEstudio _procesadorPlanesEstudio;

    public ActualizarProgramaEducativoService(
        IProgramaEducativoRepository repository,
        ProcesadorPlanesEstudio procesadorPlanesEstudio)
    {
        _repository = repository;
        _procesadorPlanesEstudio = procesadorPlanesEstudio;
    }

    public async Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> ActualizarAsync(
        ActualizarProgramaEducativoCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdProgramaEducativo <= 0)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                "El ID del programa educativo no es válido.",
                "idProgramaEducativo");
        }

        var validacion = ProgramaEducativoReglas.ValidarDatos(
            command.Nombre,
            command.Campus,
            command.IdEntidadAcademica);

        if (validacion is not null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var preparacionPlanes = _procesadorPlanesEstudio.Preparar(
            command.PlanesEstudio,
            esCreacion: false);
        if (!preparacionPlanes.EsValida)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                preparacionPlanes.Mensaje!,
                preparacionPlanes.Campo);
        }

        var datos = ProgramaEducativoReglas.Normalizar(
            command.Nombre!,
            command.Campus!,
            command.IdEntidadAcademica!.Value);

        if (!await _repository.ExisteEntidadAcademicaActivaAsync(
                datos.IdEntidadAcademica,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "La entidad académica indicada no existe.",
                "idEntidadAcademica");
        }

        var clave = ProgramaEducativoReglas.ObtenerClave(datos.Nombre);
        if (await _repository.ExistePorClaveAsync(
                clave,
                command.IdProgramaEducativo,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Conflicto,
                "Ya existe un programa educativo con la misma clave.",
                "nombre");
        }

        var planesActuales = await _repository.ObtenerPlanesEstudioAsync(
            command.IdProgramaEducativo,
            cancellationToken);
        var idsPlanesActuales = planesActuales
            .Select(plan => plan.IdPlanEstudios)
            .ToHashSet();
        var idsPlanesRecibidos = preparacionPlanes.PlanesEstudio
            .Where(plan => plan.IdPlanEstudios.HasValue)
            .Select(plan => plan.IdPlanEstudios!.Value)
            .ToHashSet();

        if (idsPlanesRecibidos.Any(id => !idsPlanesActuales.Contains(id)))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "Uno de los planes de estudio no pertenece al programa educativo.",
                "planesEstudio");
        }

        var idsPlanesConCambiosEnExperiencias = preparacionPlanes.PlanesEstudio
            .Where(plan => plan.IdPlanEstudios.HasValue && plan.Archivo is not null)
            .Select(plan => plan.IdPlanEstudios!.Value)
            .Concat(idsPlanesActuales.Except(idsPlanesRecibidos))
            .ToHashSet();

        if (await _repository.PlanesEstudioTienenDependenciasAsync(
                idsPlanesConCambiosEnExperiencias,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Conflicto,
                "No se puede eliminar ni reemplazar un plan de estudios con ofertas o solicitudes de apertura asociadas.",
                "planesEstudio");
        }

        var planesEstudio = await _procesadorPlanesEstudio.GuardarArchivosAsync(
            preparacionPlanes.PlanesEstudio,
            cancellationToken);

        ProgramaActualizacionConPlanesResultado? actualizado;
        try
        {
            actualizado = await _repository.ActualizarConPlanesEstudioAsync(
                new ProgramaEducativoParaActualizar(
                    command.IdProgramaEducativo,
                    datos.Nombre,
                    datos.Campus,
                    datos.IdEntidadAcademica),
                planesEstudio,
                cancellationToken);
        }
        catch
        {
            await _procesadorPlanesEstudio.EliminarArchivosNuevosAsync(
                planesEstudio,
                cancellationToken);
            throw;
        }

        if (actualizado is null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "El programa educativo indicado no existe.");
        }

        await _procesadorPlanesEstudio.EliminarArchivosAnterioresAsync(
            actualizado.RutasArchivosAnteriores,
            cancellationToken);
        var planesRegistrados = await _repository.ObtenerPlanesEstudioAsync(
            actualizado.Programa.IdProgramaEducativo,
            cancellationToken);

        return ProgramaEducativoResultado<ProgramaEducativoResponse>.Exito(
            Mapear(actualizado.Programa, planesRegistrados));
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
