using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedSuperusuarioAdminSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task RestableceConCambioObligatorio_YProtegeAlUltimoSuperusuario()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N");
        var actor = new Usuario
        {
            Correo = $"admin.{token}@integration.invalid", Nombre = "Admin superusuario", RolId = 1
        };
        var objetivo = new Usuario
        {
            Correo = $"target.{token}@integration.invalid", Nombre = "Objetivo superusuario", RolId = 1
        };
        db.Entry(actor).State = EntityState.Added;
        db.Entry(objetivo).State = EntityState.Added;
        await db.SaveChangesAsync();

        var hasher = new Argon2idPasswordHasher();
        db.Entry(new CredencialSuperusuario
        {
            UsuarioId = actor.Id, Contrasena = hasher.Hash("OldAdmin!2026"), FechaActualizacion = DateTime.UtcNow
        }).State = EntityState.Added;
        db.Entry(new CredencialSuperusuario
        {
            UsuarioId = objetivo.Id, Contrasena = hasher.Hash("OldTarget!2026"), FechaActualizacion = DateTime.UtcNow
        }).State = EntityState.Added;
        await db.SaveChangesAsync();

        var service = new SuperusuarioAdminService(db, hasher, TimeProvider.System);
        try
        {
            var listado = await service.ListarAsync(actor.Id);
            Assert.Contains(listado, x => x.Id == objetivo.Id && x.TieneCredencial);

            const string temporal = "Temporal!2026";
            await service.RestablecerContrasenaTemporalAsync(actor.Id, objetivo.Id, temporal);
            var restablecida = await db.CredencialSuperusuarios.AsNoTracking()
                .SingleAsync(x => x.UsuarioId == objetivo.Id);
            Assert.Null(restablecida.FechaActualizacion);
            Assert.True(hasher.Verify(temporal, restablecida.Contrasena).EsCorrecta);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.RestablecerContrasenaTemporalAsync(actor.Id, objetivo.Id, "weak"));
            await service.DesactivarAsync(actor.Id, objetivo.Id);
            Assert.NotNull((await db.Usuarios.AsNoTracking().SingleAsync(x => x.Id == objetivo.Id)).FechaEliminacion);
            Assert.NotNull((await db.CredencialSuperusuarios.AsNoTracking().SingleAsync(x => x.UsuarioId == objetivo.Id)).FechaEliminacion);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DesactivarAsync(actor.Id, actor.Id));
            Assert.Null((await db.Usuarios.AsNoTracking().SingleAsync(x => x.Id == actor.Id)).FechaEliminacion);
        }
        finally
        {
            await db.CredencialSuperusuarios.IgnoreQueryFilters()
                .Where(x => x.UsuarioId == actor.Id || x.UsuarioId == objetivo.Id).ExecuteDeleteAsync();
            await db.Usuarios.IgnoreQueryFilters()
                .Where(x => x.Id == actor.Id || x.Id == objetivo.Id).ExecuteDeleteAsync();
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
