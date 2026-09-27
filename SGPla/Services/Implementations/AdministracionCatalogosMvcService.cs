using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class AdministracionCatalogosMvcService : IAdministracionCatalogosMvcService
{
    private static readonly Regex ClaveClasificacion = new("^[A-Z0-9]{1,50}$", RegexOptions.CultureInvariant);
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;

    public AdministracionCatalogosMvcService(SgplaDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<CatalogoAdministracionMvcFila>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var filas = new List<CatalogoAdministracionMvcFila>();
        filas.AddRange(await _db.SistemasEducativos.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.SistemaEducativo, "Sistema educativo",
                x.Id, null, x.Nombre, _db.ProgramasEducativos.Count(p => p.SistemaEducativoId == x.Id),
                x.FechaEliminacion == null, null, true)).ToListAsync(cancellationToken));
        filas.AddRange(await _db.NivelesFormacion.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.NivelFormacion, "Nivel de formación",
                x.Id, x.Clave, x.Nombre, _db.ProgramasEducativos.Count(p => p.NivelFormacionId == x.Id),
                x.FechaEliminacion == null, null, true)).ToListAsync(cancellationToken));
        filas.AddRange(await _db.AreaFormaciones.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.AreaFormacion, "Área de formación",
                x.Id, x.Clave, x.Nombre, _db.ExperienciasEducativas.Count(e => e.AreaFormacionId == x.Id),
                x.FechaEliminacion == null, null, true)).ToListAsync(cancellationToken));
        filas.AddRange(await _db.GradoAcademicos.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.GradoAcademico, "Grado académico",
                x.Id, null, x.Nombre,
                _db.TratamientosAcademicos.Count(t => t.GradoAcademicoId == x.Id)
                    + _db.FormacionAspirantes.Count(f => f.GradoAcademicoId == x.Id), true, null, false))
            .ToListAsync(cancellationToken));
        filas.AddRange(await _db.TratamientosAcademicos.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.TratamientoAcademico, "Tratamiento académico",
                x.Id, null, x.Nombre, _db.IntegranteConsejoTecnicos.Count(i => i.TratamientoAcademicoId == x.Id),
                true, _db.GradoAcademicos.Where(g => g.Id == x.GradoAcademicoId).Select(g => g.Nombre).FirstOrDefault(), false))
            .ToListAsync(cancellationToken));
        filas.AddRange(await _db.ModalidadesRecepcion.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.ModalidadRecepcion, "Modalidad de recepción",
                x.Id, null, x.Nombre, _db.Avisos.Count(a => a.ModalidadRecepcionId == x.Id), true,
                x.RequiereLugar ? "Requiere lugar" : "No requiere lugar", false)).ToListAsync(cancellationToken));
        filas.AddRange(await _db.TipoPlazas.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.TipoPlaza, "Tipo de plaza",
                x.Id, null, x.Nombre, _db.Ofertas.Count(o => o.TipoPlazaId == x.Id), true, null, false))
            .ToListAsync(cancellationToken));
        filas.AddRange(await _db.TiposContratacion.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.TipoContratacion, "Tipo de contratación",
                x.Id, null, x.Nombre, _db.Ofertas.Count(o => o.TipoContratacionId == x.Id), true, null, false))
            .ToListAsync(cancellationToken));
        filas.AddRange(await _db.TipoDocumentoAspirantes.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdministracionMvcFila(TipoCatalogoNormalizado.TipoDocumentoAspirante, "Tipo de documento de Aspirante",
                x.Id, null, x.Nombre, _db.DocumentoAspirantes.Count(d => d.TipoDocumentoId == x.Id), true, null, false))
            .ToListAsync(cancellationToken));
        return filas;
    }

    public async Task CrearClasificacionAsync(CrearCatalogoClasificacionMvcDto datos,
        CancellationToken cancellationToken = default)
    {
        var nombre = NormalizarNombre(datos.Nombre, datos.Tipo switch
        {
            TipoCatalogoNormalizado.SistemaEducativo => 200,
            TipoCatalogoNormalizado.NivelFormacion or TipoCatalogoNormalizado.AreaFormacion => 200,
            _ => throw new ArgumentException("Sólo se pueden crear sistemas educativos, niveles y áreas de formación.")
        });

        switch (datos.Tipo)
        {
            case TipoCatalogoNormalizado.SistemaEducativo:
                if (!string.IsNullOrWhiteSpace(datos.Clave)) throw new ArgumentException("El sistema educativo no lleva clave.");
                _db.Entry(new SistemaEducativo { Nombre = nombre }).State = EntityState.Added;
                break;
            case TipoCatalogoNormalizado.NivelFormacion:
                _db.Entry(new NivelFormacion { Clave = NormalizarClave(datos.Clave), Nombre = nombre }).State = EntityState.Added;
                break;
            case TipoCatalogoNormalizado.AreaFormacion:
                _db.Entry(new AreaFormacion { Clave = NormalizarClave(datos.Clave), Nombre = nombre }).State = EntityState.Added;
                break;
            default:
                throw new ArgumentException("El catálogo indicado no admite nuevas filas.");
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task EditarNombreAsync(EditarNombreCatalogoMvcDto datos, CancellationToken cancellationToken = default)
    {
        if (datos.Id < 1) throw new ArgumentException("El identificador debe ser positivo.");
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var nombre = NormalizarNombre(datos.Nombre, LongitudNombre(datos.Tipo));
        var tieneReferencias = false;
        Action? aplicarNombre = null;

        switch (datos.Tipo)
        {
            case TipoCatalogoNormalizado.SistemaEducativo:
                var sistema = await _db.SistemasEducativos.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el sistema educativo indicado.");
                aplicarNombre = () => sistema.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.NivelFormacion:
                var nivel = await _db.NivelesFormacion.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el nivel de formación indicado.");
                tieneReferencias = await _db.ProgramasEducativos.AnyAsync(x => x.NivelFormacionId == datos.Id, cancellationToken);
                aplicarNombre = () => nivel.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.AreaFormacion:
                var areaFormacion = await _db.AreaFormaciones.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el área de formación indicada.");
                tieneReferencias = await _db.ExperienciasEducativas.AnyAsync(x => x.AreaFormacionId == datos.Id, cancellationToken);
                aplicarNombre = () => areaFormacion.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.GradoAcademico:
                var grado = await _db.GradoAcademicos.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el grado académico indicado.");
                tieneReferencias = await _db.TratamientosAcademicos.AnyAsync(x => x.GradoAcademicoId == datos.Id, cancellationToken)
                    || await _db.FormacionAspirantes.AnyAsync(x => x.GradoAcademicoId == datos.Id, cancellationToken);
                aplicarNombre = () => grado.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.TratamientoAcademico:
                var tratamiento = await _db.TratamientosAcademicos.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el tratamiento académico indicado.");
                tieneReferencias = await _db.IntegranteConsejoTecnicos.AnyAsync(x => x.TratamientoAcademicoId == datos.Id, cancellationToken);
                aplicarNombre = () => tratamiento.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.ModalidadRecepcion:
                var modalidad = await _db.ModalidadesRecepcion.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe la modalidad de recepción indicada.");
                tieneReferencias = await _db.Avisos.AnyAsync(x => x.ModalidadRecepcionId == datos.Id, cancellationToken);
                aplicarNombre = () => modalidad.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.TipoPlaza:
                var tipoPlaza = await _db.TipoPlazas.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el tipo de plaza indicado.");
                tieneReferencias = await _db.Ofertas.AnyAsync(x => x.TipoPlazaId == datos.Id, cancellationToken);
                aplicarNombre = () => tipoPlaza.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.TipoContratacion:
                var tipoContratacion = await _db.TiposContratacion.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el tipo de contratación indicado.");
                tieneReferencias = await _db.Ofertas.AnyAsync(x => x.TipoContratacionId == datos.Id, cancellationToken);
                aplicarNombre = () => tipoContratacion.Nombre = nombre;
                break;
            case TipoCatalogoNormalizado.TipoDocumentoAspirante:
                var tipoDocumento = await _db.TipoDocumentoAspirantes.AsTracking().SingleOrDefaultAsync(x => x.Id == datos.Id, cancellationToken)
                    ?? throw new KeyNotFoundException("No existe el tipo de documento indicado.");
                tieneReferencias = await _db.DocumentoAspirantes.AnyAsync(x => x.TipoDocumentoId == datos.Id, cancellationToken);
                aplicarNombre = () => tipoDocumento.Nombre = nombre;
                break;
            default:
                throw new ArgumentException("El tipo de catálogo no es válido.");
        }

        if (tieneReferencias && !EsClasificacion(datos.Tipo))
            throw new InvalidOperationException("Este valor ya tiene referencias y su nombre es inmutable.");
        aplicarNombre!();
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CambiarActivoAsync(TipoCatalogoNormalizado tipo, int id, bool activo,
        CancellationToken cancellationToken = default)
    {
        if (id < 1) throw new ArgumentException("El identificador debe ser positivo.");
        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        if (!activo)
        {
            var tieneDependenciasActivas = tipo switch
            {
                TipoCatalogoNormalizado.SistemaEducativo => await _db.ProgramasEducativos.AsNoTracking()
                    .AnyAsync(x => x.SistemaEducativoId == id && x.FechaEliminacion == null, cancellationToken),
                TipoCatalogoNormalizado.NivelFormacion => await _db.ProgramasEducativos.AsNoTracking()
                    .AnyAsync(x => x.NivelFormacionId == id && x.FechaEliminacion == null, cancellationToken),
                TipoCatalogoNormalizado.AreaFormacion => await _db.ExperienciasEducativas.AsNoTracking()
                    .AnyAsync(x => x.AreaFormacionId == id && x.FechaEliminacion == null, cancellationToken),
                _ => throw new ArgumentException("Sólo los catálogos de clasificación admiten baja lógica.")
            };
            if (tieneDependenciasActivas)
                throw new InvalidOperationException("No se puede dar de baja una clasificación que tiene registros vigentes relacionados.");
        }

        var filas = tipo switch
        {
            TipoCatalogoNormalizado.SistemaEducativo => await _db.SistemasEducativos.Where(x => x.Id == id
                    && (activo ? x.FechaEliminacion != null : x.FechaEliminacion == null))
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, activo ? null : ahora), cancellationToken),
            TipoCatalogoNormalizado.NivelFormacion => await _db.NivelesFormacion.Where(x => x.Id == id
                    && (activo ? x.FechaEliminacion != null : x.FechaEliminacion == null))
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, activo ? null : ahora), cancellationToken),
            TipoCatalogoNormalizado.AreaFormacion => await _db.AreaFormaciones.Where(x => x.Id == id
                    && (activo ? x.FechaEliminacion != null : x.FechaEliminacion == null))
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, activo ? null : ahora), cancellationToken),
            _ => throw new ArgumentException("Sólo los catálogos de clasificación admiten baja lógica.")
        };
        if (filas == 0 && !await ExisteClasificacionAsync(tipo, id, cancellationToken))
            throw new KeyNotFoundException("No existe la clasificación indicada.");
        await transaction.CommitAsync(cancellationToken);
    }

    private Task<bool> ExisteClasificacionAsync(TipoCatalogoNormalizado tipo, int id, CancellationToken cancellationToken) => tipo switch
    {
        TipoCatalogoNormalizado.SistemaEducativo => _db.SistemasEducativos.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        TipoCatalogoNormalizado.NivelFormacion => _db.NivelesFormacion.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        TipoCatalogoNormalizado.AreaFormacion => _db.AreaFormaciones.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken),
        _ => Task.FromResult(false)
    };

    private static bool EsClasificacion(TipoCatalogoNormalizado tipo) => tipo is
        TipoCatalogoNormalizado.SistemaEducativo or TipoCatalogoNormalizado.NivelFormacion or TipoCatalogoNormalizado.AreaFormacion;

    private static int LongitudNombre(TipoCatalogoNormalizado tipo) => tipo switch
    {
        TipoCatalogoNormalizado.SistemaEducativo or TipoCatalogoNormalizado.NivelFormacion or TipoCatalogoNormalizado.AreaFormacion => 200,
        TipoCatalogoNormalizado.GradoAcademico or TipoCatalogoNormalizado.TipoPlaza or TipoCatalogoNormalizado.TipoContratacion
            or TipoCatalogoNormalizado.TipoDocumentoAspirante => 150,
        TipoCatalogoNormalizado.TratamientoAcademico => 30,
        TipoCatalogoNormalizado.ModalidadRecepcion => 100,
        _ => throw new ArgumentException("El tipo de catálogo no es válido.")
    };

    private static string NormalizarNombre(string? nombre, int maximo)
    {
        var normalizado = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(normalizado) || normalizado.Length > maximo)
            throw new ArgumentException($"El nombre es obligatorio y debe tener hasta {maximo} caracteres.");
        return normalizado;
    }

    private static string NormalizarClave(string? clave)
    {
        var normalizada = clave?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(normalizada) || !ClaveClasificacion.IsMatch(normalizada))
            throw new ArgumentException("La clave debe contener de 1 a 50 letras ASCII o números, en mayúsculas.");
        return normalizada;
    }
}
