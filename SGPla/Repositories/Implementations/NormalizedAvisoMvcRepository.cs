using System.Data;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations;

public sealed class NormalizedAvisoMvcRepository : IAvisoMvcRepository
{
    private const byte RolDgaa = 2;
    private const byte RolEntidadAcademica = 3;
    private readonly SgplaDbContext _db;
    private readonly TimeProvider _timeProvider;

    public NormalizedAvisoMvcRepository(SgplaDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<AvisoMvcPagina> BuscarAsync(int usuarioId, AvisoMvcFiltro filtro,
        CancellationToken cancellationToken = default)
    {
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: true, cancellationToken);
        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);

        var query =
            from aviso in _db.Avisos.AsNoTracking()
            join entidad in _db.EntidadAcademicas.AsNoTracking() on aviso.EntidadAcademicaId equals entidad.Id
            join periodo in _db.PeriodosEscolares.AsNoTracking() on aviso.PeriodoEscolarId equals periodo.Id
            join sistema in _db.SistemasEducativos.AsNoTracking() on aviso.SistemaEducativoId equals sistema.Id
            join articulo in _db.Articulos.AsNoTracking() on aviso.ArticuloId equals articulo.Id
            where entidad.FechaEliminacion == null
            select new
            {
                Aviso = aviso,
                Entidad = entidad.Nombre,
                entidad.AreaAcademicaId,
                Periodo = periodo.Clave,
                Sistema = sistema.Nombre,
                Articulo = articulo.Numero,
                Ofertas = _db.AvisoOfertas.AsNoTracking().Count(x => x.AvisoId == aviso.Id)
            };

        query = scope.RolId == RolEntidadAcademica
            ? query.Where(x => x.Aviso.EntidadAcademicaId == scope.EntidadAcademicaId)
            : query.Where(x => x.AreaAcademicaId == scope.AreaAcademicaId);

        if (filtro.EntidadAcademicaId.HasValue)
        {
            var entity = await _db.EntidadAcademicas.AsNoTracking()
                .Where(x => x.Id == filtro.EntidadAcademicaId.Value && x.FechaEliminacion == null)
                .Select(x => new { x.Id, x.AreaAcademicaId }).SingleOrDefaultAsync(cancellationToken);
            if (entity is null || (scope.RolId == RolEntidadAcademica
                    ? entity.Id != scope.EntidadAcademicaId
                    : entity.AreaAcademicaId != scope.AreaAcademicaId))
                throw new UnauthorizedAccessException("La Entidad Académica no pertenece al ámbito autorizado.");
            query = query.Where(x => x.Aviso.EntidadAcademicaId == entity.Id);
        }

        if (filtro.PeriodoEscolarId is > 0)
            query = query.Where(x => x.Aviso.PeriodoEscolarId == filtro.PeriodoEscolarId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(x => x.Aviso.Estado == filtro.Estado.Trim().ToUpperInvariant());

        var periodos = await _db.PeriodosEscolares.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderByDescending(x => x.Clave).Select(x => new AvisoMvcOpcion(x.Id, x.Clave))
            .Take(100).ToListAsync(cancellationToken);
        var entidadesQuery = _db.EntidadAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null);
        entidadesQuery = scope.RolId == RolEntidadAcademica
            ? entidadesQuery.Where(x => x.Id == scope.EntidadAcademicaId)
            : entidadesQuery.Where(x => x.AreaAcademicaId == scope.AreaAcademicaId);
        var entidades = await entidadesQuery.OrderBy(x => x.Nombre)
            .Select(x => new AvisoMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken);

        var total = await query.CountAsync(cancellationToken);
        var ultimaPagina = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        pagina = Math.Min(pagina, ultimaPagina);
        var rows = await query.OrderByDescending(x => x.Aviso.CreadoEn).ThenByDescending(x => x.Aviso.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new AvisoMvcItem(x.Aviso.Id, x.Entidad, x.Periodo, x.Sistema,
                x.Articulo, x.Aviso.TipoComunicado, x.Aviso.Estado, x.Aviso.CreadoEn, x.Ofertas))
            .ToListAsync(cancellationToken);

