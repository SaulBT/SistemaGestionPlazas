using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Avisos;
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
        var documentoOriginalId = 0;
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

            var page = await service.BuscarAsync(usuario.Id, new AvisoMvcFiltro());
            Assert.Contains(page.Items, x => x.Id == avisoId && x.Ofertas == 1);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.BuscarAsync(usuario.Id, new AvisoMvcFiltro(EntidadAcademicaId: entidad.Id + 50000)));
        }
        finally
        {
            if (avisoId > 0)
            {
                db.RevisionAvisos.RemoveRange(await db.RevisionAvisos.AsTracking().Where(x => x.AvisoId == avisoId).ToListAsync());
                await db.SaveChangesAsync();
                db.HorarioRecepcionRequisitos.RemoveRange(await db.HorarioRecepcionRequisitos.AsTracking().Where(x => x.AvisoId == avisoId).ToListAsync());
                db.DocumentoAvisos.RemoveRange(await db.DocumentoAvisos.AsTracking().Where(x => x.AvisoId == avisoId).ToListAsync());
                db.AvisoOfertas.RemoveRange(await db.AvisoOfertas.AsTracking().Where(x => x.AvisoId == avisoId).ToListAsync());
                await db.SaveChangesAsync();
                db.Avisos.Remove(await db.Avisos.AsTracking().SingleAsync(x => x.Id == avisoId));
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
}
