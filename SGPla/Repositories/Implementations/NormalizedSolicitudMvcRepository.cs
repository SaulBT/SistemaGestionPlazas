using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedSolicitudMvcRepository : ISolicitudMvcRepository
{
    private const byte RolEntidadAcademica = 3;
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;

    public NormalizedSolicitudMvcRepository(SgplaDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<SolicitudMvcListado?> ListarPorAvisoAsync(int usuarioId, int entidadAcademicaId,
        int avisoId, CancellationToken cancellationToken = default)
    {
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var aviso = await _db.Avisos.AsNoTracking().Where(x => x.Id == avisoId && x.EntidadAcademicaId == entidadAcademicaId)
            .Select(x => new { x.Id, x.Estado }).SingleOrDefaultAsync(cancellationToken);
        if (aviso is null) return null;

        var solicitudes = await (from solicitud in _db.Solicitudes.AsNoTracking()
                                 join vinculo in _db.AvisoOfertas.AsNoTracking()
                                     on solicitud.AvisoOfertaId equals vinculo.Id
                                 join perfil in _db.PerfilAspirantes.AsNoTracking()
                                     on solicitud.PerfilAspiranteId equals perfil.Id
                                 where vinculo.AvisoId == avisoId
                                 orderby solicitud.RegistradaEn descending, solicitud.Id descending
                                 select new SolicitudMvcItem(solicitud.Id, solicitud.AvisoOfertaId, solicitud.Estado,
                                     perfil.Nombre, perfil.Correo, solicitud.RegistradaEn, perfil.Id))
            .ToListAsync(cancellationToken);
        var grados = await _db.GradoAcademicos.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new GradoAcademicoSolicitudMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken);
        var tiposDocumento = await _db.TipoDocumentoAspirantes.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new TipoDocumentoAspiranteSolicitudMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken);
        var vacantes = new List<VacanteSolicitudMvcOpcion>();
        if (aviso.Estado == "PUBLICADO" && await EstaEnHorarioRecepcionAsync(aviso.Id, cancellationToken))
        {
            vacantes = await (from vinculo in _db.AvisoOfertas.AsNoTracking()
                              join oferta in _db.Ofertas.AsNoTracking() on vinculo.OfertaId equals oferta.Id
                              join programacion in _db.ProgramacionAcademicas.AsNoTracking()
                                  on oferta.ProgramacionAcademicaId equals programacion.Id
                              join ee in _db.ExperienciasEducativas.AsNoTracking()
                                  on programacion.ExperienciaEducativaId equals ee.Id
                              where vinculo.AvisoId == avisoId && vinculo.CerradoEn == null
                                    && oferta.Estado == "EN_PUBLICACION" && oferta.CerradaEn == null
                                    && programacion.FechaEliminacion == null && ee.FechaEliminacion == null
                              orderby oferta.ClavePlaza
                              select new VacanteSolicitudMvcOpcion(vinculo.Id, oferta.ClavePlaza, programacion.Nrc,
                                  ee.MateriaEe, ee.CursoEe)).ToListAsync(cancellationToken);
        }
        return new SolicitudMvcListado(aviso.Id, aviso.Estado, solicitudes, grados, vacantes, tiposDocumento);
    }

    public async Task<int> RegistrarAsync(int usuarioId, int entidadAcademicaId,
        RegistrarSolicitudMvcDatos datos, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        await ValidarVentanaRecepcionAsync(entidadAcademicaId, datos.AvisoOfertaId, cancellationToken);
        await ValidarGradosAsync(datos.Formaciones.Select(x => x.GradoAcademicoId), cancellationToken);

        var aspiranteId = await _db.PerfilAspirantes.AsNoTracking()
            .Where(x => x.EsVigente && x.Correo == datos.Correo)
            .Select(x => (int?)x.AspiranteId).SingleOrDefaultAsync(cancellationToken);
        if (aspiranteId is null)
        {
            var aspirante = new Aspirante();
            _db.Aspirantes.Add(aspirante);
            await _db.SaveChangesAsync(cancellationToken);
            aspiranteId = aspirante.Id;
        }

        if (await _db.Solicitudes.AsNoTracking().AnyAsync(x => x.AvisoOfertaId == datos.AvisoOfertaId
                && x.AspiranteId == aspiranteId.Value, cancellationToken))
            throw new InvalidOperationException("Este Aspirante ya tiene una Solicitud para esta vacante.");

        var anterior = await _db.PerfilAspirantes.AsTracking()
            .SingleOrDefaultAsync(x => x.AspiranteId == aspiranteId.Value && x.EsVigente, cancellationToken);
        var numeroVersion = await _db.PerfilAspirantes.AsNoTracking().Where(x => x.AspiranteId == aspiranteId.Value)
            .Select(x => (int?)x.NumeroVersion).MaxAsync(cancellationToken) ?? 0;
        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        if (anterior is not null)
        {
            anterior.EsVigente = false;
            await _db.SaveChangesAsync(cancellationToken);
        }
        var perfil = new PerfilAspirante
        {
            AspiranteId = aspiranteId.Value,
            NumeroVersion = checked(numeroVersion + 1),
            Nombre = datos.Nombre,
            Correo = datos.Correo,
            PuestoActual = datos.PuestoActual,
            DescripcionPerfil = datos.DescripcionPerfil,
            EsVigente = true,
            CreadoEn = ahora,
            CreadoPorUsuarioId = usuarioId
        };
        _db.PerfilAspirantes.Add(perfil);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var formacion in datos.Formaciones)
            _db.FormacionAspirantes.Add(new FormacionAspirante
            {
                PerfilAspiranteId = perfil.Id,
                GradoAcademicoId = formacion.GradoAcademicoId,
                Descripcion = formacion.Descripcion
            });
        var solicitud = new Solicitud
        {
            AvisoOfertaId = datos.AvisoOfertaId,
            AspiranteId = aspiranteId.Value,
            PerfilAspiranteId = perfil.Id,
            RegistradaEn = ahora,
            Estado = "REGISTRADA",
            Observaciones = datos.Observaciones
        };
        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return solicitud.Id;
    }

    public async Task<SolicitudMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int entidadAcademicaId,
        int avisoId, int solicitudId, CancellationToken cancellationToken = default)
    {
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var fila = await (from solicitud in _db.Solicitudes.AsNoTracking()
                          join vinculo in _db.AvisoOfertas.AsNoTracking()
                              on solicitud.AvisoOfertaId equals vinculo.Id
                          join aviso in _db.Avisos.AsNoTracking() on vinculo.AvisoId equals aviso.Id
                          join perfil in _db.PerfilAspirantes.AsNoTracking() on solicitud.PerfilAspiranteId equals perfil.Id
                          where aviso.Id == avisoId && aviso.EntidadAcademicaId == entidadAcademicaId
                                && solicitud.Id == solicitudId
                          select new SolicitudMvcDetalle(solicitud.Id, aviso.Id, vinculo.Id, solicitud.AspiranteId,
                              perfil.Id, solicitud.Estado, perfil.Nombre, perfil.Correo, perfil.PuestoActual,
                              perfil.DescripcionPerfil, solicitud.Observaciones, solicitud.RegistradaEn,
                              solicitud.AdmitidaEn, solicitud.NoAdmitidaEn, solicitud.MotivoNoAdmision,
                              new List<FormacionSolicitudMvcItem>(), new List<DocumentoSolicitudMvcItem>()))
            .SingleOrDefaultAsync(cancellationToken);
        if (fila is null) return null;
        var formaciones = await (from formacion in _db.FormacionAspirantes.AsNoTracking()
                                 join grado in _db.GradoAcademicos.AsNoTracking()
                                     on formacion.GradoAcademicoId equals grado.Id
                                 where formacion.PerfilAspiranteId == fila.PerfilAspiranteId
                                 orderby grado.Nombre
                                 select new FormacionSolicitudMvcItem(formacion.Id, grado.Id, grado.Nombre, formacion.Descripcion))
            .ToListAsync(cancellationToken);
        var documentos = await (from link in _db.SolicitudDocumentos.AsNoTracking()
                                join version in _db.VersionDocumentoAspirantes.AsNoTracking()
                                    on link.VersionDocumentoAspiranteId equals version.Id
                                join documento in _db.DocumentoAspirantes.AsNoTracking()
                                    on version.DocumentoAspiranteId equals documento.Id
                                join tipo in _db.TipoDocumentoAspirantes.AsNoTracking()
                                    on documento.TipoDocumentoId equals tipo.Id
                                where link.SolicitudId == solicitudId && documento.AspiranteId == fila.AspiranteId
                                orderby tipo.Nombre, version.NumeroVersion descending
                                select new DocumentoSolicitudMvcItem(documento.Id, version.Id, tipo.Id, tipo.Nombre,
                                    version.Nombre, version.NumeroVersion, version.EsVigente)).ToListAsync(cancellationToken);
        return fila with { Formaciones = formaciones, Documentos = documentos };
    }

    public async Task ActualizarPerfilAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        EditarPerfilSolicitudMvcDatos datos, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var solicitud = await ObtenerSolicitudTrackingAsync(entidadAcademicaId, avisoId, solicitudId, cancellationToken);
        if (solicitud.Estado != "REGISTRADA")
            throw new InvalidOperationException("El perfil sólo se puede modificar mientras la Solicitud está registrada.");
        await ValidarGradosAsync(datos.Formaciones.Select(x => x.GradoAcademicoId), cancellationToken);
        if (await _db.PerfilAspirantes.AsNoTracking().AnyAsync(x => x.EsVigente
                && x.AspiranteId != solicitud.AspiranteId && x.Correo == datos.Correo, cancellationToken))
            throw new InvalidOperationException("El correo ya pertenece a otro Aspirante.");

        var perfilAnterior = await _db.PerfilAspirantes.AsTracking()
            .SingleAsync(x => x.Id == solicitud.PerfilAspiranteId && x.AspiranteId == solicitud.AspiranteId, cancellationToken);
        var maxVersion = await _db.PerfilAspirantes.AsNoTracking().Where(x => x.AspiranteId == solicitud.AspiranteId)
            .Select(x => (int?)x.NumeroVersion).MaxAsync(cancellationToken) ?? perfilAnterior.NumeroVersion;
        perfilAnterior.EsVigente = false;
        await _db.SaveChangesAsync(cancellationToken);
        var nuevo = new PerfilAspirante
        {
            AspiranteId = solicitud.AspiranteId, NumeroVersion = checked(maxVersion + 1), Nombre = datos.Nombre,
            Correo = datos.Correo, PuestoActual = datos.PuestoActual, DescripcionPerfil = datos.DescripcionPerfil,
            EsVigente = true, CreadoEn = _timeProvider.GetUtcNow().UtcDateTime, CreadoPorUsuarioId = usuarioId
        };
        _db.PerfilAspirantes.Add(nuevo);
        await _db.SaveChangesAsync(cancellationToken);
        foreach (var formacion in datos.Formaciones)
            _db.FormacionAspirantes.Add(new FormacionAspirante
            {
                PerfilAspiranteId = nuevo.Id, GradoAcademicoId = formacion.GradoAcademicoId,
                Descripcion = formacion.Descripcion
            });
        solicitud.PerfilAspiranteId = nuevo.Id;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ResolverAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        ResolverSolicitudMvcDatos datos, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var solicitud = await ObtenerSolicitudTrackingAsync(entidadAcademicaId, avisoId, solicitudId, cancellationToken);
        if (solicitud.Estado != "REGISTRADA")
            throw new InvalidOperationException("Sólo se pueden resolver Solicitudes registradas.");
        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        if (datos.Admitir)
        {
            solicitud.Estado = "ADMITIDA";
            solicitud.AdmitidaEn = ahora;
            solicitud.AdmitidaPorUsuarioId = usuarioId;
        }
        else
        {
            solicitud.Estado = "NO_ADMITIDA";
            solicitud.NoAdmitidaEn = ahora;
            solicitud.NoAdmitidaPorUsuarioId = usuarioId;
            solicitud.MotivoNoAdmision = datos.MotivoNoAdmision;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RetirarAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        string? motivo, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var solicitud = await ObtenerSolicitudTrackingAsync(entidadAcademicaId, avisoId, solicitudId, cancellationToken);
        if (solicitud.Estado != "ADMITIDA")
            throw new InvalidOperationException("Sólo se puede retirar una Solicitud admitida.");
        var aviso = await _db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId, cancellationToken);
        var ahora = _timeProvider.GetUtcNow();
        var fechaLocal = ObtenerFechaLocal(ahora);
        if (aviso.FechaConsejoTecnico is null || fechaLocal >= aviso.FechaConsejoTecnico.Value
            || await _db.ActaConsejoTecnicos.AsNoTracking().AnyAsync(x => x.AvisoId == avisoId
                && x.FechaEliminacion == null, cancellationToken))
            throw new InvalidOperationException("La Solicitud ya no se puede retirar después del inicio de la sesión o al existir un Acta.");
        solicitud.Estado = "RETIRADA";
        solicitud.RetiradaEn = ahora.UtcDateTime;
        solicitud.MotivoRetiro = motivo;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<int> AgregarDocumentoAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        int solicitudId, DocumentoAspiranteMvcDatos documento, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var solicitud = await ObtenerSolicitudTrackingAsync(entidadAcademicaId, avisoId, solicitudId, cancellationToken);
        if (solicitud.Estado != "REGISTRADA")
            throw new InvalidOperationException("Los documentos sólo se pueden modificar mientras la Solicitud está registrada.");
        var tipoExiste = await _db.TipoDocumentoAspirantes.AsNoTracking()
            .AnyAsync(x => x.Id == documento.TipoDocumentoId, cancellationToken);
        if (!tipoExiste) throw new ArgumentException("Seleccione un tipo de documento válido.");

        DocumentoAspirante cabecera;
        int numeroVersion;
        if (documento.DocumentoAspiranteId.HasValue)
        {
            cabecera = await _db.DocumentoAspirantes.AsTracking().SingleOrDefaultAsync(x =>
                x.Id == documento.DocumentoAspiranteId.Value && x.AspiranteId == solicitud.AspiranteId
                && x.TipoDocumentoId == documento.TipoDocumentoId, cancellationToken)
                ?? throw new ArgumentException("El documento seleccionado no pertenece a este Aspirante o tipo.");
            var vigente = await _db.VersionDocumentoAspirantes.AsTracking().SingleOrDefaultAsync(x =>
                x.DocumentoAspiranteId == cabecera.Id && x.EsVigente, cancellationToken);
            if (vigente is null) throw new InvalidOperationException("El documento lógico no tiene una versión vigente.");
            vigente.EsVigente = false;
            await _db.SaveChangesAsync(cancellationToken);
            numeroVersion = checked((await _db.VersionDocumentoAspirantes.AsNoTracking()
                .Where(x => x.DocumentoAspiranteId == cabecera.Id).Select(x => (int?)x.NumeroVersion)
                .MaxAsync(cancellationToken) ?? 0) + 1);
        }
        else
        {
            cabecera = new DocumentoAspirante
            {
                AspiranteId = solicitud.AspiranteId,
                TipoDocumentoId = documento.TipoDocumentoId
            };
            _db.DocumentoAspirantes.Add(cabecera);
            await _db.SaveChangesAsync(cancellationToken);
            numeroVersion = 1;
        }
        var version = new VersionDocumentoAspirante
        {
            DocumentoAspiranteId = cabecera.Id,
            Nombre = documento.Nombre,
            Mime = documento.Mime,
            Tamano = documento.Tamano,
            ChecksumSha256 = documento.ChecksumSha256,
            ClaveAlmacenamiento = documento.ClaveRelativa,
            NumeroVersion = numeroVersion,
            EsVigente = true,
            CargadoEn = _timeProvider.GetUtcNow().UtcDateTime,
            CargadoPorUsuarioId = usuarioId
        };
        _db.VersionDocumentoAspirantes.Add(version);
        await _db.SaveChangesAsync(cancellationToken);
        _db.SolicitudDocumentos.Add(new SolicitudDocumento
        {
            SolicitudId = solicitudId,
            VersionDocumentoAspiranteId = version.Id
        });
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return version.Id;
    }

    public async Task<(string Nombre, string Mime, string ClaveRelativa, byte[] Checksum)?> ObtenerDocumentoAsync(int usuarioId,
        int entidadAcademicaId, int avisoId, int solicitudId, int versionDocumentoId,
        CancellationToken cancellationToken = default)
    {
        await ValidarAmbitoAsync(usuarioId, entidadAcademicaId, cancellationToken);
        var fila = await (from solicitud in _db.Solicitudes.AsNoTracking()
                      join vinculo in _db.AvisoOfertas.AsNoTracking() on solicitud.AvisoOfertaId equals vinculo.Id
                      join aviso in _db.Avisos.AsNoTracking() on vinculo.AvisoId equals aviso.Id
                      join link in _db.SolicitudDocumentos.AsNoTracking() on solicitud.Id equals link.SolicitudId
                      join version in _db.VersionDocumentoAspirantes.AsNoTracking()
                          on link.VersionDocumentoAspiranteId equals version.Id
                      join documento in _db.DocumentoAspirantes.AsNoTracking()
                          on version.DocumentoAspiranteId equals documento.Id
                      where solicitud.Id == solicitudId && aviso.Id == avisoId
                            && aviso.EntidadAcademicaId == entidadAcademicaId
                            && version.Id == versionDocumentoId && documento.AspiranteId == solicitud.AspiranteId
                      select new
                      {
                          version.Nombre,
                          version.Mime,
                          version.ClaveAlmacenamiento,
                          Checksum = version.ChecksumSha256
                      }).SingleOrDefaultAsync(cancellationToken);
        return fila is null ? null : (fila.Nombre, fila.Mime, fila.ClaveAlmacenamiento, fila.Checksum);
    }

    public Task<bool> ExisteClaveDocumentoAsync(string claveRelativa, CancellationToken cancellationToken = default) =>
        _db.VersionDocumentoAspirantes.AsNoTracking().AnyAsync(x => x.ClaveAlmacenamiento == claveRelativa,
            cancellationToken);

    private async Task<Solicitud> ObtenerSolicitudTrackingAsync(int entidadAcademicaId, int avisoId,
        int solicitudId, CancellationToken cancellationToken)
    {
        var existeEnAmbito = await (from item in _db.Solicitudes.AsNoTracking()
                                     join vinculo in _db.AvisoOfertas.AsNoTracking()
                                         on item.AvisoOfertaId equals vinculo.Id
                                     join aviso in _db.Avisos.AsNoTracking() on vinculo.AvisoId equals aviso.Id
                                     where item.Id == solicitudId && aviso.Id == avisoId
                                           && aviso.EntidadAcademicaId == entidadAcademicaId
                                     select item.Id).AnyAsync(cancellationToken);
        if (!existeEnAmbito)
            throw new KeyNotFoundException("No existe la Solicitud dentro del ámbito de esta Entidad Académica.");

        var solicitud = _db.Solicitudes.Local.SingleOrDefault(x => x.Id == solicitudId)
            ?? await _db.Solicitudes.AsTracking().SingleAsync(x => x.Id == solicitudId, cancellationToken);
        await _db.Entry(solicitud).ReloadAsync(cancellationToken);
        return solicitud;
    }

    private async Task ValidarVentanaRecepcionAsync(int entidadAcademicaId, int avisoOfertaId,
        CancellationToken cancellationToken)
    {
        var aviso = await (from vinculo in _db.AvisoOfertas.AsNoTracking()
                           join item in _db.Avisos.AsNoTracking() on vinculo.AvisoId equals item.Id
                           join oferta in _db.Ofertas.AsNoTracking() on vinculo.OfertaId equals oferta.Id
                           where vinculo.Id == avisoOfertaId && item.EntidadAcademicaId == entidadAcademicaId
                                 && item.Estado == "PUBLICADO" && vinculo.CerradoEn == null
                                 && oferta.Estado == "EN_PUBLICACION" && oferta.CerradaEn == null
                           select item.Id).SingleOrDefaultAsync(cancellationToken);
        if (aviso < 1) throw new InvalidOperationException("La vacante no está disponible en un Aviso publicado de esta Entidad Académica.");
        if (!await EstaEnHorarioRecepcionAsync(aviso, cancellationToken))
            throw new InvalidOperationException("La recepción de Solicitudes está cerrada en este momento.");
    }

    private async Task<bool> EstaEnHorarioRecepcionAsync(int avisoId, CancellationToken cancellationToken)
    {
        var local = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), ZonaInstitucional());
        var fecha = DateOnly.FromDateTime(local.DateTime);
        var hora = TimeOnly.FromDateTime(local.DateTime);
        return await _db.HorarioRecepcionRequisitos.AsNoTracking().AnyAsync(x => x.AvisoId == avisoId
            && x.Fecha == fecha && x.HoraInicio <= hora && hora < x.HoraFin, cancellationToken);
    }

    private async Task ValidarAmbitoAsync(int usuarioId, int entidadAcademicaId, CancellationToken cancellationToken)
    {
        var autorizado = await (from usuario in _db.Usuarios.AsNoTracking()
                                join perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                    on usuario.Id equals perfil.UsuarioId
                                join entidad in _db.EntidadAcademicas.AsNoTracking()
                                    on perfil.EntidadAcademicaId equals entidad.Id
                                where usuario.Id == usuarioId && usuario.RolId == RolEntidadAcademica
                                      && usuario.FechaEliminacion == null && entidad.Id == entidadAcademicaId
                                      && entidad.FechaEliminacion == null
                                select usuario.Id).AnyAsync(cancellationToken);
        if (!autorizado) throw new UnauthorizedAccessException("La cuenta no tiene una Entidad Académica vigente para esta operación.");
    }

    private async Task ValidarGradosAsync(IEnumerable<int> gradoIds, CancellationToken cancellationToken)
    {
        var ids = gradoIds.Distinct().ToArray();
        if (ids.Length == 0 || await _db.GradoAcademicos.AsNoTracking().CountAsync(x => ids.Contains(x.Id), cancellationToken) != ids.Length)
            throw new ArgumentException("Una o más formaciones usan un grado académico inexistente.");
    }

    private static TimeZoneInfo ZonaInstitucional()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"); }
    }

    private static DateOnly ObtenerFechaLocal(DateTimeOffset utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utc, ZonaInstitucional()).DateTime);
}
