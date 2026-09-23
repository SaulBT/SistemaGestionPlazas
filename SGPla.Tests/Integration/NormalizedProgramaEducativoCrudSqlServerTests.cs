using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

public sealed class NormalizedProgramaEducativoCrudSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Crud_y_baja_en_cascada_operan_en_el_esquema_academico()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaAcademicaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var sistemaId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var nivelId = await db.NivelesFormacion.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var areaFormacionId = await db.AreaFormaciones.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var token = Guid.NewGuid().ToString("N");
        var entidad = new EntidadAcademica
        {
            Clave = $"IT{token[..10]}".ToUpperInvariant(), Nombre = $"Entidad Programa {token}",
            Calle = "Calle de integración", Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234567",
            CampusId = campus.Id, AreaAcademicaId = areaAcademicaId, MunicipioId = municipioId
        };
        db.Entry(entidad).State = EntityState.Added;
        await db.SaveChangesAsync();
        var entidadId = entidad.Id;
        var repo = new NormalizedProgramaEducativoMvcRepository(db);
        var service = new ProgramaEducativoMvcService(repo, new CatalogosMvcService(db), TimeProvider.System);
        var programaId = 0;
        int planId = 0;
        try
        {
            programaId = await service.GuardarAsync(new GuardarProgramaEducativoMvcDto
            {
                Nombre = $"Programa integración {token}", EntidadAcademicaId = entidadId,
                SistemaEducativoId = sistemaId, NivelFormacionId = nivelId
            });
            var plan = new PlanEstudios { Codigo = $"IT{token[..12]}".ToUpperInvariant(), ProgramaEducativoId = programaId };
            db.Entry(plan).State = EntityState.Added;
            await db.SaveChangesAsync();
            planId = plan.Id;
            var experiencia = new ExperienciaEducativa
            {
                Nombre = $"EE integración {token}", MateriaEe = token[..6].ToUpperInvariant(), CursoEe = "01", Creditos = 1,
                HorasTeoricas = 1, HorasPracticas = 0, AreaFormacionId = areaFormacionId, PlanEstudiosId = planId
            };
            db.Entry(experiencia).State = EntityState.Added;
            await db.SaveChangesAsync();

            var detail = await service.ObtenerAsync(programaId);
            Assert.NotNull(detail);
            Assert.Equal(entidadId, detail.EntidadAcademicaId);
            Assert.Contains((await service.BuscarAsync(new ProgramaEducativoMvcFiltro { Nombre = token })).Items, x => x.Id == programaId);

            await service.GuardarAsync(new GuardarProgramaEducativoMvcDto
            {
                Id = programaId, Nombre = $"Programa integración editado {token}", EntidadAcademicaId = entidadId,
                SistemaEducativoId = sistemaId, NivelFormacionId = nivelId
            });
            Assert.Equal($"Programa integración editado {token}", (await service.ObtenerAsync(programaId))?.Nombre);

            Assert.True(await service.EliminarAsync(programaId));
            var fechaPrograma = await db.ProgramasEducativos.IgnoreQueryFilters().Where(x => x.Id == programaId)
                .Select(x => x.FechaEliminacion).SingleAsync();
            var fechaPlan = await db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.Id == planId)
                .Select(x => x.FechaEliminacion).SingleAsync();
            var fechaEe = await db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => x.PlanEstudiosId == planId)
                .Select(x => x.FechaEliminacion).SingleAsync();
            Assert.NotNull(fechaPrograma);
            Assert.Equal(fechaPrograma, fechaPlan);
            Assert.Equal(fechaPrograma, fechaEe);
            Assert.Null(await service.ObtenerAsync(programaId));
        }
        finally
        {
            if (programaId > 0)
            {
                var planIds = db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.ProgramaEducativoId == programaId).Select(x => x.Id);
                await db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => planIds.Contains(x.PlanEstudiosId)).ExecuteDeleteAsync();
                await db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.ProgramaEducativoId == programaId).ExecuteDeleteAsync();
                await db.ProgramasEducativos.IgnoreQueryFilters().Where(x => x.Id == programaId).ExecuteDeleteAsync();
            }
            await db.EntidadAcademicas.IgnoreQueryFilters().Where(x => x.Id == entidadId).ExecuteDeleteAsync();
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
