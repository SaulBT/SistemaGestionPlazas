using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Models.DTOs.Catalogos;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedCatalogosAdministracionSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task AdministraClasificacionesYCongelaCatalogosPermanentesConReferencias()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!).Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);
        var token = Guid.NewGuid().ToString("N");
        var nivelClave = ("LVL" + token.Substring(0, 12)).ToUpperInvariant();
        var areaClave = ("AF" + token.Substring(0, 12)).ToUpperInvariant();
        var planCodigo = $"P{token[..18]}".ToUpperInvariant();
        var service = new AdministracionCatalogosMvcService(db, TimeProvider.System);
        var entidadId = await db.EntidadAcademicas.AsNoTracking()
            .Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var grado = new GradoAcademico { Nombre = $"Grado {token}" };
        db.Entry(grado).State = EntityState.Added;
        await db.SaveChangesAsync();
        var tratamiento = new TratamientoAcademico { Nombre = $"Tr {token[..18]}", GradoAcademicoId = grado.Id };
        db.Entry(tratamiento).State = EntityState.Added;
        await db.SaveChangesAsync();
        try
        {
            await service.CrearClasificacionAsync(new CrearCatalogoClasificacionMvcDto
            {
                Tipo = TipoCatalogoNormalizado.SistemaEducativo, Nombre = $"Sistema {token}"
            });
            await service.CrearClasificacionAsync(new CrearCatalogoClasificacionMvcDto
            {
                Tipo = TipoCatalogoNormalizado.NivelFormacion, Clave = nivelClave, Nombre = $"Nivel {token}"
            });
            await service.CrearClasificacionAsync(new CrearCatalogoClasificacionMvcDto
            {
                Tipo = TipoCatalogoNormalizado.AreaFormacion, Clave = areaClave, Nombre = $"Área {token}"
            });

            var sistema = await db.SistemasEducativos.AsTracking().SingleAsync(x => x.Nombre == $"Sistema {token}");
            var nivel = await db.NivelesFormacion.AsTracking().SingleAsync(x => x.Clave == nivelClave);
            var area = await db.AreaFormaciones.AsTracking().SingleAsync(x => x.Clave == areaClave);
            var programa = new ProgramaEducativo
            {
                Nombre = $"Programa {token}", EntidadAcademicaId = entidadId,
                SistemaEducativoId = sistema.Id, NivelFormacionId = nivel.Id
            };
            db.Entry(programa).State = EntityState.Added;
            await db.SaveChangesAsync();
            var plan = new PlanEstudios { Codigo = planCodigo, ProgramaEducativoId = programa.Id };
            db.Entry(plan).State = EntityState.Added;
            await db.SaveChangesAsync();
            var experiencia = new ExperienciaEducativa
            {
                Nombre = $"Experiencia {token}", MateriaEe = $"M{token[..15]}".ToUpperInvariant(),
                CursoEe = "C1", HorasTeoricas = 0, HorasPracticas = 0, Creditos = 1,
                AreaFormacionId = area.Id, PlanEstudiosId = plan.Id
            };
            db.Entry(experiencia).State = EntityState.Added;
            await db.SaveChangesAsync();
            await service.EditarNombreAsync(new EditarNombreCatalogoMvcDto
            { Tipo = TipoCatalogoNormalizado.SistemaEducativo, Id = sistema.Id, Nombre = $"Sistema corregido {token}" });
            await service.EditarNombreAsync(new EditarNombreCatalogoMvcDto
            { Tipo = TipoCatalogoNormalizado.NivelFormacion, Id = nivel.Id, Nombre = $"Nivel corregido {token}" });
            await service.EditarNombreAsync(new EditarNombreCatalogoMvcDto
            { Tipo = TipoCatalogoNormalizado.AreaFormacion, Id = area.Id, Nombre = $"Área corregida {token}" });
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarActivoAsync(
                TipoCatalogoNormalizado.SistemaEducativo, sistema.Id, activo: false));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarActivoAsync(
                TipoCatalogoNormalizado.NivelFormacion, nivel.Id, activo: false));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CambiarActivoAsync(
                TipoCatalogoNormalizado.AreaFormacion, area.Id, activo: false));
            Assert.Null(await db.SistemasEducativos.AsNoTracking().Where(x => x.Id == sistema.Id)
                .Select(x => x.FechaEliminacion).SingleAsync());
            await db.ProgramasEducativos.Where(x => x.Id == programa.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.FechaEliminacion, DateTime.UtcNow));
            await db.ExperienciasEducativas.Where(x => x.Id == experiencia.Id).ExecuteDeleteAsync();
            await db.PlanesEstudios.Where(x => x.Id == plan.Id).ExecuteDeleteAsync();
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.SistemaEducativo, sistema.Id, activo: false);
            var fechaBaja = await db.SistemasEducativos.AsNoTracking().Where(x => x.Id == sistema.Id)
                .Select(x => x.FechaEliminacion).SingleAsync();
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.SistemaEducativo, sistema.Id, activo: false);
            Assert.Equal(fechaBaja, await db.SistemasEducativos.AsNoTracking().Where(x => x.Id == sistema.Id)
                .Select(x => x.FechaEliminacion).SingleAsync());
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.NivelFormacion, nivel.Id, activo: false);
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.AreaFormacion, area.Id, activo: false);
            Assert.All(await db.SistemasEducativos.AsNoTracking().Where(x => x.Id == sistema.Id).ToListAsync(), x => Assert.NotNull(x.FechaEliminacion));
            Assert.All(await db.NivelesFormacion.AsNoTracking().Where(x => x.Id == nivel.Id).ToListAsync(), x => Assert.NotNull(x.FechaEliminacion));
            Assert.All(await db.AreaFormaciones.AsNoTracking().Where(x => x.Id == area.Id).ToListAsync(), x => Assert.NotNull(x.FechaEliminacion));
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.SistemaEducativo, sistema.Id, activo: true);
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.NivelFormacion, nivel.Id, activo: true);
            await service.CambiarActivoAsync(TipoCatalogoNormalizado.AreaFormacion, area.Id, activo: true);

            var listado = await service.ListarAsync();
            Assert.Contains(listado, x => x.Tipo == TipoCatalogoNormalizado.TratamientoAcademico
                && x.Id == tratamiento.Id && x.Detalle == grado.Nombre);
            Assert.Equal(1, Assert.Single(listado, x => x.Tipo == TipoCatalogoNormalizado.GradoAcademico && x.Id == grado.Id).Referencias);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditarNombreAsync(
                new EditarNombreCatalogoMvcDto
                { Tipo = TipoCatalogoNormalizado.GradoAcademico, Id = grado.Id, Nombre = $"Grado cambiado {token}" }));
            Assert.Equal($"Grado {token}", await db.GradoAcademicos.AsNoTracking()
                .Where(x => x.Id == grado.Id).Select(x => x.Nombre).SingleAsync());
            await Assert.ThrowsAsync<ArgumentException>(() => service.CambiarActivoAsync(
                TipoCatalogoNormalizado.GradoAcademico, grado.Id, activo: false));
        }
        finally
        {
            await db.ExperienciasEducativas.Where(x => x.AreaFormacionId > 0 && x.Nombre == $"Experiencia {token}")
                .ExecuteDeleteAsync();
            await db.PlanesEstudios.Where(x => x.Codigo == planCodigo).ExecuteDeleteAsync();
            await db.ProgramasEducativos.Where(x => x.Nombre == $"Programa {token}").ExecuteDeleteAsync();
            await db.TratamientosAcademicos.Where(x => x.Id == tratamiento.Id).ExecuteDeleteAsync();
            await db.GradoAcademicos.Where(x => x.Id == grado.Id).ExecuteDeleteAsync();
            await db.SistemasEducativos.Where(x => x.Nombre == $"Sistema {token}" || x.Nombre == $"Sistema corregido {token}")
                .ExecuteDeleteAsync();
            await db.NivelesFormacion.Where(x => x.Clave == nivelClave).ExecuteDeleteAsync();
            await db.AreaFormaciones.Where(x => x.Clave == areaClave).ExecuteDeleteAsync();
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
