using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedIntegranteConsejoTecnicoMvcRepository : IIntegranteConsejoTecnicoMvcRepository
{
    private const byte RolEntidadAcademicaId = 3;
    private readonly SgplaDbContext _db;

    public NormalizedIntegranteConsejoTecnicoMvcRepository(SgplaDbContext db) => _db = db;

    public async Task<IntegranteConsejoTecnicoMvcPagina> BuscarAsync(int usuarioId, int entidadAcademicaId,
        IntegranteConsejoTecnicoMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);
        var query = from integrante in _db.IntegranteConsejoTecnicos.AsNoTracking()
                    join tratamiento in _db.TratamientosAcademicos.AsNoTracking()
                        on integrante.TratamientoAcademicoId equals tratamiento.Id
                    join grado in _db.GradoAcademicos.AsNoTracking()
                        on tratamiento.GradoAcademicoId equals grado.Id
                    where integrante.EntidadAcademicaId == entidadAcademicaId
                    select new
                    {
                        integrante.Id, integrante.Nombre, integrante.Cargo,
                        integrante.TratamientoAcademicoId, Tratamiento = tratamiento.Nombre,
                        Grado = grado.Nombre, integrante.FechaInicio, integrante.FechaFin,
                        TieneAsistencias = _db.ActaAsistencias.AsNoTracking()
                            .Any(asistencia => asistencia.IntegranteConsejoTecnicoId == integrante.Id)
                    };

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var busqueda = filtro.Busqueda.Trim();
            query = query.Where(x => x.Nombre.Contains(busqueda) || x.Cargo.Contains(busqueda));
        }

        var total = await query.CountAsync(cancellationToken);
        var ultimaPagina = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        pagina = Math.Min(pagina, ultimaPagina);
        var items = await query.OrderByDescending(x => x.FechaFin == null)
            .ThenBy(x => x.Nombre).ThenByDescending(x => x.FechaInicio)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new IntegranteConsejoTecnicoMvcItem(x.Id, x.Nombre, x.Cargo,
                x.TratamientoAcademicoId, x.Tratamiento, x.Grado, x.FechaInicio, x.FechaFin,
                x.TieneAsistencias))
            .ToListAsync(cancellationToken);

        var tratamientos = await (from tratamiento in _db.TratamientosAcademicos.AsNoTracking()
                                  join grado in _db.GradoAcademicos.AsNoTracking()
                                      on tratamiento.GradoAcademicoId equals grado.Id
                                  orderby grado.Nombre, tratamiento.Nombre
                                  select new IntegranteConsejoTecnicoMvcTratamiento(
                                      tratamiento.Id, tratamiento.Nombre, grado.Nombre))
            .ToListAsync(cancellationToken);
        return new IntegranteConsejoTecnicoMvcPagina(items, total, pagina, tamano, tratamientos);
    }

    public async Task CrearAsync(int usuarioId, int entidadAcademicaId,
        IntegranteConsejoTecnicoMvcCambio cambio, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        await ValidarTratamientoAsync(cambio.TratamientoAcademicoId, cancellationToken);
        await ValidarSinTraslapeAsync(entidadAcademicaId, cambio, excluirId: null, cancellationToken);

        _db.Entry(new IntegranteConsejoTecnico
        {
            EntidadAcademicaId = entidadAcademicaId,
            Nombre = cambio.Nombre,
            Cargo = cambio.Cargo,
            TratamientoAcademicoId = cambio.TratamientoAcademicoId,
            FechaInicio = cambio.FechaInicio,
            FechaFin = cambio.FechaFin
        }).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CambiarVigenciaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        IntegranteConsejoTecnicoMvcCambio cambio, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var actual = await _db.IntegranteConsejoTecnicos.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == integranteId && x.EntidadAcademicaId == entidadAcademicaId,
                cancellationToken)
            ?? throw new KeyNotFoundException("El integrante no existe en la Entidad Académica vigente.");
        if (actual.FechaFin is not null)
            throw new InvalidOperationException("Sólo se puede cambiar la vigencia del nombramiento actual.");
        if (cambio.FechaInicio <= actual.FechaInicio)
            throw new ArgumentException("La nueva vigencia debe comenzar después de la vigencia actual.");

        await ValidarTratamientoAsync(cambio.TratamientoAcademicoId, cancellationToken);
        await ValidarSinTraslapeAsync(entidadAcademicaId, cambio, integranteId, cancellationToken);
        actual.FechaFin = cambio.FechaInicio.AddDays(-1);
        _db.Entry(new IntegranteConsejoTecnico
        {
            EntidadAcademicaId = entidadAcademicaId,
            Nombre = cambio.Nombre,
            Cargo = cambio.Cargo,
            TratamientoAcademicoId = cambio.TratamientoAcademicoId,
            FechaInicio = cambio.FechaInicio,
            FechaFin = cambio.FechaFin
        }).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task EliminarCapturaErroneaAsync(int usuarioId, int entidadAcademicaId, int integranteId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var integrante = await _db.IntegranteConsejoTecnicos.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == integranteId && x.EntidadAcademicaId == entidadAcademicaId,
                cancellationToken)
            ?? throw new KeyNotFoundException("El integrante no existe en la Entidad Académica vigente.");
        if (await _db.ActaAsistencias.AsNoTracking()
                .AnyAsync(x => x.IntegranteConsejoTecnicoId == integranteId, cancellationToken))
            throw new InvalidOperationException("No se puede eliminar un integrante ya referenciado en un Acta.");
        _db.Entry(integrante).State = EntityState.Deleted;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ValidarSinTraslapeAsync(int entidadAcademicaId,
        IntegranteConsejoTecnicoMvcCambio cambio, int? excluirId, CancellationToken cancellationToken)
    {
        var nombre = cambio.Nombre.Trim();
        var query = _db.IntegranteConsejoTecnicos.AsNoTracking()
            .Where(x => x.EntidadAcademicaId == entidadAcademicaId && x.Nombre == nombre);
        if (excluirId.HasValue) query = query.Where(x => x.Id != excluirId.Value);
        var hayTraslape = await query.AnyAsync(x =>
            (!cambio.FechaFin.HasValue || x.FechaInicio <= cambio.FechaFin.Value)
            && (!x.FechaFin.HasValue || x.FechaFin.Value >= cambio.FechaInicio), cancellationToken);
        if (hayTraslape)
            throw new InvalidOperationException("Las vigencias de una misma persona no pueden traslaparse en la Entidad Académica.");
    }

    private async Task ValidarTratamientoAsync(int tratamientoId, CancellationToken cancellationToken)
    {
        if (tratamientoId < 1 || !await _db.TratamientosAcademicos.AsNoTracking()
                .AnyAsync(x => x.Id == tratamientoId, cancellationToken))
            throw new ArgumentException("Selecciona un tratamiento académico vigente.");
    }

    private async Task ValidarAmbitoAsync(int usuarioId, int entidadAcademicaId,
        CancellationToken cancellationToken)
    {
        var autorizado = await (from usuario in _db.Usuarios.AsNoTracking()
                                join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                    on usuario.Id equals perfil.UsuarioId
                                join entidad in _db.EntidadAcademicas.AsNoTracking()
                                    on perfil.EntidadAcademicaId equals entidad.Id
                                where usuario.Id == usuarioId && usuario.FechaEliminacion == null
                                      && usuario.RolId == RolEntidadAcademicaId
                                      && entidad.Id == entidadAcademicaId
                                      && entidad.FechaEliminacion == null
                                select usuario.Id).AnyAsync(cancellationToken);
        if (!autorizado)
            throw new UnauthorizedAccessException("La cuenta no tiene ámbito vigente para esta Entidad Académica.");
    }
}
