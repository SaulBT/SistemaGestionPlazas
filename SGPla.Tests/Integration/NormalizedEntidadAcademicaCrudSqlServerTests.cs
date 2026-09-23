using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

public sealed class NormalizedEntidadAcademicaCrudSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Crud_completo_de_entidad_academica_funciona_contra_sql_server()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);
        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking()
            .Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.RegionId })
            .FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking()
            .Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();

        var repository = new NormalizedEntidadAcademicaMvcRepository(db);
        var service = new EntidadAcademicaMvcService(repository, TimeProvider.System);
        var clave = $"IT{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var input = new EntidadAcademicaMvcInputDto
        {
            Clave = clave,
            Nombre = "Entidad integración CRUD",
            Calle = "Avenida de prueba",
            NumeroExterior = "123",
            Colonia = "Centro",
            CodigoPostal = "91000",
            Telefono = "2281234567",
            Extension = "1234",
            CampusId = campus.Id,
            AreaAcademicaId = areaId,
            MunicipioId = municipioId
        };

        int id = 0;
        try
        {
            id = await service.CrearAsync(input);
            var created = await service.ObtenerPorIdAsync(id);
            Assert.NotNull(created);
            Assert.Equal(clave, created.Clave);
            Assert.Equal(campus.RegionId, created.RegionId);

            var listing = await service.BuscarAsync(
                new FiltroEntidadAcademicaMvcDto(campus.RegionId, areaId, clave, 1, 10));
            Assert.Contains(listing.Items, x => x.Id == id);

            var updated = new EntidadAcademicaMvcInputDto
            {
                Clave = clave,
                Nombre = "Entidad integración CRUD actualizada",
                Calle = "Calle actualizada",
                NumeroExterior = "456B",
                Colonia = "Nueva colonia",
                CodigoPostal = "91001",
                Telefono = "2287654321",
                Extension = null,
                CampusId = campus.Id,
                AreaAcademicaId = areaId,
                MunicipioId = municipioId
            };
            await service.ActualizarAsync(id, updated);
            Assert.Equal("Entidad integración CRUD actualizada", (await service.ObtenerPorIdAsync(id))?.Nombre);

            await service.EliminarAsync(id);
            Assert.Null(await service.ObtenerPorIdAsync(id));
        }
        finally
        {
            if (id > 0)
            {
                await db.EntidadAcademicas.IgnoreQueryFilters()
                    .Where(x => x.Id == id)
                    .ExecuteDeleteAsync();
            }
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
