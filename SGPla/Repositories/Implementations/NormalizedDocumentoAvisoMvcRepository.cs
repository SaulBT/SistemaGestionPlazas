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
                        documento.Nombre, documento.Mime, documento.ClaveAlmacenamiento);
        return await query.SingleOrDefaultAsync(cancellationToken);
    }
}