        return new AvisoMvcPagina(rows, total, pagina, tamano, periodos, entidades);
    }

    public async Task<AvisoMvcNuevoDatos> ObtenerDatosNuevoAsync(int usuarioId, int? periodoEscolarId,
        int? sistemaEducativoId, CancellationToken cancellationToken = default)
    {
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: false, cancellationToken);
        var entidadId = scope.EntidadAcademicaId!.Value;

        var periodos = await (from periodo in _db.PeriodosEscolares.AsNoTracking()
                              where periodo.FechaEliminacion == null
                              orderby periodo.Clave descending
                              select new AvisoMvcOpcion(periodo.Id, periodo.Clave))
            .Take(100).ToListAsync(cancellationToken);

        var sistemas = await (from programa in _db.ProgramasEducativos.AsNoTracking()
                              join sistema in _db.SistemasEducativos.AsNoTracking()
                                  on programa.SistemaEducativoId equals sistema.Id
                              where programa.EntidadAcademicaId == entidadId
                                    && programa.FechaEliminacion == null && sistema.FechaEliminacion == null
                              orderby sistema.Nombre
                              select new AvisoMvcOpcion(sistema.Id, sistema.Nombre))
            .Distinct().ToListAsync(cancellationToken);

        var articulos = await _db.Articulos.AsNoTracking().OrderBy(x => x.Numero)
            .Select(x => new AvisoMvcOpcion(x.Id, x.Numero))
            .ToListAsync(cancellationToken);
        var modalidades = await _db.ModalidadesRecepcion.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new AvisoMvcOpcion(x.Id, x.Nombre)).ToListAsync(cancellationToken);

        var ofertas = new List<AvisoMvcOpcion>();
        if (periodoEscolarId is > 0 && sistemaEducativoId is > 0)
        {
            ofertas = await (from oferta in ConsultaOfertasDisponibles(entidadId, periodoEscolarId.Value,
                                    sistemaEducativoId.Value, tracking: false)
                             join programacion in _db.ProgramacionAcademicas.AsNoTracking()
                                 on oferta.ProgramacionAcademicaId equals programacion.Id
                             orderby oferta.ClavePlaza, programacion.Nrc
                             select new AvisoMvcOpcion(oferta.Id,
                                 oferta.ClavePlaza + " / NRC " + programacion.Nrc))
                .Take(500).ToListAsync(cancellationToken);
        }

        return new AvisoMvcNuevoDatos(periodos, sistemas, articulos, ofertas, modalidades);
    }

    public async Task<AvisoMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int avisoId,
        CancellationToken cancellationToken = default)
    {
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: true, cancellationToken);
        var query =
            from aviso in _db.Avisos.AsNoTracking()
            join entidad in _db.EntidadAcademicas.AsNoTracking() on aviso.EntidadAcademicaId equals entidad.Id
            join periodo in _db.PeriodosEscolares.AsNoTracking() on aviso.PeriodoEscolarId equals periodo.Id
            join sistema in _db.SistemasEducativos.AsNoTracking() on aviso.SistemaEducativoId equals sistema.Id
            join articulo in _db.Articulos.AsNoTracking() on aviso.ArticuloId equals articulo.Id
            where aviso.Id == avisoId && entidad.FechaEliminacion == null
            select new { Aviso = aviso, Entidad = entidad.Nombre, entidad.AreaAcademicaId,
                Periodo = periodo.Clave, Sistema = sistema.Nombre, Articulo = articulo.Numero };
        query = scope.RolId == RolEntidadAcademica
            ? query.Where(x => x.Aviso.EntidadAcademicaId == scope.EntidadAcademicaId)
            : query.Where(x => x.AreaAcademicaId == scope.AreaAcademicaId);

        var avisoData = await query.Select(x => new
        {
            x.Aviso.Id, x.Aviso.PeriodoEscolarId, x.Aviso.SistemaEducativoId,
            x.Entidad, x.Periodo, x.Sistema, x.Articulo,
            x.Aviso.TipoComunicado, x.Aviso.Estado, x.Aviso.CreadoEn,
            x.Aviso.ModalidadRecepcionId, x.Aviso.Requisitos, x.Aviso.LugarRecepcion,
            ModalidadRecepcion = _db.ModalidadesRecepcion.AsNoTracking()
                .Where(m => m.Id == x.Aviso.ModalidadRecepcionId).Select(m => m.Nombre).SingleOrDefault(),
            x.Aviso.CorreoContacto, x.Aviso.NombreTitular, x.Aviso.FechaConsejoTecnico, x.Aviso.FechaVacantes
        }).SingleOrDefaultAsync(cancellationToken);
        if (avisoData is null) return null;

        var ofertas = await (from link in _db.AvisoOfertas.AsNoTracking()
                             join oferta in _db.Ofertas.AsNoTracking() on link.OfertaId equals oferta.Id
                             join programacion in _db.ProgramacionAcademicas.AsNoTracking()
                                 on oferta.ProgramacionAcademicaId equals programacion.Id
                             join ee in _db.ExperienciasEducativas.AsNoTracking()
                                 on programacion.ExperienciaEducativaId equals ee.Id
                             join plan in _db.PlanesEstudios.AsNoTracking() on ee.PlanEstudiosId equals plan.Id
                             join programa in _db.ProgramasEducativos.AsNoTracking()
                                 on plan.ProgramaEducativoId equals programa.Id
                             where link.AvisoId == avisoId
                             orderby oferta.ClavePlaza, programacion.Nrc
                             select new AvisoMvcOfertaItem(link.Id, oferta.Id, oferta.ClavePlaza,
                                 programacion.Nrc, programa.Nombre, ee.Nombre, oferta.Estado))
            .ToListAsync(cancellationToken);

        var documentos = await _db.DocumentoAvisos.AsNoTracking()
            .Where(x => x.AvisoId == avisoId)
            .OrderBy(x => x.Tipo).ThenByDescending(x => x.NumeroVersion)
            .Select(x => new AvisoMvcDocumentoItem(x.Id, x.Tipo, x.Nombre, x.Mime,
                x.Tamano, x.NumeroVersion, x.EsVigente, x.CargadoEn))
            .ToListAsync(cancellationToken);

        var horarios = await _db.HorarioRecepcionRequisitos.AsNoTracking()
            .Where(x => x.AvisoId == avisoId).OrderBy(x => x.Fecha).ThenBy(x => x.HoraInicio)
            .Select(x => new AvisoMvcHorario(x.Fecha, x.HoraInicio, x.HoraFin))
            .ToListAsync(cancellationToken);

        var revisiones = await _db.RevisionAvisos.AsNoTracking()
            .Where(x => x.AvisoId == avisoId).OrderByDescending(x => x.NumeroRevision)
            .Select(x => new AvisoMvcRevisionItem(x.NumeroRevision, x.EnviadoEn, x.Resultado,
                x.ResueltoEn, x.Comentarios)).ToListAsync(cancellationToken);

        return new AvisoMvcDetalle(avisoData.Id, avisoData.PeriodoEscolarId, avisoData.SistemaEducativoId,
            avisoData.Entidad, avisoData.Periodo,
            avisoData.Sistema, avisoData.Articulo, avisoData.TipoComunicado, avisoData.Estado,
            avisoData.CreadoEn, ofertas, documentos, avisoData.ModalidadRecepcionId,
            avisoData.ModalidadRecepcion, avisoData.Requisitos, avisoData.LugarRecepcion, avisoData.CorreoContacto,
            avisoData.NombreTitular, avisoData.FechaConsejoTecnico, avisoData.FechaVacantes,
            horarios, revisiones);
    }

    public async Task<int> CrearBorradorAsync(int usuarioId, int entidadAcademicaId,
        CrearAvisoMvcDatos datos, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: false, cancellationToken);
        if (scope.EntidadAcademicaId != entidadAcademicaId)
            throw new UnauthorizedAccessException("La Entidad Académica de la sesión no coincide con el ámbito vigente.");

        var periodoVigente = await _db.PeriodosEscolares.AsNoTracking()
            .AnyAsync(x => x.Id == datos.PeriodoEscolarId && x.FechaEliminacion == null, cancellationToken);
        var articuloVigente = await _db.Articulos.AsNoTracking()
            .AnyAsync(x => x.Id == datos.ArticuloId, cancellationToken);
        var sistemaVigente = await (from sistema in _db.SistemasEducativos.AsNoTracking()
                                    join programa in _db.ProgramasEducativos.AsNoTracking()
                                        on sistema.Id equals programa.SistemaEducativoId
                                    where sistema.Id == datos.SistemaEducativoId
                                          && sistema.FechaEliminacion == null
                                          && programa.EntidadAcademicaId == entidadAcademicaId
                                          && programa.FechaEliminacion == null
                                    select sistema.Id).AnyAsync(cancellationToken);
        if (!periodoVigente || !articuloVigente || !sistemaVigente)
            throw new ArgumentException("Uno de los catálogos seleccionados está inactivo o fuera de tu ámbito.");

        var ids = datos.OfertaIds.Distinct().ToArray();
        var idsDisponibles = await ConsultaOfertasDisponibles(entidadAcademicaId, datos.PeriodoEscolarId,
                datos.SistemaEducativoId, tracking: false)
            .Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (idsDisponibles.Count != ids.Length)
            throw new ArgumentException("Una o más Ofertas no están vigentes o no pertenecen al periodo, sistema y Entidad Académica.");
        var ofertas = await _db.Ofertas.AsTracking()
            .Where(x => ids.Contains(x.Id) && x.Estado == "DISPONIBLE" && x.CerradaEn == null)
            .ToListAsync(cancellationToken);
        if (ofertas.Count != ids.Length)
            throw new ArgumentException("Una o más Ofertas dejaron de estar disponibles.");

        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        var aviso = new Aviso
        {
            EntidadAcademicaId = entidadAcademicaId,
            PeriodoEscolarId = datos.PeriodoEscolarId,
            SistemaEducativoId = datos.SistemaEducativoId,
            ArticuloId = datos.ArticuloId,
            TipoComunicado = datos.TipoComunicado,
            Estado = "CREADO",
            CreadoEn = ahora
        };
        _db.Entry(aviso).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);

        var asociaciones = ofertas.Select(oferta => new AvisoOferta
        {
            AvisoId = aviso.Id,
            OfertaId = oferta.Id,
            IncorporadoEn = ahora
        }).ToList();
        foreach (var oferta in ofertas) oferta.Estado = "EN_PUBLICACION";
        foreach (var asociacion in asociaciones) _db.Entry(asociacion).State = EntityState.Added;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return aviso.Id;
    }

    public async Task AgregarOfertasAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        IReadOnlyList<int> ofertaIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: false, cancellationToken);
        if (scope.EntidadAcademicaId != entidadAcademicaId)
            throw new UnauthorizedAccessException("La Entidad Académica de la sesión no coincide con el ámbito vigente.");

        var aviso = await _db.Avisos.AsTracking().SingleOrDefaultAsync(x => x.Id == avisoId
            && x.EntidadAcademicaId == entidadAcademicaId, cancellationToken);
        if (aviso is null) throw new UnauthorizedAccessException("El Aviso no pertenece a tu Entidad Académica.");
        if (aviso.Estado is not ("CREADO" or "DEVUELTO_DGAA"))
            throw new InvalidOperationException("Sólo se pueden agregar Ofertas a Avisos creados o devueltos.");

        var ids = ofertaIds.Distinct().ToArray();
        var disponibles = await ConsultaOfertasDisponibles(entidadAcademicaId, aviso.PeriodoEscolarId,
                aviso.SistemaEducativoId, tracking: false)
            .Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (disponibles.Count != ids.Length)
            throw new ArgumentException("Una o más Ofertas no están disponibles para este Aviso.");

        var ofertas = await _db.Ofertas.AsTracking()
            .Where(x => ids.Contains(x.Id) && x.Estado == "DISPONIBLE" && x.CerradaEn == null)
            .ToListAsync(cancellationToken);
        if (ofertas.Count != ids.Length)
            throw new ArgumentException("Una o más Ofertas dejaron de estar disponibles.");

        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        foreach (var oferta in ofertas)
        {
            oferta.Estado = "EN_PUBLICACION";
            _db.Entry(new AvisoOferta
            {
                AvisoId = aviso.Id,
                OfertaId = oferta.Id,
                IncorporadoEn = ahora
            }).State = EntityState.Added;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RetirarOfertaAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        int avisoOfertaId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: false, cancellationToken);
        if (scope.EntidadAcademicaId != entidadAcademicaId)
            throw new UnauthorizedAccessException("La Entidad Académica de la sesión no coincide con el ámbito vigente.");

        var aviso = await _db.Avisos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == avisoId
            && x.EntidadAcademicaId == entidadAcademicaId, cancellationToken);
        if (aviso is null) throw new UnauthorizedAccessException("El Aviso no pertenece a tu Entidad Académica.");
        if (aviso.Estado is not ("CREADO" or "DEVUELTO_DGAA"))
            throw new InvalidOperationException("Sólo se pueden retirar Ofertas de Avisos creados o devueltos.");

        var link = await _db.AvisoOfertas.AsTracking().SingleOrDefaultAsync(x => x.Id == avisoOfertaId
            && x.AvisoId == avisoId && x.CerradoEn == null, cancellationToken);
        if (link is null) throw new KeyNotFoundException("La Oferta no está asociada al Aviso.");
        if (await _db.Solicitudes.AsNoTracking().AnyAsync(x => x.AvisoOfertaId == link.Id, cancellationToken)
            || await _db.ActaOfertas.AsNoTracking().AnyAsync(x => x.AvisoOfertaId == link.Id, cancellationToken))
            throw new InvalidOperationException("No se puede retirar una Oferta que ya tiene Solicitudes o Acta.");

        var oferta = await _db.Ofertas.AsTracking().SingleOrDefaultAsync(x => x.Id == link.OfertaId, cancellationToken)
            ?? throw new InvalidOperationException("La Oferta asociada ya no existe.");
        if (oferta.Estado != "EN_PUBLICACION")
            throw new InvalidOperationException("La Oferta no está en estado EN_PUBLICACION.");
        _db.Entry(link).State = EntityState.Deleted;
        oferta.Estado = "DISPONIBLE";
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task EnviarARevisionAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        ConfigurarYEnviarAvisoMvcDatos datos, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: false, cancellationToken);
        if (scope.EntidadAcademicaId != entidadAcademicaId)
            throw new UnauthorizedAccessException("La Entidad Académica de la sesión no coincide con el ámbito vigente.");

        var aviso = await _db.Avisos.AsTracking().SingleOrDefaultAsync(x => x.Id == avisoId
            && x.EntidadAcademicaId == entidadAcademicaId, cancellationToken);
        if (aviso is null) throw new UnauthorizedAccessException("El Aviso no pertenece a tu Entidad Académica.");
        if (aviso.Estado is not ("CREADO" or "DEVUELTO_DGAA"))
            throw new InvalidOperationException("Sólo se pueden enviar Avisos creados o devueltos.");

        var modalidad = await _db.ModalidadesRecepcion.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == datos.ModalidadRecepcionId, cancellationToken);
        if (modalidad is null)
            throw new ArgumentException("La modalidad de recepción seleccionada no existe.");
        var lugar = string.IsNullOrWhiteSpace(datos.LugarRecepcion) ? null : datos.LugarRecepcion.Trim();
        if (modalidad.RequiereLugar && lugar is null)
            throw new ArgumentException("La modalidad seleccionada requiere un lugar de recepción.");
        if (!modalidad.RequiereLugar && lugar is not null)
            throw new ArgumentException("Esta modalidad no admite un lugar de recepción.");
        var correo = datos.CorreoContacto.Trim();
        if (!System.Net.Mail.MailAddress.TryCreate(correo, out var address)
            || !string.Equals(address.Address, correo, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El correo de contacto no tiene un formato válido.");
        if (datos.FechaConsejoTecnico > datos.FechaVacantes
            || datos.Horarios.Any(x => x.Fecha > datos.FechaConsejoTecnico))
            throw new ArgumentException("La recepción debe ocurrir antes o el día del Consejo Técnico, y éste antes o el día de las vacantes.");

        var horarios = datos.Horarios.OrderBy(x => x.Fecha).ThenBy(x => x.HoraInicio).ToArray();
        for (var index = 1; index < horarios.Length; index++)
        {
            if (horarios[index].Fecha == horarios[index - 1].Fecha
                && horarios[index].HoraInicio < horarios[index - 1].HoraFin)
                throw new ArgumentException("Los horarios de recepción no pueden traslaparse.");
        }

        if (!await _db.AvisoOfertas.AsNoTracking().AnyAsync(x => x.AvisoId == avisoId && x.CerradoEn == null, cancellationToken))
            throw new InvalidOperationException("El Aviso debe contener al menos una Oferta abierta.");
        var original = await _db.DocumentoAvisos.AsNoTracking().Where(x => x.AvisoId == avisoId
                && x.Tipo == "ORIGINAL" && x.EsVigente)
            .Select(x => new { x.Id, x.Nombre }).SingleOrDefaultAsync(cancellationToken);
        if (original is null)
            throw new InvalidOperationException("Carga una versión vigente del documento original antes de enviar.");

        var existentes = await _db.HorarioRecepcionRequisitos.AsTracking()
            .Where(x => x.AvisoId == avisoId).ToListAsync(cancellationToken);
        _db.HorarioRecepcionRequisitos.RemoveRange(existentes);
        foreach (var horario in horarios)
            _db.Entry(new HorarioRecepcionRequisito
            {
                AvisoId = avisoId, Fecha = horario.Fecha,
                HoraInicio = horario.HoraInicio, HoraFin = horario.HoraFin
            }).State = EntityState.Added;

        var ultimaRevision = await _db.RevisionAvisos.AsNoTracking()
            .Where(x => x.AvisoId == avisoId).Select(x => (int?)x.NumeroRevision)
            .MaxAsync(cancellationToken) ?? 0;
        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        aviso.ModalidadRecepcionId = modalidad.Id;
        aviso.Requisitos = datos.Requisitos.Trim();
        aviso.LugarRecepcion = lugar;
        aviso.CorreoContacto = correo;
        aviso.NombreTitular = datos.NombreTitular.Trim();
        aviso.FechaConsejoTecnico = datos.FechaConsejoTecnico;
        aviso.FechaVacantes = datos.FechaVacantes;
        aviso.Estado = "EN_REVISION_DGAA";
        _db.Entry(new RevisionAviso
        {
            AvisoId = avisoId, NumeroRevision = checked(ultimaRevision + 1),
            DocumentoOriginalId = original.Id, EnviadoPorUsuarioId = usuarioId, EnviadoEn = ahora
        }).State = EntityState.Added;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ResolverRevisionAsync(int usuarioId, int avisoId, bool avalar, string? comentarios,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var scope = await ObtenerAmbitoAsync(usuarioId, permitirDgaa: true, cancellationToken);
        if (scope.RolId != RolDgaa)
            throw new UnauthorizedAccessException("Sólo DGAA puede resolver la revisión de un Aviso.");

        var perteneceAlAmbito = await (from item in _db.Avisos.AsNoTracking()
                                        join entidad in _db.EntidadAcademicas.AsNoTracking()
                                            on item.EntidadAcademicaId equals entidad.Id
                                        where item.Id == avisoId && entidad.FechaEliminacion == null
                                              && entidad.AreaAcademicaId == scope.AreaAcademicaId
                                        select item.Id).AnyAsync(cancellationToken);
        if (!perteneceAlAmbito) throw new UnauthorizedAccessException("El Aviso está fuera del ámbito DGAA.");
        var aviso = await _db.Avisos.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == avisoId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El Aviso ya no está vigente.");
        if (aviso.Estado != "EN_REVISION_DGAA")
            throw new InvalidOperationException("El Aviso no se encuentra en revisión de DGAA.");

        var revision = await _db.RevisionAvisos.AsTracking().SingleOrDefaultAsync(x => x.AvisoId == avisoId
            && x.ResueltoEn == null, cancellationToken);
        if (revision is null) throw new InvalidOperationException("No hay una revisión abierta para resolver.");

        var ahora = _timeProvider.GetUtcNow().UtcDateTime;
        revision.ResueltoPorUsuarioId = usuarioId;
        revision.ResueltoEn = ahora;
        revision.Resultado = avalar ? "AVALADA" : "DEVUELTA";
        revision.Comentarios = comentarios;
        aviso.Estado = avalar ? "AVALADO_DGAA" : "DEVUELTO_DGAA";
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<Oferta> ConsultaOfertasDisponibles(int entidadAcademicaId, int periodoEscolarId,
        int sistemaEducativoId, bool tracking)
    {
        var ofertas = tracking ? _db.Ofertas.AsTracking() : _db.Ofertas.AsNoTracking();
        return
            from oferta in ofertas
            join programacion in _db.ProgramacionAcademicas.AsNoTracking()
                on oferta.ProgramacionAcademicaId equals programacion.Id
            join periodo in _db.PeriodosEscolares.AsNoTracking()
                on programacion.PeriodoEscolarId equals periodo.Id
            join ee in _db.ExperienciasEducativas.AsNoTracking()
                on programacion.ExperienciaEducativaId equals ee.Id
            join plan in _db.PlanesEstudios.AsNoTracking() on ee.PlanEstudiosId equals plan.Id
            join programa in _db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
            join entidad in _db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
            where oferta.Estado == "DISPONIBLE" && oferta.CerradaEn == null
                  && programacion.FechaEliminacion == null && periodo.FechaEliminacion == null
                  && ee.FechaEliminacion == null && plan.FechaEliminacion == null
                  && programa.FechaEliminacion == null && entidad.FechaEliminacion == null
                  && entidad.Id == entidadAcademicaId && periodo.Id == periodoEscolarId
                  && programa.SistemaEducativoId == sistemaEducativoId
                  && !_db.AvisoOfertas.AsNoTracking().Any(link => link.OfertaId == oferta.Id && link.CerradoEn == null)
            select oferta;
    }

    private async Task<UsuarioAmbito> ObtenerAmbitoAsync(int usuarioId, bool permitirDgaa,
        CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.AsNoTracking().Where(x => x.Id == usuarioId && x.FechaEliminacion == null)
            .Select(x => new { x.Id, x.RolId }).SingleOrDefaultAsync(cancellationToken);
        if (usuario is null) throw new UnauthorizedAccessException("La cuenta no está vigente.");

        if (usuario.RolId == RolEntidadAcademica)
        {
            var entityId = await (from perfil in _db.UsuariosEntidadAcademica.AsNoTracking()
                                  join entidad in _db.EntidadAcademicas.AsNoTracking()
                                      on perfil.EntidadAcademicaId equals entidad.Id
                                  where perfil.UsuarioId == usuarioId && entidad.FechaEliminacion == null
                                  select entidad.Id).SingleOrDefaultAsync(cancellationToken);
            if (entityId < 1) throw new UnauthorizedAccessException("La cuenta no tiene una Entidad Académica vigente.");
            return new UsuarioAmbito(usuario.RolId, entityId, null);
        }

        if (permitirDgaa && usuario.RolId == RolDgaa)
        {
            var areaId = await (from perfil in _db.UsuariosDgaa.AsNoTracking()
                                join area in _db.AreaAcademicas.AsNoTracking() on perfil.AreaAcademicaId equals area.Id
                                where perfil.UsuarioId == usuarioId && area.FechaEliminacion == null
                                select area.Id).SingleOrDefaultAsync(cancellationToken);
            if (areaId < 1) throw new UnauthorizedAccessException("La cuenta no tiene un Área Académica vigente.");
            return new UsuarioAmbito(usuario.RolId, null, areaId);
        }

        throw new UnauthorizedAccessException("El rol actual no tiene acceso a Avisos.");
    }

    private sealed record UsuarioAmbito(byte RolId, int? EntidadAcademicaId, int? AreaAcademicaId);
}
