using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Repositories.Implementations;

namespace SGPla.Tests.Integration;

public sealed class NormalizedUsuarioConsultaSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Buscar_une_filtra_y_pagina_usuarios_en_sql()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!;
        var options = new DbContextOptionsBuilder<SgplaDbContext>().UseSqlServer(connectionString).Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).FirstAsync();
        var region = await db.Regiones.AsNoTracking().Where(x => x.Id == campus.RegionId)
            .Select(x => new { x.Clave, x.Nombre }).SingleAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null)
            .OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var token = Guid.NewGuid().ToString("N");
        await using var transaction = await db.Database.BeginTransactionAsync();
        var entidad = new EntidadAcademica
        {
            Clave = $"IT{token[..10]}".ToUpperInvariant(),
            Nombre = $"Entidad integración {token}",
            Calle = "Calle de prueba",
            Colonia = "Centro",
            CodigoPostal = "91000",
            Telefono = "2281234567",
            CampusId = campus.Id,
            AreaAcademicaId = areaId,
            MunicipioId = municipioId
        };
        db.EntidadAcademicas.Add(entidad);
        await db.SaveChangesAsync();
        var ea = new Usuario { Nombre = $"A {token}", Correo = $"a-{token}@integration.invalid", RolId = 3 };
        var dgaa = new Usuario { Nombre = $"B {token}", Correo = $"b-{token}@integration.invalid", RolId = 2 };
        db.Usuarios.AddRange(ea, dgaa);
        await db.SaveChangesAsync();
        db.UsuariosEntidadAcademica.Add(new UsuarioEntidadAcademica
        {
            UsuarioId = ea.Id,
            EntidadAcademicaId = entidad.Id
        });
        db.UsuariosDgaa.Add(new UsuarioDgaa
        {
            UsuarioId = dgaa.Id,
            AreaAcademicaId = areaId
        });
        await db.SaveChangesAsync();

        var repository = new NormalizedUsuarioConsultaRepository(db);
        var firstPage = await repository.BuscarPaginadoAsync(new FiltrosUsuarioDTO
        {
            Busqueda = token,
            Pagina = 1,
            Cantidad = 1
        });
        var secondPage = await repository.BuscarPaginadoAsync(new FiltrosUsuarioDTO
        {
            Busqueda = token,
            Pagina = 2,
            Cantidad = 1
        });

        Assert.Equal(2, firstPage.TotalCount);
        Assert.Single(firstPage.Items);
        Assert.Single(secondPage.Items);
        Assert.Equal($"A {token}", firstPage.Items[0].Nombre);
        Assert.Equal($"B {token}", secondPage.Items[0].Nombre);

        var filtradoPorAmbito = await repository.BuscarPaginadoAsync(new FiltrosUsuarioDTO
        {
            Busqueda = token,
            Region = $"{region.Clave}-{region.Nombre}",
            IdAreaAcademica = areaId,
            IdEntidadAcademica = entidad.Id,
            Pagina = 1,
            Cantidad = 10
        });
        Assert.Equal(2, filtradoPorAmbito.TotalCount);
        Assert.Contains(filtradoPorAmbito.Items, x => x.Nombre == $"A {token}");
        Assert.Contains(filtradoPorAmbito.Items, x => x.Nombre == $"B {token}");
        await transaction.RollbackAsync();
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
