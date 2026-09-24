using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedRegionCampusAdminSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Administra_nombres_y_bajas_de_region_campus_sin_cambiar_claves_o_dependencias()
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
            Nombre = $"Región prueba {token}"
        };
        db.Entry(region).State = EntityState.Added;
        await db.SaveChangesAsync();
        var regionId = region.Id;
        var campusClave = $"IT{token[..20]}";
        var repo = new NormalizedRegionCampusMvcRepository(db);
        var service = new RegionCampusMvcService(repo, TimeProvider.System);
        var campusId = 0;
        try
        {
            await service.CrearCampusAsync(new CrearCampusMvcDto
            {
                Clave = campusClave.ToLowerInvariant(), Nombre = $"Campus {token}", RegionId = regionId
            });
            campusId = await db.Campuses.AsNoTracking().Where(x => x.Clave == campusClave).Select(x => x.Id).SingleAsync();

            var listado = await service.ObtenerAsync();
            Assert.Contains(listado.Regiones, x => x.Id == regionId);
            Assert.Contains(listado.Campus, x => x.Id == campusId && x.RegionId == regionId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DarDeBajaRegionAsync(regionId));
            Assert.True(await service.EditarNombreCampusAsync(campusId, "Campus actualizado"));
            Assert.True(await service.EditarNombreRegionAsync(regionId, "Región actualizada"));

            Assert.True(await service.DarDeBajaCampusAsync(campusId));
            Assert.True(await service.DarDeBajaRegionAsync(regionId));
            await Assert.ThrowsAsync<ArgumentException>(() => service.CrearCampusAsync(new CrearCampusMvcDto
            {
                Clave = $"IX{token[..20]}", Nombre = "Campus región inactiva", RegionId = regionId
            }));
        }
        finally
        {
            if (campusId > 0)
                await db.Campuses.IgnoreQueryFilters().Where(x => x.Id == campusId).ExecuteDeleteAsync();
            await db.Regiones.IgnoreQueryFilters().Where(x => x.Id == regionId).ExecuteDeleteAsync();
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
