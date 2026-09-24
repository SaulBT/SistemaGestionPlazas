using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Repositories.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedProgramacionAcademicaImportSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Importa_vacantes_y_descargas_como_programacion_normalizada_idempotente()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var region = new Region
        {
            Clave = 1_000_000_000 + (int)(Convert.ToUInt32(token[..8], 16) % 1_000_000_000),
            Nombre = $"Región import {token}"
        };
        db.Entry(region).State = EntityState.Added;
        await db.SaveChangesAsync();
        var campus = new Campus { Clave = $"I{token[..20]}", Nombre = $"Campus import {token}", RegionId = region.Id };
        db.Entry(campus).State = EntityState.Added;
        await db.SaveChangesAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().Select(x => x.Id).FirstAsync();
        var entidad = new EntidadAcademica
        {
            Clave = $"E{token[..15]}", Nombre = $"Entidad import {token}", Calle = "Calle 1", Colonia = "Centro",
            CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        db.Entry(entidad).State = EntityState.Added;
        await db.SaveChangesAsync();
        var programa = new ProgramaEducativo
        {
            Nombre = $"Programa import {token}", EntidadAcademicaId = entidad.Id,
            SistemaEducativoId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync(),
            NivelFormacionId = await db.NivelesFormacion.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync()
        };
        db.Entry(programa).State = EntityState.Added;
        await db.SaveChangesAsync();
        var plan = new PlanEstudios { Codigo = $"P{token[..15]}", ProgramaEducativoId = programa.Id };
        db.Entry(plan).State = EntityState.Added;
        await db.SaveChangesAsync();
        var ee = new ExperienciaEducativa
        {
            Nombre = $"EE import {token}", MateriaEe = $"M{token[..10]}", CursoEe = "01",
            HorasTeoricas = 2, HorasPracticas = 1, Creditos = 3,
            AreaFormacionId = await db.AreaFormaciones.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync(),
            PlanEstudiosId = plan.Id
        };
        var eeAlterna = new ExperienciaEducativa
        {
            Nombre = $"EE alterna {token}", MateriaEe = $"A{token[..10]}", CursoEe = "01",
            HorasTeoricas = 2, HorasPracticas = 1, Creditos = 3,
            AreaFormacionId = ee.AreaFormacionId, PlanEstudiosId = plan.Id
        };
        db.Entry(ee).State = EntityState.Added;
        db.Entry(eeAlterna).State = EntityState.Added;
        var periodo = new PeriodoEscolar
        {
            Clave = (Convert.ToUInt32(token[8..16], 16) % 1_000_000).ToString("D6"),
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 6, 30)
        };
        db.Entry(periodo).State = EntityState.Added;
        await db.SaveChangesAsync();

        var repository = new NormalizedProgramacionAcademicaMvcRepository(db);
        int? programacionCreadaId = null;
        try
        {
            var fila = new ProgramacionAcademicaImportacionFila(programa.Nombre, ee.Nombre, "AB123");
            var importada = await repository.ImportarAsync(periodo.Id, entidad.Id, [fila]);
            Assert.Equal(new ResultadoImportacionProgramacionMvc(1, 0, 1), importada);
            Assert.Equal(new ResultadoImportacionProgramacionMvc(0, 1, 1), await repository.ImportarAsync(periodo.Id, entidad.Id, [fila]));

            var listado = await repository.BuscarAsync(new ProgramacionAcademicaMvcFiltro(
                RegionId: region.Id, EntidadAcademicaId: entidad.Id, ProgramaEducativoId: programa.Id,
                PeriodoEscolarId: periodo.Id, Busqueda: "AB12"), entidad.Id);
            var item = Assert.Single(listado.Items);
            Assert.Equal(ee.Id, item.ExperienciaEducativaId);
            Assert.Equal(programa.Nombre, item.ProgramaEducativo);

            var tipoPlazaId = await db.TipoPlazas.AsNoTracking().Select(x => x.Id).FirstAsync();
            var tipoContratacionId = await db.TiposContratacion.AsNoTracking().Select(x => x.Id).FirstAsync();
            var ofertas = new NormalizedOfertaMvcRepository(db, TimeProvider.System);
            programacionCreadaId = await db.ProgramacionAcademicas.AsNoTracking()
                .Where(x => x.PeriodoEscolarId == periodo.Id && x.Nrc == "AB123").Select(x => x.Id).SingleAsync();
            var ofertaId = await ofertas.CrearAsync(new(programacionCreadaId.Value, "prof-01", tipoPlazaId,
                tipoContratacionId, "Perfil requerido", null), entidad.Id);
            var oferta = await db.Ofertas.AsNoTracking().SingleAsync(x => x.Id == ofertaId);
            Assert.Equal("PROF-01", oferta.ClavePlaza);
            Assert.Equal("DISPONIBLE", oferta.Estado);
            await Assert.ThrowsAsync<InvalidOperationException>(() => ofertas.CrearAsync(new(
                oferta.ProgramacionAcademicaId, "PROF-01", tipoPlazaId, tipoContratacionId, "Otro perfil", null), entidad.Id));
            await Assert.ThrowsAsync<ArgumentException>(() => ofertas.CrearAsync(new(
                oferta.ProgramacionAcademicaId, "PROF-02", tipoPlazaId, tipoContratacionId, "Perfil", null), entidad.Id + 1_000_000));

            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ImportarAsync(periodo.Id, entidad.Id,
                [new ProgramacionAcademicaImportacionFila(programa.Nombre, eeAlterna.Nombre, "AB123")]));
            await Assert.ThrowsAsync<ArgumentException>(() => repository.ImportarAsync(periodo.Id, entidad.Id,
                [new ProgramacionAcademicaImportacionFila("Programa externo", ee.Nombre, "CD456")]));
        }
        finally
        {
            if (programacionCreadaId.HasValue)
                await db.Ofertas.IgnoreQueryFilters().Where(x => x.ProgramacionAcademicaId == programacionCreadaId.Value).ExecuteDeleteAsync();
            await db.ProgramacionAcademicas.IgnoreQueryFilters().Where(x => x.PeriodoEscolarId == periodo.Id).ExecuteDeleteAsync();
            await db.PeriodosEscolares.IgnoreQueryFilters().Where(x => x.Id == periodo.Id).ExecuteDeleteAsync();
            await db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => x.PlanEstudiosId == plan.Id).ExecuteDeleteAsync();
            await db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.Id == plan.Id).ExecuteDeleteAsync();
            await db.ProgramasEducativos.IgnoreQueryFilters().Where(x => x.Id == programa.Id).ExecuteDeleteAsync();
            await db.EntidadAcademicas.IgnoreQueryFilters().Where(x => x.Id == entidad.Id).ExecuteDeleteAsync();
            await db.Campuses.IgnoreQueryFilters().Where(x => x.Id == campus.Id).ExecuteDeleteAsync();
            await db.Regiones.IgnoreQueryFilters().Where(x => x.Id == region.Id).ExecuteDeleteAsync();
        }
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)))
                Skip = $"Configura {ConnectionEnvironmentVariable} para ejecutar la prueba de integración con SQL Server.";
        }
    }
}
