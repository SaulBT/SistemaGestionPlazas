using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models;
using SGPla.Repositories.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedAreaAcademicaSequenceSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Crear_areas_concurrentemente_asigna_claves_unicas()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!;
        var options = new DbContextOptionsBuilder<SgplaDbContext>().UseSqlServer(connectionString).Options;
        await using (var verificationDb = new SgplaDbContext(options))
            Assert.Equal("GestionDePlazasBD", verificationDb.Database.GetDbConnection().Database);

        var marker = Guid.NewGuid().ToString("N");
        var nombres = new[] { $"Prueba secuencia A {marker}", $"Prueba secuencia B {marker}" };
        try
        {
            var created = await Task.WhenAll(nombres.Select(async nombre =>
            {
                await using var db = new SgplaDbContext(options);
                var repository = new NormalizedAreaAcademicaRepository(db);
                return await repository.CrearAsync(new AreaAcademica { Nombre = nombre });
            }));

            Assert.Equal(2, created.Select(x => x.IdAreaAcademica).Distinct().Count());
            var ids = created.Select(x => x.IdAreaAcademica).ToArray();
            await using var db = new SgplaDbContext(options);
            var claves = await db.AreaAcademicas.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => x.Clave)
                .ToListAsync();
            Assert.Equal(2, claves.Count);
            Assert.Equal(2, claves.Distinct().Count());
            Assert.All(claves, clave => Assert.True(clave > 0));
        }
        finally
        {
            await using var cleanupDb = new SgplaDbContext(options);
            await cleanupDb.AreaAcademicas
                .Where(x => nombres.Contains(x.Nombre))
                .ExecuteDeleteAsync();
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
