using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models;
using SGPla.Repositories.Implementations;
using NewEntidadAcademica = SGPla.Data.NewModel.Entities.EntidadAcademica;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedCatalogRepositoriesSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Articulos_periodos_y_usuarios_mvc_operan_sobre_sus_esquemas_normalizados()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!).Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var region = await db.Regiones.AsNoTracking().Where(x => x.Id == campus.RegionId).Select(x => new { x.Clave, x.Nombre }).SingleAsync();
        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var articleNumber = $"IT{token[..18]}";
        var entity = new NewEntidadAcademica
        {
            Clave = $"IT{token[..10]}", Nombre = $"Entidad usuario {token}", Calle = "Calle de prueba",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        db.Entry(entity).State = EntityState.Added;
        await db.SaveChangesAsync();

        var articleRepo = new NormalizedArticuloRepository(db);
        var periodRepo = new NormalizedPeriodoEscolarRepository(db, TimeProvider.System);
        var dgaaRepo = new NormalizedCoordinadorDgaaRepository(db);
        var eaRepo = new NormalizedCoordinadorEaRepository(db);
        var articleId = 0;
        var periodId = 0;
        var dgaaId = 0;
        var eaId = 0;
        try
        {
            var article = await articleRepo.CrearArticuloAsync(new SGPla.Models.Articulo { Numero = articleNumber, Descripcion = "Artículo de prueba integración" });
            articleId = article.IdArticulo;
            Assert.Equal(articleNumber, (await articleRepo.ObtenerArticuloPorIdAsync(articleId))?.Numero);
            Assert.Single(await articleRepo.BuscarPorTerminoAsync(token[..8]));
            article.Descripcion = "Artículo actualizado";
            Assert.Equal("Artículo actualizado", (await articleRepo.ActualizarArticuloAsync(article))?.Descripcion);

            var code = await NuevoCodigoPeriodoAsync(db);
            var period = await periodRepo.CrearAsync(new Periodo { Codigo = code });
            periodId = period.IdPeriodo;
            Assert.Equal(code, (await periodRepo.ObtenerPorIdAsync(periodId))?.Codigo);
            await Assert.ThrowsAsync<InvalidOperationException>(() => periodRepo.ActualizarAsync(new Periodo
            {
                IdPeriodo = periodId, Codigo = code[..4] + (code.EndsWith("51", StringComparison.Ordinal) ? "01" : "51")
            }));
            Assert.True(await periodRepo.EliminarAsync(periodId));
            Assert.Null(await periodRepo.ObtenerPorIdAsync(periodId));

            dgaaId = await dgaaRepo.CrearAsync(new CoordinadorDgaa
            {
                Nombre = $"DGAA {token}", Correo = $"DGAA-{token}@integration.invalid", IdAreaAcademica = areaId
            });
            eaId = await eaRepo.CrearAsync(new CoordinadorEa
            {
                Nombre = $"EA {token}", Correo = $"EA-{token}@integration.invalid", IdEntidadAcademica = entity.Id
            });
            Assert.True(await dgaaRepo.ExisteCorreoAsync($"dgaa-{token}@integration.invalid"));
            Assert.Contains(await dgaaRepo.BuscarConFiltros(areaId, token), x => x.IdCoordinadorDgaa == dgaaId);
            Assert.Contains(await eaRepo.BuscarConFiltros($"{region.Clave}-{region.Nombre}", areaId, entity.Id, token), x => x.IdCoordinadorEa == eaId);

            await dgaaRepo.ActualizarAsync(new CoordinadorDgaa { IdCoordinadorDgaa = dgaaId, Nombre = $"DGAA editado {token}", IdAreaAcademica = areaId });
            await eaRepo.ActualizarAsync(new CoordinadorEa { IdCoordinadorEa = eaId, Nombre = $"EA editado {token}", IdEntidadAcademica = entity.Id });
            Assert.Equal($"DGAA editado {token}", (await dgaaRepo.ObtenerPorIdAsync(dgaaId))?.Nombre);
            Assert.Equal($"EA editado {token}", (await eaRepo.ObtenerPorIdAsync(eaId))?.Nombre);

            entity.FechaEliminacion = TimeProvider.System.GetUtcNow().UtcDateTime;
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => eaRepo.ActualizarAsync(new CoordinadorEa
            {
                IdCoordinadorEa = eaId, Nombre = $"EA inválida {token}", IdEntidadAcademica = entity.Id
            }));
            var storedUser = await db.Usuarios.AsNoTracking().SingleAsync(x => x.Id == eaId);
            var storedProfile = await db.UsuariosEntidadAcademica.AsNoTracking()
                .SingleAsync(x => x.UsuarioId == eaId);
            Assert.Equal($"EA editado {token}", storedUser.Nombre);
            Assert.Equal(entity.Id, storedProfile.EntidadAcademicaId);

            await dgaaRepo.EliminarAsync(new CoordinadorDgaa { IdCoordinadorDgaa = dgaaId });
            await eaRepo.EliminarAsync(new CoordinadorEa { IdCoordinadorEa = eaId });
            Assert.Null(await dgaaRepo.ObtenerPorIdAsync(dgaaId));
            Assert.Null(await eaRepo.ObtenerPorIdAsync(eaId));

            Assert.True(await articleRepo.EliminarArticuloAsync(articleId));
            Assert.Null(await articleRepo.ObtenerArticuloPorIdAsync(articleId));
        }
        finally
        {
            if (articleId > 0) await db.Articulos.IgnoreQueryFilters().Where(x => x.Id == articleId).ExecuteDeleteAsync();
            if (periodId > 0) await db.PeriodosEscolares.IgnoreQueryFilters().Where(x => x.Id == periodId).ExecuteDeleteAsync();
            var userIds = new[] { dgaaId, eaId }.Where(x => x > 0).ToArray();
            if (userIds.Length > 0)
            {
                await db.UsuariosDgaa.IgnoreQueryFilters().Where(x => userIds.Contains(x.UsuarioId)).ExecuteDeleteAsync();
                await db.UsuariosEntidadAcademica.IgnoreQueryFilters().Where(x => userIds.Contains(x.UsuarioId)).ExecuteDeleteAsync();
                await db.Usuarios.IgnoreQueryFilters().Where(x => userIds.Contains(x.Id)).ExecuteDeleteAsync();
            }
            await db.EntidadAcademicas.IgnoreQueryFilters().Where(x => x.Id == entity.Id).ExecuteDeleteAsync();
        }
    }

    private static async Task<string> NuevoCodigoPeriodoAsync(SgplaDbContext db)
    {
        var year = DateTime.UtcNow.Year + 20;
        for (var offset = 0; offset < 50; offset++)
        {
            var code = $"{year + offset:0000}51";
            if (!await db.PeriodosEscolares.IgnoreQueryFilters().AnyAsync(x => x.Clave == code)) return code;
        }
        throw new InvalidOperationException("No se encontró un código de periodo de prueba libre.");
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
