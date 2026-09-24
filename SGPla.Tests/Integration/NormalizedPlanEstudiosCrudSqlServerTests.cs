using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedPlanEstudiosCrudSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Crud_de_plan_y_experiencias_usa_esquema_academico_y_bajas_logicas()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaAcademicaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var sistemaId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var nivelId = await db.NivelesFormacion.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var areaFormacionId = await db.AreaFormaciones.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var entidad = new EntidadAcademica
        {
            Clave = $"IT{token[..10]}", Nombre = $"Entidad Plan {token}", Calle = "Calle de integración",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = areaAcademicaId, MunicipioId = municipioId
        };
        db.Entry(entidad).State = EntityState.Added;
        await db.SaveChangesAsync();
        var entidadId = entidad.Id;

        var programaRepository = new NormalizedProgramaEducativoMvcRepository(db);
        var programaService = new ProgramaEducativoMvcService(programaRepository, new CatalogosMvcService(db), TimeProvider.System);
        var planRepository = new NormalizedPlanEstudiosMvcRepository(db);
        var planService = new PlanEstudiosMvcService(planRepository, TimeProvider.System);
        var programaId = 0;
        var planId = 0;
        var sincronizacionId = 0;
        var programacionId = 0;
        var programacionSecundariaId = 0;
        var periodoId = 0;
        try
        {
            programaId = await programaService.GuardarAsync(new GuardarProgramaEducativoMvcDto
            {
                Nombre = $"Programa Plan {token}", EntidadAcademicaId = entidadId,
                SistemaEducativoId = sistemaId, NivelFormacionId = nivelId
            });
            planId = await planService.CrearAsync(new GuardarPlanEstudiosMvcDto
            {
                ProgramaEducativoId = programaId, Codigo = $"PL-{token[..12]}"
            });

            var filasImportadas = new[]
            {
                new GuardarExperienciaEducativaMvcDto
            {
                Nombre = $"Experiencia {token}", MateriaEe = $"MAT{token[..8]}",
                CursoEe = "01", HorasTeoricas = 2, HorasPracticas = 3, Creditos = 4,
                PerfilDocente = "Perfil inicial", AreaFormacionId = areaFormacionId
                }
            };
            Assert.Equal(1, await planService.ImportarExperienciasAsync(planId, areaFormacionId, filasImportadas));
            var experienceId = await db.ExperienciasEducativas.AsNoTracking()
                .Where(x => x.PlanEstudiosId == planId && x.FechaEliminacion == null)
                .Select(x => x.Id).SingleAsync();

            var planes = await planService.BuscarAsync(new PlanEstudiosMvcFiltro
            {
                RegionId = campus.RegionId, AreaAcademicaId = areaAcademicaId,
                EntidadAcademicaId = entidadId, ProgramaEducativoId = programaId,
                Busqueda = token[..12], Pagina = 1, TamanoPagina = 10
            });
            Assert.Contains(planes.Items, x => x.Id == planId && x.Codigo == $"PL-{token[..12]}");
            Assert.NotNull(await planService.ObtenerAsync(planId));

            var experiencias = await planService.BuscarExperienciasAsync(planId, token, 1, 10);
            var experience = Assert.Single(experiencias.Items);
            Assert.Equal(2, experience.HorasTeoricas);
            Assert.Equal(3, experience.HorasPracticas);
            Assert.Equal(4, experience.Creditos);

            Assert.True(await planService.ActualizarExperienciaAsync(new GuardarExperienciaEducativaMvcDto
            {
                Id = experienceId, PlanEstudiosId = planId, Nombre = $"Experiencia editada {token}",
                MateriaEe = "CAMBIO_MANIPULADO", CursoEe = "99", HorasTeoricas = 5,
                HorasPracticas = 6, Creditos = 7, PerfilDocente = "Perfil editado", AreaFormacionId = areaFormacionId
            }));
            var edited = await planService.ObtenerExperienciaAsync(planId, experienceId);
            Assert.NotNull(edited);
            Assert.Equal($"MAT{token[..8]}", edited.MateriaEe);
            Assert.Equal("01", edited.CursoEe);
            Assert.Equal(5, edited.HorasTeoricas);
            Assert.Equal(6, edited.HorasPracticas);

            var periodo = new PeriodoEscolar
            {
                Clave = (Convert.ToUInt32(token[..8], 16) % 1_000_000).ToString("D6"),
                FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 6, 30)
            };
            db.Entry(periodo).State = EntityState.Added;
            await db.SaveChangesAsync();
            periodoId = periodo.Id;
            var inicio = DateTime.UtcNow;
            var sincronizacion = new SincronizacionPlanea
            {
                PeriodoEscolarId = periodoId, Estado = "EXITOSA", IniciadaEn = inicio,
                FinalizadaEn = inicio, RegistrosRecibidos = 1, RegistrosIgnorados = 0,
                SesionesGeneradas = 1, DuplicadosDescartados = 0, Advertencias = 0
            };
            db.Entry(sincronizacion).State = EntityState.Added;
            await db.SaveChangesAsync();
            sincronizacionId = sincronizacion.Id;
            var programacion = new ProgramacionAcademica
            {
                Nrc = $"N{token[..18]}", PeriodoEscolarId = periodoId, ExperienciaEducativaId = experienceId
            };
            db.Entry(programacion).State = EntityState.Added;
            await db.SaveChangesAsync();
            programacionId = programacion.Id;
            var horario = new HorarioProgramacion
            {
                ProgramacionAcademicaId = programacionId, SincronizacionPlaneaId = sincronizacionId,
                DiaSemana = 1, HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(9, 0),
                FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 6, 30),
                Edificio = "Edificio integración", Aula = "Aula 1"
            };
            db.Entry(horario).State = EntityState.Added;
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => planService.ActualizarExperienciaAsync(new GuardarExperienciaEducativaMvcDto
            {
                Id = experienceId, PlanEstudiosId = planId, Nombre = $"Intento de cambio {token}",
                MateriaEe = edited.MateriaEe, CursoEe = edited.CursoEe, HorasTeoricas = 8,
                HorasPracticas = 6, Creditos = 7, PerfilDocente = "Perfil no guardado", AreaFormacionId = areaFormacionId
            }));
            Assert.True(await planService.ActualizarExperienciaAsync(new GuardarExperienciaEducativaMvcDto
            {
                Id = experienceId, PlanEstudiosId = planId, Nombre = $"Experiencia programada {token}",
                MateriaEe = edited.MateriaEe, CursoEe = edited.CursoEe, HorasTeoricas = edited.HorasTeoricas,
                HorasPracticas = edited.HorasPracticas, Creditos = edited.Creditos,
                PerfilDocente = "Perfil permitido", AreaFormacionId = edited.AreaFormacionId
            }));
            var programada = await planService.ObtenerExperienciaAsync(planId, experienceId);
            Assert.Equal(5, programada!.HorasTeoricas);
            Assert.Equal(6, programada.HorasPracticas);
            Assert.Equal(7, programada.Creditos);

            var experienciaSecundariaId = await planService.CrearExperienciaAsync(new GuardarExperienciaEducativaMvcDto
            {
                PlanEstudiosId = planId, Nombre = $"Experiencia secundaria {token}", MateriaEe = $"SEC{token[..7]}",
                CursoEe = "02", HorasTeoricas = 1, HorasPracticas = 1, Creditos = 2, AreaFormacionId = areaFormacionId
            });
            var programacionSecundaria = new ProgramacionAcademica
            {
                Nrc = $"S{token[..18]}", PeriodoEscolarId = periodoId, ExperienciaEducativaId = experienciaSecundariaId
            };
            db.Entry(programacionSecundaria).State = EntityState.Added;
            await db.SaveChangesAsync();
            programacionSecundariaId = programacionSecundaria.Id;
            db.Entry(new HorarioProgramacion
            {
                ProgramacionAcademicaId = programacionSecundariaId, SincronizacionPlaneaId = sincronizacionId,
                DiaSemana = 2, HoraInicio = new TimeOnly(10, 0), HoraFin = new TimeOnly(11, 0),
                FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 6, 30),
                Edificio = "Edificio integración", Aula = "Aula 2"
            }).State = EntityState.Added;
            await db.SaveChangesAsync();
            Assert.True(await planService.EliminarExperienciaAsync(planId, experienciaSecundariaId));
            Assert.NotNull(await db.ProgramacionAcademicas.IgnoreQueryFilters().Where(x => x.Id == programacionSecundariaId)
                .Select(x => x.FechaEliminacion).SingleAsync());
            Assert.False(await db.HorariosProgramacion.AsNoTracking().AnyAsync(x => x.ProgramacionAcademicaId == programacionSecundariaId));

            Assert.False(await planService.ActualizarExperienciaAsync(new GuardarExperienciaEducativaMvcDto
            {
                Id = experienceId, PlanEstudiosId = planId + 50000, Nombre = "Experiencia ajena",
                MateriaEe = edited.MateriaEe, CursoEe = edited.CursoEe, HorasTeoricas = 1,
                HorasPracticas = 1, Creditos = 1, AreaFormacionId = areaFormacionId
            }));

            Assert.True(await planService.EliminarAsync(planId));
            Assert.Null(await planService.ObtenerAsync(planId));
            var fechaPlan = await db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.Id == planId)
                .Select(x => x.FechaEliminacion).SingleAsync();
            var fechaExperiencia = await db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => x.Id == experienceId)
                .Select(x => x.FechaEliminacion).SingleAsync();
            Assert.NotNull(fechaPlan);
            Assert.Equal(fechaPlan, fechaExperiencia);
            Assert.Equal(fechaPlan, await db.ProgramacionAcademicas.IgnoreQueryFilters().Where(x => x.Id == programacionId).Select(x => x.FechaEliminacion).SingleAsync());
            Assert.False(await db.HorariosProgramacion.AsNoTracking().AnyAsync(x => x.ProgramacionAcademicaId == programacionId));
        }
        finally
        {
            if (programaId > 0)
            {
                var planIds = db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.ProgramaEducativoId == programaId).Select(x => x.Id);
                var experienceIds = db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => planIds.Contains(x.PlanEstudiosId)).Select(x => x.Id);
                var programmingIds = db.ProgramacionAcademicas.IgnoreQueryFilters().Where(x => experienceIds.Contains(x.ExperienciaEducativaId)).Select(x => x.Id);
                await db.HorariosProgramacion.IgnoreQueryFilters().Where(x => programmingIds.Contains(x.ProgramacionAcademicaId)).ExecuteDeleteAsync();
                await db.ProgramacionAcademicas.IgnoreQueryFilters().Where(x => experienceIds.Contains(x.ExperienciaEducativaId)).ExecuteDeleteAsync();
                await db.ExperienciasEducativas.IgnoreQueryFilters().Where(x => planIds.Contains(x.PlanEstudiosId)).ExecuteDeleteAsync();
                await db.PlanesEstudios.IgnoreQueryFilters().Where(x => x.ProgramaEducativoId == programaId).ExecuteDeleteAsync();
                if (sincronizacionId > 0) await db.SincronizacionesPlanea.IgnoreQueryFilters().Where(x => x.Id == sincronizacionId).ExecuteDeleteAsync();
                await db.ProgramasEducativos.IgnoreQueryFilters().Where(x => x.Id == programaId).ExecuteDeleteAsync();
            }
            if (periodoId > 0) await db.PeriodosEscolares.IgnoreQueryFilters().Where(x => x.Id == periodoId).ExecuteDeleteAsync();
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
