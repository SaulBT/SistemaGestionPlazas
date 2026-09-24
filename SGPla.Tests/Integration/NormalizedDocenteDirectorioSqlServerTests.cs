using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Docentes;
using SGPla.Repositories.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedDocenteDirectorioSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Buscar_filtra_y_pagina_directorio_normalizado()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N")[..18].ToUpperInvariant();
        var nombre = $"Docente {token}";
        var docentes = new[]
        {
            new Docente { Nombre = $"{nombre} A", NumPersonal = $"{token}A" },
            new Docente { Nombre = $"{nombre} B", NumPersonal = $"{token}B" }
        };
        foreach (var docente in docentes) db.Entry(docente).State = EntityState.Added;
        await db.SaveChangesAsync();

        try
        {
            var repository = new NormalizedDocenteDirectorioMvcRepository(db);
            var resultado = await repository.BuscarAsync(new DocenteDirectorioMvcFiltro(token, Pagina: 1, TamanoPagina: 1));
            Assert.Equal(2, resultado.Total);
            var docente = Assert.Single(resultado.Items);
            Assert.StartsWith(nombre, docente.Nombre, StringComparison.Ordinal);
            Assert.NotNull(docente.NumeroPersonal);
        }
        finally
        {
            await db.Docentes.Where(x => x.NumPersonal != null && x.NumPersonal.StartsWith(token)).ExecuteDeleteAsync();
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
