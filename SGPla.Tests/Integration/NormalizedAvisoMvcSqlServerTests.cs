using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Avisos;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedAvisoMvcSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Crea_borrador_vincula_oferta_y_aplica_ambito_en_sql()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().Select(x => x.Id).FirstAsync();
        var sistemaId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var nivelId = await db.NivelesFormacion.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var areaFormacionId = await db.AreaFormaciones.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var tipoPlazaId = await db.TipoPlazas.AsNoTracking().Select(x => x.Id).FirstAsync();
        var tipoContratacionId = await db.TiposContratacion.AsNoTracking().Select(x => x.Id).FirstAsync();
        var rolId = await db.Roles.AsNoTracking().Where(x => x.Id == 3).Select(x => x.Id).SingleAsync();

        var periodo = new PeriodoEscolar
        {
            Clave = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString(),
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 12, 31)
        };
        var entidad = new EntidadAcademica
        {
            Clave = $"AV{token[..10]}", Nombre = $"Entidad aviso {token}", Calle = "Calle de prueba",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234570", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        var sistema = sistemaId;
        var usuario = new Usuario
        {
            Correo = $"aviso.{token[..12].ToLowerInvariant()}@uv.mx", Nombre = "Usuario integración", RolId = rolId
        };
        var usuarioDgaa = new Usuario
        {
            Correo = $"dgaa.{token[..12].ToLowerInvariant()}@uv.mx", Nombre = "DGAA integración", RolId = 2
        };
        var articulo = new Articulo { Numero = $"AV-{token[..16]}", Descripcion = "Aviso integration fixture" };
        var programa = new ProgramaEducativo
        {
            Nombre = $"Programa Aviso {token}", EntidadAcademicaId = 0,
            SistemaEducativoId = sistema, NivelFormacionId = nivelId
        };
        var plan = new PlanEstudios { Codigo = $"PL{token[..12]}", ProgramaEducativoId = 0 };
        var experiencia = new ExperienciaEducativa
        {
            Nombre = $"Experiencia Aviso {token}", MateriaEe = $"MAT{token[..10]}", CursoEe = "01",
            HorasTeoricas = 2, HorasPracticas = 0, Creditos = 2, AreaFormacionId = areaFormacionId,
            PlanEstudiosId = 0
        };
        var programacion = new ProgramacionAcademica
        {
            Nrc = $"NRC{token[..12]}", PeriodoEscolarId = 0, ExperienciaEducativaId = 0
        };
        var oferta = new Oferta
        {
            ClavePlaza = $"PLAZA{token[..10]}", TipoPlazaId = tipoPlazaId,
            TipoContratacionId = tipoContratacionId, PerfilSolicitado = "Perfil de integración",
            Estado = "DISPONIBLE"
        };
        var perfil = new UsuarioEntidadAcademica { UsuarioId = 0, EntidadAcademicaId = 0 };
        var perfilDgaa = new UsuarioDgaa { UsuarioId = 0, AreaAcademicaId = areaId };
        var avisoId = 0;
        var avisoCancelacionId = 0;
        var ofertaCancelacionId = 0;
        var documentoOriginalId = 0;
        var solicitudId = 0;
        var raizDocumentos = Path.Combine(Path.GetTempPath(), "sgpla-solicitud-docs-" + token.ToLowerInvariant());
        var configuracionDocumentos = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Documentos:Directorio"] = raizDocumentos
        }).Build();
        var almacenDocumentos = new AlmacenDocumentosLocal(configuracionDocumentos, new TestHostEnvironment());
        var clavesDocumento = new List<string>();
        try
        {
            db.Entry(periodo).State = EntityState.Added;
            await db.SaveChangesAsync();
            db.Entry(entidad).State = EntityState.Added;
            await db.SaveChangesAsync();
            db.Entry(articulo).State = EntityState.Added;
            await db.SaveChangesAsync();

            programa.EntidadAcademicaId = entidad.Id;
            db.Entry(programa).State = EntityState.Added;
            await db.SaveChangesAsync();
            plan.ProgramaEducativoId = programa.Id;
            db.Entry(plan).State = EntityState.Added;
            await db.SaveChangesAsync();
            experiencia.PlanEstudiosId = plan.Id;
            db.Entry(experiencia).State = EntityState.Added;
            await db.SaveChangesAsync();
            programacion.PeriodoEscolarId = periodo.Id;
            programacion.ExperienciaEducativaId = experiencia.Id;
            db.Entry(programacion).State = EntityState.Added;
            await db.SaveChangesAsync();
            oferta.ProgramacionAcademicaId = programacion.Id;
            db.Entry(oferta).State = EntityState.Added;
            await db.SaveChangesAsync();
            db.Entry(usuario).State = EntityState.Added;
            await db.SaveChangesAsync();
            perfil.UsuarioId = usuario.Id;
            perfil.EntidadAcademicaId = entidad.Id;
            db.Entry(perfil).State = EntityState.Added;
            await db.SaveChangesAsync();
            db.Entry(usuarioDgaa).State = EntityState.Added;
            await db.SaveChangesAsync();
            perfilDgaa.UsuarioId = usuarioDgaa.Id;
            db.Entry(perfilDgaa).State = EntityState.Added;
            await db.SaveChangesAsync();

            var service = new AvisoMvcService(new NormalizedAvisoMvcRepository(db, TimeProvider.System));
            var optionsForCreate = await service.ObtenerDatosNuevoAsync(usuario.Id, periodo.Id, sistemaId);
            Assert.Contains(optionsForCreate.Periodos, x => x.Id == periodo.Id);
            Assert.Contains(optionsForCreate.SistemasEducativos, x => x.Id == sistemaId);
            Assert.Contains(optionsForCreate.Articulos, x => x.Id == articulo.Id);
            Assert.Contains(optionsForCreate.OfertasDisponibles, x => x.Id == oferta.Id);

            avisoId = await service.CrearBorradorAsync(usuario.Id, entidad.Id,
                new CrearAvisoMvcDatos(periodo.Id, sistemaId, articulo.Id, "AVISO", [oferta.Id]));

            var aviso = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("CREADO", aviso.Estado);
            Assert.Equal("AVISO", aviso.TipoComunicado);
            Assert.Equal(entidad.Id, aviso.EntidadAcademicaId);
            Assert.Equal(1, await db.AvisoOfertas.AsNoTracking().CountAsync(x => x.AvisoId == avisoId && x.OfertaId == oferta.Id));
            Assert.Equal("EN_PUBLICACION", (await db.Ofertas.AsNoTracking().SingleAsync(x => x.Id == oferta.Id)).Estado);

            var detail = await service.ObtenerDetalleAsync(usuario.Id, avisoId);
            Assert.NotNull(detail);
            Assert.Equal(entidad.Nombre, detail.EntidadAcademica);
            var firstLink = await db.AvisoOfertas.AsNoTracking().SingleAsync(x => x.AvisoId == avisoId);
            await service.RetirarOfertaAsync(usuario.Id, entidad.Id, avisoId, firstLink.Id);
            Assert.False(await db.AvisoOfertas.AsNoTracking().AnyAsync(x => x.Id == firstLink.Id));
            Assert.Equal("DISPONIBLE", (await db.Ofertas.AsNoTracking().SingleAsync(x => x.Id == oferta.Id)).Estado);

            await service.AgregarOfertasAsync(usuario.Id, entidad.Id, avisoId, [oferta.Id]);
            var reincorporada = await db.AvisoOfertas.AsNoTracking()
                .SingleAsync(x => x.AvisoId == avisoId && x.OfertaId == oferta.Id);
            Assert.NotEqual(firstLink.Id, reincorporada.Id);
            Assert.Null(reincorporada.CerradoEn);
            Assert.Equal("EN_PUBLICACION", (await db.Ofertas.AsNoTracking().SingleAsync(x => x.Id == oferta.Id)).Estado);

            var modalidad = await db.ModalidadesRecepcion.AsNoTracking().FirstAsync();
            var documento = new DocumentoAviso
            {
                AvisoId = avisoId, Tipo = "ORIGINAL", Nombre = "aviso.pdf", Mime = "application/pdf",
                Tamano = 100, ChecksumSha256 = new byte[32], ClaveAlmacenamiento = $"tests/{token}/original.pdf",
                NumeroVersion = 1, EsVigente = true, CargadoPorUsuarioId = usuario.Id
            };
            db.Entry(documento).State = EntityState.Added;
            await db.SaveChangesAsync();
            documentoOriginalId = documento.Id;
            var fechaRecepcion = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2));
            var fechaConsejo = fechaRecepcion.AddDays(2);
            var fechaVacantes = fechaConsejo.AddDays(2);
            var datosEnvio = new ConfigurarYEnviarAvisoMvcDatos(modalidad.Id, "Entregar documentación en original",
                    modalidad.RequiereLugar ? "Oficina de atención" : null, "contacto@example.org", "Titular de prueba",
                    fechaConsejo, fechaVacantes,
                    [new AvisoMvcHorario(fechaRecepcion, new TimeOnly(9, 0), new TimeOnly(13, 0))]);
            await service.EnviarARevisionAsync(usuario.Id, entidad.Id, avisoId, datosEnvio);

            var enviado = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("EN_REVISION_DGAA", enviado.Estado);
            Assert.Equal("Entregar documentación en original", enviado.Requisitos);
            Assert.Equal(fechaConsejo, enviado.FechaConsejoTecnico);
            var revision = await db.RevisionAvisos.AsNoTracking().SingleAsync(x => x.AvisoId == avisoId);
            Assert.Equal(1, revision.NumeroRevision);
            Assert.Equal(documentoOriginalId, revision.DocumentoOriginalId);
            Assert.Null(revision.ResueltoEn);
            Assert.Single(await db.HorarioRecepcionRequisitos.AsNoTracking().Where(x => x.AvisoId == avisoId).ToListAsync());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ResolverRevisionAsync(
                usuario.Id, avisoId, avalar: true, comentarios: null));
            await service.ResolverRevisionAsync(usuarioDgaa.Id, avisoId, avalar: false,
                comentarios: "Corregir el horario de atención.");
            var devuelto = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("DEVUELTO_DGAA", devuelto.Estado);
            Assert.Equal("DEVUELTA", (await db.RevisionAvisos.AsNoTracking().SingleAsync(x => x.AvisoId == avisoId)).Resultado);

            await service.EnviarARevisionAsync(usuario.Id, entidad.Id, avisoId, datosEnvio);
            var revisiones = await db.RevisionAvisos.AsNoTracking().Where(x => x.AvisoId == avisoId)
                .OrderBy(x => x.NumeroRevision).ToListAsync();
            Assert.Equal(new[] { 1, 2 }, revisiones.Select(x => x.NumeroRevision));
            Assert.Equal("EN_REVISION_DGAA", (await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId)).Estado);

            await service.ResolverRevisionAsync(usuarioDgaa.Id, avisoId, avalar: true, comentarios: null);
            Assert.Equal("AVALADO_DGAA", (await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId)).Estado);
            var documentos = new NormalizedDocumentoAvisoMvcRepository(db, TimeProvider.System);
            var firmado = new DocumentoAvisoOriginalMvc(avisoId, entidad.Id, usuario.Id, "firmado.pdf",
                "application/pdf", 100, new byte[32], $"tests/{token}/firmado.pdf");
            await documentos.ValidarCargaFirmadoAsync(avisoId, entidad.Id, usuario.Id);
            var documentoFirmadoId = await documentos.GuardarFirmadoAsync(firmado);
            var firmadoAviso = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("FIRMADO", firmadoAviso.Estado);
            Assert.True(await db.DocumentoAvisos.AsNoTracking().AnyAsync(x => x.Id == documentoFirmadoId
                && x.Tipo == "FIRMADO" && x.EsVigente));

            var fechaPublicacion = fechaRecepcion.AddDays(-1);
            await service.PublicarAsync(usuario.Id, entidad.Id, avisoId,
                new PublicarAvisoMvcDatos(fechaPublicacion, "https://example.org/avisos/" + avisoId));
            var publicado = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("PUBLICADO", publicado.Estado);
            Assert.Equal(fechaPublicacion, publicado.FechaPublicacion);
            Assert.Equal("https://example.org/avisos/" + avisoId, publicado.UrlPublicacion);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PublicarAsync(usuario.Id, entidad.Id,
                avisoId, new PublicarAvisoMvcDatos(fechaPublicacion, "https://example.org/duplicate")));

            var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            var fechaHoraRecepcion = TimeZoneInfo.ConvertTimeToUtc(fechaRecepcion.ToDateTime(new TimeOnly(10, 0)), zona);
            var solicitudes = new SolicitudMvcService(new NormalizedSolicitudMvcRepository(db,
                new FixedTimeProvider(new DateTimeOffset(fechaHoraRecepcion, TimeSpan.Zero))), almacenDocumentos);
            var gradoId = await db.GradoAcademicos.AsNoTracking().Select(x => x.Id).FirstAsync();
            solicitudId = await solicitudes.RegistrarAsync(usuario.Id, entidad.Id, new RegistrarSolicitudMvcDatos(
                reincorporada.Id, "Aspirante integración", $"aspirante.{token[..12].ToLowerInvariant()}@example.org",
                "Docente", "Perfil inicial", [new FormacionSolicitudMvcDatos(gradoId, "Doctorado")], "Primera solicitud"));
            var detalleSolicitud = await solicitudes.ObtenerDetalleAsync(usuario.Id, entidad.Id, avisoId, solicitudId);
            Assert.NotNull(detalleSolicitud);
            Assert.Equal("REGISTRADA", detalleSolicitud.Estado);
            Assert.Equal("Doctorado", Assert.Single(detalleSolicitud.Formaciones).Descripcion);
            var tipoDocumentoId = await db.TipoDocumentoAspirantes.AsNoTracking().Select(x => x.Id).FirstAsync();
            var pdfSolicitud = "%PDF-1.7\nsolicitud test"u8.ToArray();
            await using (var archivo = new MemoryStream(pdfSolicitud))
                await solicitudes.AgregarDocumentoAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                    tipoDocumentoId, null, archivo, "curriculum.pdf", "application/pdf", pdfSolicitud.Length);
            var detalleConDocumento = await solicitudes.ObtenerDetalleAsync(usuario.Id, entidad.Id, avisoId, solicitudId);
            var documentoAnterior = Assert.Single(detalleConDocumento!.Documentos);
            var pdfSolicitudV2 = "%PDF-1.7\nsolicitud v2"u8.ToArray();
            await using (var archivo = new MemoryStream(pdfSolicitudV2))
                await solicitudes.AgregarDocumentoAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                    tipoDocumentoId, documentoAnterior.DocumentoAspiranteId, archivo, "curriculum-v2.pdf",
                    "application/pdf", pdfSolicitudV2.Length);
            var versionesSolicitud = (await solicitudes.ObtenerDetalleAsync(usuario.Id, entidad.Id, avisoId, solicitudId))!
                .Documentos.OrderBy(x => x.NumeroVersion).ToList();
            Assert.Equal(new[] { 1, 2 }, versionesSolicitud.Select(x => x.NumeroVersion));
            Assert.False(versionesSolicitud[0].EsVigente);
            Assert.True(versionesSolicitud[1].EsVigente);
            var documentoSolicitud = versionesSolicitud[1];
            var descargaAnteriorSolicitud = await solicitudes.AbrirDocumentoAsync(usuario.Id, entidad.Id, avisoId,
                solicitudId, versionesSolicitud[0].VersionId);
            Assert.NotNull(descargaAnteriorSolicitud);
            await descargaAnteriorSolicitud!.Contenido.DisposeAsync();
            Assert.True(await db.SolicitudDocumentos.AsNoTracking().AnyAsync(x => x.SolicitudId == solicitudId
                && x.VersionDocumentoAspiranteId == documentoSolicitud.VersionId));
            var descargaSolicitud = await solicitudes.AbrirDocumentoAsync(usuario.Id, entidad.Id, avisoId,
                solicitudId, documentoSolicitud.VersionId);
            Assert.NotNull(descargaSolicitud);
            using (var contenido = new MemoryStream())
            {
                await descargaSolicitud!.Contenido.CopyToAsync(contenido);
                Assert.Equal(pdfSolicitudV2, contenido.ToArray());
            }
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => solicitudes.AbrirDocumentoAsync(
                usuario.Id, entidad.Id + 1000, avisoId, solicitudId, documentoSolicitud.VersionId));
            clavesDocumento.AddRange(await db.VersionDocumentoAspirantes.AsNoTracking()
                .Where(x => versionesSolicitud.Select(v => v.VersionId).Contains(x.Id))
                .Select(x => x.ClaveAlmacenamiento).ToListAsync());
            Assert.Null(await solicitudes.AbrirDocumentoAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                documentoSolicitud.VersionId + 500000));
            var claveVersionHistorica = await db.VersionDocumentoAspirantes.AsNoTracking()
                .Where(x => x.Id == versionesSolicitud[0].VersionId).Select(x => x.ClaveAlmacenamiento).SingleAsync();
            await almacenDocumentos.EliminarAsync(claveVersionHistorica);
            await Assert.ThrowsAsync<FileNotFoundException>(() => solicitudes.AbrirDocumentoAsync(usuario.Id,
                entidad.Id, avisoId, solicitudId, versionesSolicitud[0].VersionId));
            await Assert.ThrowsAsync<InvalidOperationException>(() => solicitudes.RegistrarAsync(usuario.Id, entidad.Id,
                new RegistrarSolicitudMvcDatos(reincorporada.Id, "Aspirante duplicado",
                    $"aspirante.{token[..12].ToLowerInvariant()}@example.org", null, "Perfil duplicado",
                    [new FormacionSolicitudMvcDatos(gradoId, "Doctorado")], null)));
            await solicitudes.ActualizarPerfilAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                new EditarPerfilSolicitudMvcDatos("Aspirante actualizado", $"actualizado.{token[..12].ToLowerInvariant()}@example.org",
                    "Investigadora", "Perfil v2", [new FormacionSolicitudMvcDatos(gradoId, "Doctorado actualizado")]));
            var sufijoCorreo = token[..12].ToLowerInvariant();
            var prefijoCorreoOriginal = "aspirante." + sufijoCorreo;
            var prefijoCorreoActualizado = "actualizado." + sufijoCorreo;
            var versionesPerfil = await db.PerfilAspirantes.AsNoTracking()
                .Where(x => x.Correo.StartsWith(prefijoCorreoOriginal)
                    || x.Correo.StartsWith(prefijoCorreoActualizado))
                .OrderBy(x => x.NumeroVersion).ToListAsync();
            Assert.Equal(new[] { 1, 2 }, versionesPerfil.Select(x => x.NumeroVersion));
            Assert.False(versionesPerfil[0].EsVigente);
            Assert.True(versionesPerfil[1].EsVigente);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => solicitudes.ListarPorAvisoAsync(
                usuario.Id, entidad.Id + 1000, avisoId));
            await solicitudes.ResolverAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                new ResolverSolicitudMvcDatos(true, null));
            await Assert.ThrowsAsync<InvalidOperationException>(() => solicitudes.ActualizarPerfilAsync(
                usuario.Id, entidad.Id, avisoId, solicitudId,
                new EditarPerfilSolicitudMvcDatos("No procede", $"otro.{token[..12]}@example.org", null,
                    "No procede", [new FormacionSolicitudMvcDatos(gradoId, "Doctorado")])));
            var pdfBloqueado = "%PDF-1.7\nimmutable"u8.ToArray();
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await using var archivo = new MemoryStream(pdfBloqueado);
                await solicitudes.AgregarDocumentoAsync(usuario.Id, entidad.Id, avisoId, solicitudId,
                    tipoDocumentoId, null, archivo, "tardio.pdf", "application/pdf", pdfBloqueado.Length);
            });
            await solicitudes.RetirarAsync(usuario.Id, entidad.Id, avisoId, solicitudId, "Se retiró antes de sesión.");
            Assert.Equal("RETIRADA", (await db.Solicitudes.AsNoTracking().SingleAsync(x => x.Id == solicitudId)).Estado);

            var ofertaCancelacion = new Oferta
            {
                ProgramacionAcademicaId = programacion.Id, ClavePlaza = $"CANCELA{token[..8]}",
                TipoPlazaId = tipoPlazaId, TipoContratacionId = tipoContratacionId,
                PerfilSolicitado = "Perfil para cancelación", Estado = "DISPONIBLE"
            };
            db.Entry(ofertaCancelacion).State = EntityState.Added;
            await db.SaveChangesAsync();
            ofertaCancelacionId = ofertaCancelacion.Id;
            avisoCancelacionId = await service.CrearBorradorAsync(usuario.Id, entidad.Id,
                new CrearAvisoMvcDatos(periodo.Id, sistemaId, articulo.Id, "AVISO", [ofertaCancelacion.Id]));
            var originalCancelacion = new DocumentoAviso
            {
                AvisoId = avisoCancelacionId, Tipo = "ORIGINAL", Nombre = "aviso-cancelar.pdf", Mime = "application/pdf",
                Tamano = 100, ChecksumSha256 = new byte[32], ClaveAlmacenamiento = $"tests/{token}/cancelar-original.pdf",
                NumeroVersion = 1, EsVigente = true, CargadoPorUsuarioId = usuario.Id
            };
            db.Entry(originalCancelacion).State = EntityState.Added;
            await db.SaveChangesAsync();
            await service.EnviarARevisionAsync(usuario.Id, entidad.Id, avisoCancelacionId, datosEnvio);
            await service.ResolverRevisionAsync(usuarioDgaa.Id, avisoCancelacionId, avalar: true, comentarios: null);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CancelarAsync(usuario.Id,
                avisoCancelacionId, new CancelarAvisoMvcDatos("Cancelación fuera de rol")));
            var revisionAbierta = new RevisionAviso
            {
                AvisoId = avisoCancelacionId, NumeroRevision = 2, DocumentoOriginalId = originalCancelacion.Id,
                EnviadoPorUsuarioId = usuario.Id, EnviadoEn = DateTime.UtcNow
            };
            db.Entry(revisionAbierta).State = EntityState.Added;
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelarAsync(usuarioDgaa.Id,
                avisoCancelacionId, new CancelarAvisoMvcDatos("No debe cancelar con revisión abierta.")));
            Assert.Equal("AVALADO_DGAA", (await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoCancelacionId)).Estado);
            db.Entry(revisionAbierta).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            await service.CancelarAsync(usuarioDgaa.Id, avisoCancelacionId,
                new CancelarAvisoMvcDatos("La entidad retiró la solicitud."));
            var cancelado = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoCancelacionId);
            Assert.Equal("CANCELADO", cancelado.Estado);
            Assert.Equal("La entidad retiró la solicitud.", cancelado.MotivoCancelacion);
            Assert.Equal(usuarioDgaa.Id, cancelado.CanceladoPorUsuarioId);
            var intentoCancelado = await db.AvisoOfertas.AsNoTracking().SingleAsync(x => x.AvisoId == avisoCancelacionId);
            Assert.Equal("CANCELADO", intentoCancelado.CausaCierre);
            Assert.NotNull(intentoCancelado.CerradoEn);
            Assert.Equal("DISPONIBLE", (await db.Ofertas.AsNoTracking().SingleAsync(x => x.Id == ofertaCancelacion.Id)).Estado);
            await service.ArchivarAsync(usuarioDgaa.Id, avisoCancelacionId);
            Assert.NotNull((await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoCancelacionId)).ArchivadoEn);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ArchivarAsync(usuarioDgaa.Id, avisoCancelacionId));

            var page = await service.BuscarAsync(usuario.Id, new AvisoMvcFiltro());
            Assert.Contains(page.Items, x => x.Id == avisoId && x.Ofertas == 1);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.BuscarAsync(usuario.Id, new AvisoMvcFiltro(EntidadAcademicaId: entidad.Id + 50000)));
            await service.ArchivarAsync(usuario.Id, avisoId);
            var avisoPublicadoArchivado = await db.Avisos.AsNoTracking().SingleAsync(x => x.Id == avisoId);
            Assert.Equal("PUBLICADO", avisoPublicadoArchivado.Estado);
            Assert.Equal("https://example.org/avisos/" + avisoId, avisoPublicadoArchivado.UrlPublicacion);
            Assert.NotNull(avisoPublicadoArchivado.ArchivadoEn);
            Assert.Equal(2, await db.RevisionAvisos.AsNoTracking().CountAsync(x => x.AvisoId == avisoId));
        }
        finally
        {
            foreach (var clave in clavesDocumento)
                await almacenDocumentos.EliminarAsync(clave);
            if (Directory.Exists(raizDocumentos)) Directory.Delete(raizDocumentos, recursive: true);
            if (avisoId > 0)
            {
                db.ChangeTracker.Clear();
                if (solicitudId > 0)
                {
                    var solicitud = await db.Solicitudes.AsNoTracking().SingleAsync(x => x.Id == solicitudId);
                    await db.SolicitudDocumentos.Where(x => x.SolicitudId == solicitudId).ExecuteDeleteAsync();
                    await db.Solicitudes.Where(x => x.Id == solicitudId).ExecuteDeleteAsync();
                    await db.FormacionAspirantes.Where(x => x.PerfilAspiranteId == solicitud.PerfilAspiranteId
                        || db.PerfilAspirantes.Any(p => p.AspiranteId == solicitud.AspiranteId && p.Id == x.PerfilAspiranteId))
                        .ExecuteDeleteAsync();
                    var documentosAspirante = await db.DocumentoAspirantes.AsNoTracking()
                        .Where(x => x.AspiranteId == solicitud.AspiranteId).Select(x => x.Id).ToListAsync();
                    await db.VersionDocumentoAspirantes.Where(x => documentosAspirante.Contains(x.DocumentoAspiranteId))
                        .ExecuteDeleteAsync();
                    await db.DocumentoAspirantes.Where(x => x.AspiranteId == solicitud.AspiranteId).ExecuteDeleteAsync();
                    await db.PerfilAspirantes.Where(x => x.AspiranteId == solicitud.AspiranteId).ExecuteDeleteAsync();
                    await db.Aspirantes.Where(x => x.Id == solicitud.AspiranteId).ExecuteDeleteAsync();
                }
                var avisosFixture = new[] { avisoId, avisoCancelacionId }.Where(x => x > 0).ToArray();
                db.RevisionAvisos.RemoveRange(await db.RevisionAvisos.AsTracking().Where(x => avisosFixture.Contains(x.AvisoId)).ToListAsync());
                await db.SaveChangesAsync();
                db.HorarioRecepcionRequisitos.RemoveRange(await db.HorarioRecepcionRequisitos.AsTracking().Where(x => avisosFixture.Contains(x.AvisoId)).ToListAsync());
                db.DocumentoAvisos.RemoveRange(await db.DocumentoAvisos.AsTracking().Where(x => avisosFixture.Contains(x.AvisoId)).ToListAsync());
                db.AvisoOfertas.RemoveRange(await db.AvisoOfertas.AsTracking().Where(x => avisosFixture.Contains(x.AvisoId)).ToListAsync());
                await db.SaveChangesAsync();
                db.Avisos.RemoveRange(await db.Avisos.AsTracking().Where(x => avisosFixture.Contains(x.Id)).ToListAsync());
                await db.SaveChangesAsync();
            }
            db.UsuariosEntidadAcademica.RemoveRange(await db.UsuariosEntidadAcademica.AsTracking()
                .Where(x => x.UsuarioId == usuario.Id).ToListAsync());
            db.UsuariosDgaa.RemoveRange(await db.UsuariosDgaa.AsTracking()
                .Where(x => x.UsuarioId == usuarioDgaa.Id).ToListAsync());
            await db.SaveChangesAsync();
            if (usuario.Id > 0) db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuario.Id));
            if (usuarioDgaa.Id > 0) db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuarioDgaa.Id));
            if (articulo.Id > 0) db.Articulos.Remove(await db.Articulos.AsTracking().SingleAsync(x => x.Id == articulo.Id));
            await db.SaveChangesAsync();
            if (ofertaCancelacionId > 0)
            {
                db.Ofertas.Remove(await db.Ofertas.AsTracking().SingleAsync(x => x.Id == ofertaCancelacionId));
                await db.SaveChangesAsync();
            }
            if (oferta.Id > 0) db.Ofertas.Remove(await db.Ofertas.AsTracking().SingleAsync(x => x.Id == oferta.Id));
            await db.SaveChangesAsync();
            if (programacion.Id > 0) db.ProgramacionAcademicas.Remove(await db.ProgramacionAcademicas.AsTracking().SingleAsync(x => x.Id == programacion.Id));
            await db.SaveChangesAsync();
            if (experiencia.Id > 0) db.ExperienciasEducativas.Remove(await db.ExperienciasEducativas.AsTracking().SingleAsync(x => x.Id == experiencia.Id));
            await db.SaveChangesAsync();
            if (plan.Id > 0) db.PlanesEstudios.Remove(await db.PlanesEstudios.AsTracking().SingleAsync(x => x.Id == plan.Id));
            await db.SaveChangesAsync();
            if (programa.Id > 0) db.ProgramasEducativos.Remove(await db.ProgramasEducativos.AsTracking().SingleAsync(x => x.Id == programa.Id));
            await db.SaveChangesAsync();
            if (entidad.Id > 0) db.EntidadAcademicas.Remove(await db.EntidadAcademicas.AsTracking().SingleAsync(x => x.Id == entidad.Id));
            await db.SaveChangesAsync();
            if (periodo.Id > 0) db.PeriodosEscolares.Remove(await db.PeriodosEscolares.AsTracking().SingleAsync(x => x.Id == periodo.Id));
            await db.SaveChangesAsync();
        }
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)))
                Skip = $"Configura {ConnectionEnvironmentVariable} para ejecutar la prueba SQL Server.";
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset instant) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instant;
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "SGPla.Tests";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
