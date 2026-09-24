using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedIntegranteConsejoTecnicoSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Versiona_cambios_de_cargo_y_tratamiento_en_el_ambito_de_claims()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().Select(x => x.Id).FirstAsync();
        var sistemaId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .Select(x => x.Id).FirstAsync();
        var rolId = await db.Roles.AsNoTracking().Where(x => x.Id == 3).Select(x => x.Id).SingleAsync();
        var tratamientoId = await db.TratamientosAcademicos.AsNoTracking().Select(x => x.Id).FirstAsync();
        var periodo = new PeriodoEscolar
        {
            Clave = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString(),
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 12, 31)
        };
        db.Entry(periodo).State = EntityState.Added;
        await db.SaveChangesAsync();
        var entidad = new EntidadAcademica
        {
            Clave = $"CT{token[..10]}", Nombre = $"Entidad CT {token}", Calle = "Calle de prueba",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234590", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        var usuario = new Usuario
        {
            Correo = $"ct.{token[..12].ToLowerInvariant()}@example.test", Nombre = "Usuario CT", RolId = rolId
        };
        var articulo = new Articulo { Numero = $"CT-{token[..16]}", Descripcion = "Fixture de asistencia" };
        db.Entry(entidad).State = EntityState.Added;
        db.Entry(usuario).State = EntityState.Added;
        db.Entry(articulo).State = EntityState.Added;
        await db.SaveChangesAsync();
        var perfil = new UsuarioEntidadAcademica { UsuarioId = usuario.Id, EntidadAcademicaId = entidad.Id };
        db.Entry(perfil).State = EntityState.Added;
        await db.SaveChangesAsync();
        var aviso = new Aviso
        {
            EntidadAcademicaId = entidad.Id, PeriodoEscolarId = periodo.Id, SistemaEducativoId = sistemaId,
            ArticuloId = articulo.Id, TipoComunicado = "AVISO", CreadoEn = DateTime.UtcNow,
            FechaPublicacion = new DateOnly(2026, 1, 2), UrlPublicacion = "https://example.test/aviso",
            Estado = "PUBLICADO"
        };
        db.Entry(aviso).State = EntityState.Added;
        await db.SaveChangesAsync();

        var service = new IntegranteConsejoTecnicoMvcService(new NormalizedIntegranteConsejoTecnicoMvcRepository(db));
        var nombre = $"Consejero {token}";
        var primerInicio = new DateOnly(2024, 1, 1);
        var nuevoInicio = new DateOnly(2026, 1, 1);
        try
        {
            await service.CrearAsync(usuario.Id, entidad.Id,
                new IntegranteConsejoTecnicoMvcCambio(nombre, "Director", tratamientoId, primerInicio, null));

            var page = await service.BuscarAsync(usuario.Id, entidad.Id,
                new IntegranteConsejoTecnicoMvcFiltro(token, 1, 20));
            var anterior = Assert.Single(page.Items);
            Assert.Equal(nombre, anterior.Nombre);
            Assert.Equal("Director", anterior.Cargo);
            Assert.False(anterior.TieneAsistencias);
            Assert.Contains(page.Tratamientos, x => x.Id == tratamientoId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearAsync(usuario.Id, entidad.Id,
                new IntegranteConsejoTecnicoMvcCambio(nombre, "Cargo duplicado", tratamientoId,
                    new DateOnly(2025, 1, 1), null)));

            await service.CambiarVigenciaAsync(usuario.Id, entidad.Id, anterior.Id,
                new IntegranteConsejoTecnicoMvcCambio(nombre, "Secretaria", tratamientoId, nuevoInicio, null));
            var historial = await db.IntegranteConsejoTecnicos.AsNoTracking()
                .Where(x => x.EntidadAcademicaId == entidad.Id).OrderBy(x => x.FechaInicio).ToListAsync();
            Assert.Equal(2, historial.Count);
            Assert.Equal(new DateOnly(2025, 12, 31), historial[0].FechaFin);
            Assert.Null(historial[1].FechaFin);
            Assert.Equal("Secretaria", historial[1].Cargo);

            var acta = new ActaConsejoTecnico
            {
                AvisoId = aviso.Id, EntidadAcademicaId = entidad.Id,
                Folio = $"CT-{token[..12]}", Fecha = new DateOnly(2026, 3, 1), Estado = "CREADA"
            };
            db.Entry(acta).State = EntityState.Added;
            await db.SaveChangesAsync();
            var asistencia = new ActaAsistencia
            {
                ActaConsejoTecnicoId = acta.Id, IntegranteConsejoTecnicoId = historial[1].Id,
                Nombre = nombre, Tratamiento = "Mtro.", Cargo = "Secretaria", Asistio = true, Firmo = false
            };
            db.Entry(asistencia).State = EntityState.Added;
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.EliminarCapturaErroneaAsync(usuario.Id, entidad.Id, historial[1].Id));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.BuscarAsync(usuario.Id,
                entidad.Id + 1, new IntegranteConsejoTecnicoMvcFiltro(null)));

            await service.EliminarCapturaErroneaAsync(usuario.Id, entidad.Id, historial[0].Id);
            Assert.False(await db.IntegranteConsejoTecnicos.AsNoTracking().AnyAsync(x => x.Id == historial[0].Id));

            db.ActaAsistencias.Remove(await db.ActaAsistencias.AsTracking().SingleAsync(x => x.Id == asistencia.Id));
            await db.SaveChangesAsync();
            db.ActaConsejoTecnicos.Remove(await db.ActaConsejoTecnicos.AsTracking().SingleAsync(x => x.Id == acta.Id));
            await db.SaveChangesAsync();
            await service.EliminarCapturaErroneaAsync(usuario.Id, entidad.Id, historial[1].Id);
            Assert.False(await db.IntegranteConsejoTecnicos.AsNoTracking().AnyAsync(x => x.EntidadAcademicaId == entidad.Id));
        }
        finally
        {
            db.ActaAsistencias.RemoveRange(await db.ActaAsistencias.AsTracking()
                .Where(x => x.ActaConsejoTecnicoId == db.ActaConsejoTecnicos.Where(a => a.AvisoId == aviso.Id)
                    .Select(a => a.Id).FirstOrDefault()).ToListAsync());
            await db.SaveChangesAsync();
            db.ActaConsejoTecnicos.RemoveRange(await db.ActaConsejoTecnicos.AsTracking()
                .Where(x => x.AvisoId == aviso.Id).ToListAsync());
            await db.SaveChangesAsync();
            db.IntegranteConsejoTecnicos.RemoveRange(await db.IntegranteConsejoTecnicos.AsTracking()
                .Where(x => x.EntidadAcademicaId == entidad.Id).ToListAsync());
            await db.SaveChangesAsync();
            if (aviso.Id > 0)
            {
                db.Avisos.Remove(await db.Avisos.AsTracking().SingleAsync(x => x.Id == aviso.Id));
                await db.SaveChangesAsync();
            }
            if (articulo.Id > 0) db.Articulos.Remove(await db.Articulos.AsTracking().SingleAsync(x => x.Id == articulo.Id));
            await db.SaveChangesAsync();
            db.UsuariosEntidadAcademica.RemoveRange(await db.UsuariosEntidadAcademica.AsTracking()
                .Where(x => x.UsuarioId == usuario.Id).ToListAsync());
            await db.SaveChangesAsync();
            db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuario.Id));
            db.EntidadAcademicas.Remove(await db.EntidadAcademicas.AsTracking().SingleAsync(x => x.Id == entidad.Id));
            db.PeriodosEscolares.Remove(await db.PeriodosEscolares.AsTracking().SingleAsync(x => x.Id == periodo.Id));
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
