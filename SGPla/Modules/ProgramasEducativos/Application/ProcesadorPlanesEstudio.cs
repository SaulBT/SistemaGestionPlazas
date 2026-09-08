using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Application;

public sealed class ProcesadorPlanesEstudio
{
    private readonly IPlanEstudiosArchivoStorage _archivoStorage;
    private readonly IPlanEstudiosImportador _importador;
    private readonly ILogger<ProcesadorPlanesEstudio> _logger;

    public ProcesadorPlanesEstudio(
        IPlanEstudiosArchivoStorage archivoStorage,
        IPlanEstudiosImportador importador,
        ILogger<ProcesadorPlanesEstudio> logger)
    {
        _archivoStorage = archivoStorage;
        _importador = importador;
        _logger = logger;
    }

    public PreparacionPlanesEstudio Preparar(
        IReadOnlyList<PlanEstudioParaGuardar> planesEstudio,
        bool esCreacion)
    {
        var validacion = PlanEstudiosReglas.ValidarPlanes(planesEstudio, esCreacion);
        if (validacion is not null)
        {
            return Error(validacion.Campo, validacion.Mensaje);
        }

        var planesPreparados = new List<PlanEstudioPreparado>();

        for (var indice = 0; indice < planesEstudio.Count; indice++)
        {
            var plan = planesEstudio[indice];
            var experiencias = Array.Empty<ExperienciaEducativaParaCrear>();

            if (plan.Archivo is not null)
            {
                var importacion = _importador.Importar(plan.Archivo);
                if (!importacion.EsValido)
                {
                    return Error($"planesEstudio[{indice}].archivo", importacion.Error!);
                }

                var validacionExperiencias = PlanEstudiosReglas.ValidarExperiencias(
                    importacion.ExperienciasEducativas,
                    $"planesEstudio[{indice}]");
                if (validacionExperiencias is not null)
                {
                    return Error(validacionExperiencias.Campo, validacionExperiencias.Mensaje);
                }

                experiencias = importacion.ExperienciasEducativas.ToArray();
            }

            planesPreparados.Add(new PlanEstudioPreparado(
                plan.IdPlanEstudios,
                plan.Nombre!.Trim(),
                string.IsNullOrWhiteSpace(plan.Modalidad)
                    ? null
                    : plan.Modalidad.Trim(),
                plan.Archivo,
                experiencias));
        }

        return new PreparacionPlanesEstudio(planesPreparados, null, null);
    }

    public async Task<IReadOnlyList<PlanEstudioParaPersistir>> GuardarArchivosAsync(
        IReadOnlyList<PlanEstudioPreparado> planesEstudio,
        CancellationToken cancellationToken)
    {
        var planesPersistir = new List<PlanEstudioParaPersistir>();

        try
        {
            foreach (var plan in planesEstudio)
            {
                ArchivoPlanGuardado? archivoGuardado = null;
                if (plan.Archivo is not null)
                {
                    archivoGuardado = await _archivoStorage.GuardarAsync(
                        new ArchivoPlanParaGuardar(
                            plan.Archivo.Contenido,
                            plan.Archivo.NombreOriginal,
                            plan.Archivo.Tipo),
                        cancellationToken);
                }

                planesPersistir.Add(new PlanEstudioParaPersistir(
                    plan.IdPlanEstudios,
                    plan.Nombre,
                    plan.Modalidad,
                    archivoGuardado,
                    plan.ExperienciasEducativas));
            }

            return planesPersistir;
        }
        catch
        {
            await EliminarArchivosNuevosAsync(planesPersistir, cancellationToken);
            throw;
        }
    }

    public async Task EliminarArchivosNuevosAsync(
        IReadOnlyList<PlanEstudioParaPersistir> planesEstudio,
        CancellationToken cancellationToken)
    {
        foreach (var ruta in planesEstudio
                     .Select(plan => plan.ArchivoNuevo?.Ruta)
                     .Where(ruta => !string.IsNullOrWhiteSpace(ruta)))
        {
            try
            {
                await _archivoStorage.EliminarAsync(ruta!, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible eliminar el archivo nuevo del plan de estudios: {Ruta}",
                    ruta);
            }
        }
    }

    public async Task EliminarArchivosAnterioresAsync(
        IReadOnlyList<string> rutas,
        CancellationToken cancellationToken)
    {
        foreach (var ruta in rutas.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                await _archivoStorage.EliminarAsync(ruta, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible eliminar el archivo anterior del plan de estudios: {Ruta}",
                    ruta);
            }
        }
    }

    private static PreparacionPlanesEstudio Error(string campo, string mensaje) =>
        new([], campo, mensaje);
}
