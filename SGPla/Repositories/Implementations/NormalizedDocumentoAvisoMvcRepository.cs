using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedDocumentoAvisoMvcRepository : IDocumentoAvisoMvcRepository
{
    // Identificador fijo del rol documentado en DATABASE.md y 0021_sembrar_catalogos_fijos.sql.
    private const byte RolEntidadAcademicaId = 3;
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;

    public NormalizedDocumentoAvisoMvcRepository(SgplaDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task ValidarCargaOriginalAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var avisoVigente = await (from aviso in _db.Avisos.AsNoTracking()
                                  join entidad in _db.EntidadAcademicas.AsNoTracking()
                                      on aviso.EntidadAcademicaId equals entidad.Id
                                  where aviso.Id == avisoId && aviso.EntidadAcademicaId == entidadAcademicaId
                                        && entidad.FechaEliminacion == null
                                        && (aviso.Estado == "CREADO" || aviso.Estado == "DEVUELTO_DGAA")
                                  select aviso.Id).AnyAsync(cancellationToken);
        if (!avisoVigente) throw new UnauthorizedAccessException("El Aviso no existe en tu ámbito o no admite cambios.");
        var usuarioAutorizado = await (from usuario in _db.Usuarios.AsNoTracking()
                                       join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                           on usuario.Id equals perfil.UsuarioId
                                       where usuario.Id == usuarioId && usuario.FechaEliminacion == null
                                             && perfil.EntidadAcademicaId == entidadAcademicaId
                                             && usuario.RolId == RolEntidadAcademicaId
                                       select usuario.Id).AnyAsync(cancellationToken);
        if (!usuarioAutorizado) throw new UnauthorizedAccessException("La cuenta no tiene ámbito vigente para esta Entidad Académica.");
    }

    public async Task<int> GuardarOriginalAsync(DocumentoAvisoOriginalMvc documento,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var aviso = await (from item in _db.Avisos.AsNoTracking()
                           join entidad in _db.EntidadAcademicas.AsNoTracking()
                               on item.EntidadAcademicaId equals entidad.Id
                           where item.Id == documento.AvisoId
                                 && item.EntidadAcademicaId == documento.EntidadAcademicaId
                                 && entidad.FechaEliminacion == null
                           select item).SingleOrDefaultAsync(cancellationToken);
        if (aviso is null) throw new UnauthorizedAccessException("El Aviso no pertenece a tu Entidad Académica.");
        if (aviso.Estado is not ("CREADO" or "DEVUELTO_DGAA"))
            throw new InvalidOperationException("El documento original sólo se puede cargar en Avisos creados o devueltos.");

        var usuarioAutorizado = await (from usuario in _db.Usuarios.AsNoTracking()
                                       join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                           on usuario.Id equals perfil.UsuarioId
                                       where usuario.Id == documento.UsuarioId
                                             && usuario.FechaEliminacion == null
                                             && perfil.EntidadAcademicaId == documento.EntidadAcademicaId
                                             && usuario.RolId == RolEntidadAcademicaId
                                       select usuario.Id).AnyAsync(cancellationToken);
        if (!usuarioAutorizado) throw new UnauthorizedAccessException("La cuenta no tiene ámbito vigente para esta Entidad Académica.");

        var revisionAbierta = await _db.RevisionAvisos.AsNoTracking()
            .AnyAsync(x => x.AvisoId == documento.AvisoId && x.ResueltoEn == null, cancellationToken);
        if (revisionAbierta) throw new InvalidOperationException("No se puede reemplazar el documento durante una revisión abierta.");

        var documentosPrevios = await _db.DocumentoAvisos.AsTracking()
            .Where(x => x.AvisoId == documento.AvisoId && x.Tipo == "ORIGINAL")
            .ToListAsync(cancellationToken);
        var siguienteVersion = documentosPrevios.Count == 0 ? 1 : checked(documentosPrevios.Max(x => x.NumeroVersion) + 1);
        foreach (var anterior in documentosPrevios.Where(x => x.EsVigente))
            anterior.EsVigente = false;

        var nuevo = new DocumentoAviso
        {
            AvisoId = documento.AvisoId,
            Tipo = "ORIGINAL",
            Nombre = documento.Nombre,
            Mime = documento.Mime,
            Tamano = documento.Tamano,
            ChecksumSha256 = documento.ChecksumSha256,
            ClaveAlmacenamiento = documento.ClaveAlmacenamiento,
            NumeroVersion = siguienteVersion,
            EsVigente = true,
            CargadoEn = _timeProvider.GetUtcNow().UtcDateTime,
            CargadoPorUsuarioId = documento.UsuarioId
        };
        _db.Entry(nuevo).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(CancellationToken.None);
        return nuevo.Id;
    }

    public async Task ValidarCargaFirmadoAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var permitida = await (from aviso in _db.Avisos.AsNoTracking()
                               join entidad in _db.EntidadAcademicas.AsNoTracking()
                                   on aviso.EntidadAcademicaId equals entidad.Id
                               join usuario in _db.Usuarios.AsNoTracking() on usuarioId equals usuario.Id
                               join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                   on usuario.Id equals perfil.UsuarioId
                               where aviso.Id == avisoId && aviso.EntidadAcademicaId == entidadAcademicaId
                                     && aviso.Estado == "AVALADO_DGAA" && entidad.FechaEliminacion == null
                                     && usuario.RolId == RolEntidadAcademicaId && usuario.FechaEliminacion == null
                                     && perfil.EntidadAcademicaId == entidadAcademicaId
                               select aviso.Id).AnyAsync(cancellationToken);
        if (!permitida)
            throw new UnauthorizedAccessException("El Aviso no está avalado dentro de tu ámbito o la cuenta no está vigente.");
    }

    public async Task<int> GuardarFirmadoAsync(DocumentoAvisoOriginalMvc documento,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var avisoVigente = await _db.Avisos.AsNoTracking().AnyAsync(item => item.Id == documento.AvisoId
            && item.EntidadAcademicaId == documento.EntidadAcademicaId
            && item.Estado == "AVALADO_DGAA"
            && _db.EntidadAcademicas.AsNoTracking().Any(entidad => entidad.Id == item.EntidadAcademicaId
                && entidad.FechaEliminacion == null), cancellationToken);
        if (!avisoVigente) throw new InvalidOperationException("Sólo se puede cargar el firmado de un Aviso avalado.");
        var usuarioAutorizado = await (from usuario in _db.Usuarios.AsNoTracking()
                                       join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                           on usuario.Id equals perfil.UsuarioId
                                       where usuario.Id == documento.UsuarioId && usuario.FechaEliminacion == null
                                             && perfil.EntidadAcademicaId == documento.EntidadAcademicaId
                                             && usuario.RolId == RolEntidadAcademicaId
                                       select usuario.Id).AnyAsync(cancellationToken);
        if (!usuarioAutorizado) throw new UnauthorizedAccessException("La cuenta no tiene ámbito vigente para esta Entidad Académica.");

        var documentosPrevios = await _db.DocumentoAvisos.AsTracking()
            .Where(x => x.AvisoId == documento.AvisoId && x.Tipo == "FIRMADO")
            .ToListAsync(cancellationToken);
        var siguienteVersion = documentosPrevios.Count == 0 ? 1 : checked(documentosPrevios.Max(x => x.NumeroVersion) + 1);
        foreach (var anterior in documentosPrevios.Where(x => x.EsVigente)) anterior.EsVigente = false;
        var nuevo = new DocumentoAviso
        {
            AvisoId = documento.AvisoId,
            Tipo = "FIRMADO",
            Nombre = documento.Nombre,
            Mime = documento.Mime,
            Tamano = documento.Tamano,
            ChecksumSha256 = documento.ChecksumSha256,
            ClaveAlmacenamiento = documento.ClaveAlmacenamiento,
            NumeroVersion = siguienteVersion,
            EsVigente = true,
            CargadoEn = _timeProvider.GetUtcNow().UtcDateTime,
            CargadoPorUsuarioId = documento.UsuarioId
        };
        _db.Entry(nuevo).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        var actualizados = await _db.Avisos.Where(x => x.Id == documento.AvisoId
                && x.EntidadAcademicaId == documento.EntidadAcademicaId && x.Estado == "AVALADO_DGAA")
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Estado, "FIRMADO"), cancellationToken);
        if (actualizados != 1) throw new InvalidOperationException("El Aviso dejó de estar avalado durante la carga del firmado.");
        await transaction.CommitAsync(CancellationToken.None);
        return nuevo.Id;
    }

    public async Task<IReadOnlyList<string>> ObtenerClavesBorradorAsync(int avisoId, int entidadAcademicaId,
        int usuarioId, CancellationToken cancellationToken = default)
    {
        await ValidarPropietarioBorradorAsync(avisoId, entidadAcademicaId, usuarioId, cancellationToken);
        return await _db.DocumentoAvisos.AsNoTracking().Where(x => x.AvisoId == avisoId)
            .OrderBy(x => x.Id).Select(x => x.ClaveAlmacenamiento).ToListAsync(cancellationToken);
    }

    public async Task EliminarBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        IReadOnlyList<string> clavesEsperadas, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarPropietarioBorradorAsync(avisoId, entidadAcademicaId, usuarioId, cancellationToken);

        var clavesActuales = await _db.DocumentoAvisos.AsNoTracking().Where(x => x.AvisoId == avisoId)
            .OrderBy(x => x.Id).Select(x => x.ClaveAlmacenamiento).ToListAsync(cancellationToken);
        if (!clavesActuales.SequenceEqual(clavesEsperadas, StringComparer.Ordinal))
            throw new InvalidOperationException("Los documentos del borrador cambiaron durante la eliminación; vuelve a intentar.");

        var tieneSolicitud = await (from solicitud in _db.Solicitudes.AsNoTracking()
                                    join enlace in _db.AvisoOfertas.AsNoTracking()
                                        on solicitud.AvisoOfertaId equals enlace.Id
                                    where enlace.AvisoId == avisoId
                                    select solicitud.Id).AnyAsync(cancellationToken);
        var tieneActaOferta = await (from actaOferta in _db.ActaOfertas.AsNoTracking()
                                     join enlace in _db.AvisoOfertas.AsNoTracking()
                                         on actaOferta.AvisoOfertaId equals enlace.Id
                                     where enlace.AvisoId == avisoId
                                     select actaOferta.Id).AnyAsync(cancellationToken);
        if (tieneSolicitud || tieneActaOferta
            || await _db.ActaConsejoTecnicos.AsNoTracking().AnyAsync(x => x.AvisoId == avisoId, cancellationToken)
            || await _db.RevisionAvisos.AsNoTracking().AnyAsync(x => x.AvisoId == avisoId, cancellationToken))
            throw new InvalidOperationException("No se puede eliminar un Aviso con Solicitudes, revisiones o Actas relacionadas.");

        var ofertaIds = await _db.AvisoOfertas.AsNoTracking().Where(x => x.AvisoId == avisoId)
            .Select(x => x.OfertaId).Distinct().ToArrayAsync(cancellationToken);
        if (ofertaIds.Length > 0)
        {
            var disponiblesAlLiberar = await _db.Ofertas.AsNoTracking()
                .Where(x => ofertaIds.Contains(x.Id) && x.Estado == "EN_PUBLICACION" && x.CerradaEn == null)
                .Select(x => x.Id).ToListAsync(cancellationToken);
            if (disponiblesAlLiberar.Count != ofertaIds.Length)
                throw new InvalidOperationException("Una Oferta vinculada ya no está en publicación; no se eliminó el borrador.");
            var liberadas = await _db.Ofertas.Where(x => ofertaIds.Contains(x.Id)
                    && x.Estado == "EN_PUBLICACION" && x.CerradaEn == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Estado, "DISPONIBLE"), cancellationToken);
            if (liberadas != ofertaIds.Length)
                throw new InvalidOperationException("No se pudieron liberar todas las Ofertas del borrador.");
        }

        await _db.DocumentoAvisos.Where(x => x.AvisoId == avisoId).ExecuteDeleteAsync(cancellationToken);
        await _db.HorarioRecepcionRequisitos.Where(x => x.AvisoId == avisoId).ExecuteDeleteAsync(cancellationToken);
        await _db.AvisoOfertas.Where(x => x.AvisoId == avisoId).ExecuteDeleteAsync(cancellationToken);
        var eliminados = await _db.Avisos.Where(x => x.Id == avisoId && x.EntidadAcademicaId == entidadAcademicaId
                && x.Estado == "CREADO")
            .ExecuteDeleteAsync(cancellationToken);
        if (eliminados != 1) throw new InvalidOperationException("El Aviso dejó de ser un borrador eliminable.");
        await transaction.CommitAsync(CancellationToken.None);
    }

    private async Task ValidarPropietarioBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken)
    {
        var permitido = await (from aviso in _db.Avisos.AsNoTracking()
                               join entidad in _db.EntidadAcademicas.AsNoTracking()
                                   on aviso.EntidadAcademicaId equals entidad.Id
                               join usuario in _db.Usuarios.AsNoTracking() on usuarioId equals usuario.Id
                               join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                   on usuario.Id equals perfil.UsuarioId
                               where aviso.Id == avisoId && aviso.EntidadAcademicaId == entidadAcademicaId
                                     && aviso.Estado == "CREADO" && entidad.FechaEliminacion == null
                                     && usuario.RolId == RolEntidadAcademicaId && usuario.FechaEliminacion == null
                                     && perfil.EntidadAcademicaId == entidadAcademicaId
                               select aviso.Id).AnyAsync(cancellationToken);
        if (!permitido)
            throw new UnauthorizedAccessException("Sólo la Entidad Académica propietaria puede eliminar un borrador CREADO.");
    }

    public async Task<DocumentoAvisoDescargaMvc?> ObtenerParaDescargaAsync(int avisoId, int documentoId,
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var query = from documento in _db.DocumentoAvisos.AsNoTracking()
                    join aviso in _db.Avisos.AsNoTracking() on documento.AvisoId equals aviso.Id
                    join entidad in _db.EntidadAcademicas.AsNoTracking()
                        on aviso.EntidadAcademicaId equals entidad.Id
                    join usuario in _db.Usuarios.AsNoTracking() on usuarioId equals usuario.Id
                    where documento.Id == documentoId && documento.AvisoId == avisoId
                          && entidad.FechaEliminacion == null && usuario.FechaEliminacion == null
                          && ((usuario.RolId == RolEntidadAcademicaId
                               && _db.UsuariosEntidadAcademica.AsNoTracking().Any(perfil =>
                                   perfil.UsuarioId == usuario.Id
                                   && perfil.EntidadAcademicaId == entidad.Id))
                              || (usuario.RolId == 2
                                  && _db.UsuariosDgaa.AsNoTracking().Any(perfil =>
                                      perfil.UsuarioId == usuario.Id
                                      && perfil.AreaAcademicaId == entidad.AreaAcademicaId)))
                    select new DocumentoAvisoDescargaMvc(documento.AvisoId, documento.Id,
                        documento.Nombre, documento.Mime, documento.ClaveAlmacenamiento, documento.ChecksumSha256);
        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteClaveAlmacenamientoAsync(string claveRelativa,
        CancellationToken cancellationToken = default) => _db.DocumentoAvisos.AsNoTracking()
        .AnyAsync(x => x.ClaveAlmacenamiento == claveRelativa, cancellationToken);

    public Task<bool> ExisteAvisoAsync(int avisoId, CancellationToken cancellationToken = default) =>
        _db.Avisos.AsNoTracking().AnyAsync(x => x.Id == avisoId, cancellationToken);
}
