using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using Xunit.Abstractions;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedPlaneaSyncSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";
    private readonly ITestOutputHelper _output;

    public NormalizedPlaneaSyncSqlServerTests(ITestOutputHelper output) => _output = output;

    [SqlServerFact]
    public async Task Sincroniza_atomico_y_no_sobrescribe_asignacion_con_Oferta()
    {
        var (db, fixture) = await CrearFixtureAsync();
        await using (db)
        {
            try
            {
                var cliente = new FakePlaneaClient(fixture.Periodo.Clave,
                [
                    Registro(fixture.Periodo.Clave, fixture.ProgramacionConOferta.Nrc, "OFERTA01", "Docente Oferta", 1, "0800", "0859"),
                    Registro(fixture.Periodo.Clave, fixture.ProgramacionSinOferta.Nrc, "NUEVO01", "Docente Nuevo", 2, "1000", "1059")
                ]);
                var sincronizador = CrearServicio(db, cliente);

                var sincronizacionId = await sincronizador.SincronizarAsync(fixture.Periodo.Id);

                var bitacora = await db.SincronizacionesPlanea.AsNoTracking().SingleAsync(x => x.Id == sincronizacionId);
                Assert.Equal("EXITOSA", bitacora.Estado);
                Assert.Equal(2, bitacora.RegistrosRecibidos);
                Assert.Equal(2, bitacora.SesionesGeneradas);
                Assert.Equal(1, bitacora.Advertencias);
                var horarios = await db.HorariosProgramacion.AsNoTracking()
                    .Where(x => x.ProgramacionAcademicaId == fixture.ProgramacionConOferta.Id
                        || x.ProgramacionAcademicaId == fixture.ProgramacionSinOferta.Id).ToListAsync();
                Assert.Equal(2, horarios.Count);
                Assert.All(horarios, x => Assert.Equal(sincronizacionId, x.SincronizacionPlaneaId));

                var asignacionProtegida = await db.AsignacionDocentes.AsNoTracking()
                    .SingleAsync(x => x.ProgramacionAcademicaId == fixture.ProgramacionConOferta.Id);
                Assert.Equal(fixture.DocenteProtegido.Id, asignacionProtegida.DocenteId);
                Assert.Equal(fixture.SincronizacionAnterior.Id, asignacionProtegida.SincronizacionPlaneaId);
                Assert.True(await db.Docentes.AsNoTracking().AnyAsync(x => x.NumPersonal == "OFERTA01"));
                var asignacionActualizada = await db.AsignacionDocentes.AsNoTracking()
                    .SingleAsync(x => x.ProgramacionAcademicaId == fixture.ProgramacionSinOferta.Id);
                var docenteNuevo = await db.Docentes.AsNoTracking().SingleAsync(x => x.Id == asignacionActualizada.DocenteId);
                Assert.Equal("NUEVO01", docenteNuevo.NumPersonal);
                Assert.Equal(sincronizacionId, asignacionActualizada.SincronizacionPlaneaId);
            }
            finally
            {
                await LimpiarAsync(db, fixture);
            }
        }
    }

    [SqlServerFact]
    public async Task Fallo_de_validacion_conserva_horarios_y_cierra_bitacora_como_fallida()
    {
        var (db, fixture) = await CrearFixtureAsync();
        await using (db)
        {
            try
            {
                var cliente = new FakePlaneaClient(fixture.Periodo.Clave,
                    [Registro(fixture.Periodo.Clave, "NRC-DESCONOCIDO", null, null, 1, "0800", "0859")]);
                var sincronizador = CrearServicio(db, cliente);

                await Assert.ThrowsAsync<InvalidDataException>(() => sincronizador.SincronizarAsync(fixture.Periodo.Id));

                var intento = await db.SincronizacionesPlanea.AsNoTracking()
                    .Where(x => x.PeriodoEscolarId == fixture.Periodo.Id && x.Id != fixture.SincronizacionAnterior.Id)
                    .SingleAsync();
                Assert.Equal("FALLIDA", intento.Estado);
                Assert.NotNull(intento.FinalizadaEn);
                Assert.Contains("NRC-DESCONOCIDO", intento.MensajeError);
                var horarioVigente = await db.HorariosProgramacion.AsNoTracking()
                    .Where(x => x.ProgramacionAcademicaId == fixture.ProgramacionConOferta.Id).SingleAsync();
                Assert.Equal(fixture.SincronizacionAnterior.Id, horarioVigente.SincronizacionPlaneaId);
            }
            finally
            {
                await LimpiarAsync(db, fixture);
            }
        }
    }

    [SqlServerFact]
    public async Task Solicitud_se_registra_sin_descargar_rechaza_concurrente_y_publica_estado_final()
    {
        var (db, fixture) = await CrearFixtureAsync();
        await using (db)
        {
            try
            {
                var cliente = new FakePlaneaClient(fixture.Periodo.Clave,
                    [Registro(fixture.Periodo.Clave, fixture.ProgramacionSinOferta.Nrc, "ASYNC01", "Docente async", 1, "0900", "0959")]);
                var canal = new CanalSincronizacionPlanea();
                var sincronizador = new SincronizacionPlaneaService(db, cliente,
                    new PlaneaSnapshotValidator(), TimeProvider.System,
                    NullLogger<SincronizacionPlaneaService>.Instance, canal);

                var id = await sincronizador.SolicitarAsync(fixture.Periodo.Id);
                Assert.Equal(0, cliente.Llamadas);
                Assert.Equal("EN_PROCESO", (await sincronizador.ObtenerEstadoAsync(id))!.Estado);
                await Assert.ThrowsAsync<InvalidOperationException>(() => sincronizador.SolicitarAsync(fixture.Periodo.Id));

                await sincronizador.EjecutarRegistradaAsync(id);

                Assert.Equal("EXITOSA", (await sincronizador.ObtenerEstadoAsync(id))!.Estado);
                Assert.Equal(1, cliente.Llamadas);
            }
            finally { await LimpiarAsync(db, fixture); }
        }
    }

    [SqlServerFact]
    public async Task Aplicacion_masiva_reemplaza_veinte_mil_sesiones()
    {
        var (db, fixture) = await CrearFixtureAsync();
        await using (db)
        {
            try
            {
                const int cantidad = 20_000;
                var registros = Enumerable.Range(0, cantidad).Select(i => RegistroVolumen(
                    fixture.Periodo.Clave, fixture.ProgramacionSinOferta.Nrc, i)).ToArray();
                var cliente = new FakePlaneaClient(fixture.Periodo.Clave, registros);
                var logger = new OutputLogger<SincronizacionPlaneaService>(_output);
                var sincronizador = new SincronizacionPlaneaService(db, cliente,
                    new PlaneaSnapshotValidator(), TimeProvider.System, logger);
                var cronometro = Stopwatch.StartNew();

                var id = await sincronizador.SincronizarAsync(fixture.Periodo.Id);

                cronometro.Stop();
                var bitacora = await db.SincronizacionesPlanea.AsNoTracking().SingleAsync(x => x.Id == id);
                Assert.Equal("EXITOSA", bitacora.Estado);
                Assert.Equal(cantidad, bitacora.RegistrosRecibidos);
                Assert.Equal(cantidad, bitacora.SesionesGeneradas);
                Assert.Equal(cantidad, await db.HorariosProgramacion.AsNoTracking()
                    .CountAsync(x => x.ProgramacionAcademicaId == fixture.ProgramacionSinOferta.Id
                        && x.SincronizacionPlaneaId == id));
                _output.WriteLine($"AplicarSnapshotAsync procesa {cantidad:N0} sesiones; sincronización completa: {cronometro.ElapsedMilliseconds:N0} ms.");
            }
            finally { await LimpiarAsync(db, fixture); }
        }
    }

    private static SincronizacionPlaneaService CrearServicio(SgplaDbContext db, IPlaneaClient cliente) =>
        new(db, cliente, new PlaneaSnapshotValidator(), TimeProvider.System,
            NullLogger<SincronizacionPlaneaService>.Instance);

    private static async Task<(SgplaDbContext Db, Fixture Fixture)> CrearFixtureAsync()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);
        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var region = new Region
        {
            Clave = 500_000_000 + (int)(Convert.ToUInt32(token[..8], 16) % 1_000_000_000),
            Nombre = $"Region PLANEA {token}"
        };
        db.Entry(region).State = EntityState.Added;
        await db.SaveChangesAsync();
        var campus = new Campus { Clave = $"P{token[..20]}", Nombre = $"Campus PLANEA {token}", RegionId = region.Id };
        db.Entry(campus).State = EntityState.Added;
        await db.SaveChangesAsync();
        var entidad = new EntidadAcademica
        {
            Clave = $"P{token[..15]}", Nombre = $"Entidad PLANEA {token}", Calle = "Calle", Colonia = "Centro",
            CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync(),
            MunicipioId = await db.Municipios.AsNoTracking().Select(x => x.Id).FirstAsync()
        };
        db.Entry(entidad).State = EntityState.Added;
        await db.SaveChangesAsync();
        var programa = new ProgramaEducativo
        {
            Nombre = $"Programa PLANEA {token}", EntidadAcademicaId = entidad.Id,
            SistemaEducativoId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync(),
            NivelFormacionId = await db.NivelesFormacion.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync()
        };
        db.Entry(programa).State = EntityState.Added;
        await db.SaveChangesAsync();
        var plan = new PlanEstudios { Codigo = $"P{token[..15]}", ProgramaEducativoId = programa.Id };
        db.Entry(plan).State = EntityState.Added;
        await db.SaveChangesAsync();
        var areaFormacionId = await db.AreaFormaciones.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var experiencia1 = CrearExperiencia(plan.Id, areaFormacionId, token, "A");
        var experiencia2 = CrearExperiencia(plan.Id, areaFormacionId, token, "B");
        db.Entry(experiencia1).State = EntityState.Added;
        db.Entry(experiencia2).State = EntityState.Added;
        var periodo = new PeriodoEscolar
        {
            Clave = (Convert.ToUInt32(token[8..16], 16) % 1_000_000).ToString("D6"),
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 12, 31)
        };
        db.Entry(periodo).State = EntityState.Added;
        await db.SaveChangesAsync();
        var programacionConOferta = new ProgramacionAcademica
        {
            Nrc = $"A{token[..10]}", PeriodoEscolarId = periodo.Id, ExperienciaEducativaId = experiencia1.Id
        };
        var programacionSinOferta = new ProgramacionAcademica
        {
            Nrc = $"B{token[..10]}", PeriodoEscolarId = periodo.Id, ExperienciaEducativaId = experiencia2.Id
        };
        db.Entry(programacionConOferta).State = EntityState.Added;
        db.Entry(programacionSinOferta).State = EntityState.Added;
        await db.SaveChangesAsync();
        var syncAnterior = new SincronizacionPlanea
        {
            PeriodoEscolarId = periodo.Id, Estado = "EXITOSA", IniciadaEn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FinalizadaEn = new DateTime(2026, 1, 1, 0, 1, 0, DateTimeKind.Utc), SesionesGeneradas = 2
        };
        var docenteProtegido = new Docente { Nombre = "Docente protegido", NumPersonal = $"K{token[..9]}" };
        var docenteAnterior = new Docente { Nombre = "Docente anterior", NumPersonal = $"O{token[..9]}" };
        db.Entry(syncAnterior).State = EntityState.Added;
        db.Entry(docenteProtegido).State = EntityState.Added;
        db.Entry(docenteAnterior).State = EntityState.Added;
        await db.SaveChangesAsync();
        foreach (var programacion in new[] { programacionConOferta, programacionSinOferta })
        {
            db.Entry(new HorarioProgramacion
            {
                ProgramacionAcademicaId = programacion.Id, SincronizacionPlaneaId = syncAnterior.Id,
                DiaSemana = 1, HoraInicio = new TimeOnly(7, 0), HoraFin = new TimeOnly(7, 59),
                FechaInicio = periodo.FechaInicio, FechaFin = periodo.FechaFin
            }).State = EntityState.Added;
        }
        db.Entry(new AsignacionDocente
        {
            ProgramacionAcademicaId = programacionConOferta.Id, DocenteId = docenteProtegido.Id,
            Origen = "PLANEA", FechaInicio = periodo.FechaInicio, FechaFin = periodo.FechaFin,
            SincronizacionPlaneaId = syncAnterior.Id
        }).State = EntityState.Added;
        db.Entry(new AsignacionDocente
        {
            ProgramacionAcademicaId = programacionSinOferta.Id, DocenteId = docenteAnterior.Id,
            Origen = "PLANEA", FechaInicio = periodo.FechaInicio, FechaFin = periodo.FechaFin,
            SincronizacionPlaneaId = syncAnterior.Id
        }).State = EntityState.Added;
        db.Entry(new Oferta
        {
            ProgramacionAcademicaId = programacionConOferta.Id, ClavePlaza = "PLAZA-1",
            TipoPlazaId = await db.TipoPlazas.AsNoTracking().Select(x => x.Id).FirstAsync(),
            TipoContratacionId = await db.TiposContratacion.AsNoTracking().Select(x => x.Id).FirstAsync(),
            PerfilSolicitado = "Perfil de prueba", Estado = "DISPONIBLE"
        }).State = EntityState.Added;
        await db.SaveChangesAsync();
        return (db, new Fixture(region, campus, entidad, programa, plan, experiencia1, experiencia2,
            periodo, programacionConOferta, programacionSinOferta, syncAnterior, docenteProtegido, docenteAnterior));
    }

    private static ExperienciaEducativa CrearExperiencia(int planId, int areaFormacionId, string token, string sufijo) => new()
    {
        Nombre = $"EE PLANEA {sufijo} {token}", MateriaEe = $"{sufijo}{token[..8]}", CursoEe = "01",
        HorasTeoricas = 2, HorasPracticas = 1, Creditos = 3, AreaFormacionId = areaFormacionId, PlanEstudiosId = planId
    };

    private static PlaneaRegistro Registro(string periodo, string nrc, string? numPersonal, string? nombre,
        byte dia, string inicio, string fin)
    {
        var horarios = Enumerable.Range(1, 6).Select(x => new PlaneaHorarioDia((byte)x,
            x == dia ? inicio : null, x == dia ? fin : null)).ToArray();
        return new PlaneaRegistro(periodo, nrc, numPersonal, nombre, "Edificio", "Aula", "2026-01-01", "2026-12-31", horarios);
    }

    private static PlaneaRegistro RegistroVolumen(string periodo, string nrc, int indice)
    {
        var dia = (byte)(indice % 6 + 1);
        var minuto = indice / 6 % 1439;
        var fecha = new DateOnly(2026, 1, 1).AddDays(indice / (6 * 1439) * 30);
        var inicio = TimeOnly.MinValue.AddMinutes(minuto).ToString("HHmm", CultureInfo.InvariantCulture);
        var fin = TimeOnly.MinValue.AddMinutes(minuto + 1).ToString("HHmm", CultureInfo.InvariantCulture);
        var horarios = Enumerable.Range(1, 6).Select(x => new PlaneaHorarioDia((byte)x,
            x == dia ? inicio : null, x == dia ? fin : null)).ToArray();
        return new PlaneaRegistro(periodo, nrc, "VOLUMEN01", "Docente volumen", "Edificio", "Aula",
            fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "2026-12-31", horarios);
    }

    private static async Task LimpiarAsync(SgplaDbContext db, Fixture fixture)
    {
        var ids = new[] { fixture.ProgramacionConOferta.Id, fixture.ProgramacionSinOferta.Id };
        await db.HorariosProgramacion.Where(x => ids.Contains(x.ProgramacionAcademicaId)).ExecuteDeleteAsync();
        await db.AsignacionDocentes.Where(x => ids.Contains(x.ProgramacionAcademicaId)).ExecuteDeleteAsync();
        await db.Ofertas.Where(x => ids.Contains(x.ProgramacionAcademicaId)).ExecuteDeleteAsync();
        await db.ProgramacionAcademicas.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();
        await db.SincronizacionesPlanea.Where(x => x.PeriodoEscolarId == fixture.Periodo.Id).ExecuteDeleteAsync();
        await db.Docentes.Where(x => x.Id == fixture.DocenteProtegido.Id || x.Id == fixture.DocenteAnterior.Id
            || x.NumPersonal == "NUEVO01" || x.NumPersonal == "OFERTA01").ExecuteDeleteAsync();
        await db.PeriodosEscolares.Where(x => x.Id == fixture.Periodo.Id).ExecuteDeleteAsync();
        await db.ExperienciasEducativas.Where(x => x.PlanEstudiosId == fixture.Plan.Id).ExecuteDeleteAsync();
        await db.PlanesEstudios.Where(x => x.Id == fixture.Plan.Id).ExecuteDeleteAsync();
        await db.ProgramasEducativos.Where(x => x.Id == fixture.Programa.Id).ExecuteDeleteAsync();
        await db.EntidadAcademicas.Where(x => x.Id == fixture.Entidad.Id).ExecuteDeleteAsync();
        await db.Campuses.Where(x => x.Id == fixture.Campus.Id).ExecuteDeleteAsync();
        await db.Regiones.Where(x => x.Id == fixture.Region.Id).ExecuteDeleteAsync();
    }

    private sealed record Fixture(Region Region, Campus Campus, EntidadAcademica Entidad, ProgramaEducativo Programa,
        PlanEstudios Plan, ExperienciaEducativa Experiencia1, ExperienciaEducativa Experiencia2, PeriodoEscolar Periodo,
        ProgramacionAcademica ProgramacionConOferta, ProgramacionAcademica ProgramacionSinOferta,
        SincronizacionPlanea SincronizacionAnterior, Docente DocenteProtegido, Docente DocenteAnterior);

    private sealed class FakePlaneaClient(string clavePeriodo, IReadOnlyList<PlaneaRegistro> registros) : IPlaneaClient
    {
        public int Llamadas { get; private set; }
        public Task<IReadOnlyList<PlaneaRegistro>> ObtenerProgramacionesAsync(string clave, CancellationToken cancellationToken = default)
        {
            Llamadas++;
            Assert.Equal(clavePeriodo, clave);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(registros);
        }
    }

    private sealed class OutputLogger<T>(ITestOutputHelper output) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => output.WriteLine(formatter(state, exception));
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
