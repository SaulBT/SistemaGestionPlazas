using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class CatalogosMvcService : ICatalogosMvcService
{
    private readonly SgplaDbContext _db;

    public CatalogosMvcService(SgplaDbContext db) => _db = db;

    public async Task<CatalogosViewModel> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var regiones = await _db.Regiones.AsNoTracking()
            .Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Clave).ThenBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Clave.ToString(), x.Nombre))
            .ToListAsync(cancellationToken);

        var campus = await _db.Campuses.AsNoTracking()
            .Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Clave, x.Nombre))
            .ToListAsync(cancellationToken);

        var areas = await _db.AreaAcademicas.AsNoTracking()
            .Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Clave).ThenBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Clave.ToString(), x.Nombre))
            .ToListAsync(cancellationToken);

        return new CatalogosViewModel
        {
            Regiones = regiones,
            Campus = campus,
            AreasAcademicas = areas,
            SistemasEducativos = await _db.SistemasEducativos.AsNoTracking()
                .Where(x => x.FechaEliminacion == null).OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            NivelesFormacion = await _db.NivelesFormacion.AsNoTracking()
                .Where(x => x.FechaEliminacion == null).OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Clave, x.Nombre)).ToListAsync(cancellationToken),
            AreasFormacion = await _db.AreaFormaciones.AsNoTracking()
                .Where(x => x.FechaEliminacion == null).OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Clave, x.Nombre)).ToListAsync(cancellationToken),
            GradosAcademicos = await _db.GradoAcademicos.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            TratamientosAcademicos = await _db.TratamientosAcademicos.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            ModalidadesRecepcion = await _db.ModalidadesRecepcion.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            TiposPlaza = await _db.TipoPlazas.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            TiposContratacion = await _db.TiposContratacion.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            TiposDocumentoAspirante = await _db.TipoDocumentoAspirantes.AsNoTracking().OrderBy(x => x.Nombre)
                .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre)).ToListAsync(cancellationToken),
            Articulos = await _db.Articulos.AsNoTracking().OrderBy(x => x.Numero)
                .Select(x => new CatalogoOpcion(x.Id, x.Numero, x.Descripcion ?? string.Empty)).ToListAsync(cancellationToken)
        };
    }

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerCampusAsync(int? regionId, CancellationToken cancellationToken = default) =>
        await _db.Campuses.AsNoTracking()
            .Where(x => x.FechaEliminacion == null && (!regionId.HasValue || x.RegionId == regionId.Value))
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Clave, x.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerEntidadesAsync(int? campusId, int? areaAcademicaId, CancellationToken cancellationToken = default) =>
        await _db.EntidadAcademicas.AsNoTracking()
            .Where(x => x.FechaEliminacion == null && (!campusId.HasValue || x.CampusId == campusId.Value) && (!areaAcademicaId.HasValue || x.AreaAcademicaId == areaAcademicaId.Value))
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Clave, x.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerProgramasAsync(int? entidadAcademicaId, CancellationToken cancellationToken = default) =>
        await _db.ProgramasEducativos.AsNoTracking()
            .Where(x => x.FechaEliminacion == null && (!entidadAcademicaId.HasValue || x.EntidadAcademicaId == entidadAcademicaId.Value))
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.Id.ToString(), x.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerPlanesAsync(int? programaEducativoId, CancellationToken cancellationToken = default) =>
        await _db.PlanesEstudios.AsNoTracking()
            .Where(x => x.FechaEliminacion == null && (!programaEducativoId.HasValue || x.ProgramaEducativoId == programaEducativoId.Value))
            .OrderBy(x => x.Codigo)
            .Select(x => new CatalogoOpcion(x.Id, x.Codigo, x.Codigo))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogoOpcion>> ObtenerExperienciasAsync(int? planEstudiosId, CancellationToken cancellationToken = default) =>
        await _db.ExperienciasEducativas.AsNoTracking()
            .Where(x => x.FechaEliminacion == null && (!planEstudiosId.HasValue || x.PlanEstudiosId == planEstudiosId.Value))
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoOpcion(x.Id, x.MateriaEe + "-" + x.CursoEe, x.Nombre))
            .ToListAsync(cancellationToken);
}
