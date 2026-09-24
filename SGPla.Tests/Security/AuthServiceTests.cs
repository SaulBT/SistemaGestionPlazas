using Microsoft.EntityFrameworkCore;
using Moq;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Security;

[Collection("SQL Server integration")]
public sealed class AuthServiceTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Superusuario_usa_Argon2id_sin_ldap_y_obliga_el_cambio_de_contrasena()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!).Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N");
        var user = new Usuario
        {
            Correo = $"super-{token}@integration.invalid",
            Nombre = "Superusuario de integración",
            RolId = 1
        };
        var hasher = new Argon2idPasswordHasher();
        const string passwordTemporal = "Temporal-1234";
        const string passwordNueva = "Nueva-Valida-1234";
        var passwordHash = hasher.Hash(passwordTemporal);
        var ldap = new Mock<ILdapAuthService>();
        ldap.Setup(x => x.Autenticar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var dgaa = new Mock<ICoordinadorDgaaRepository>();
        var ea = new Mock<ICoordinadorEaRepository>();

        try
        {
            db.Usuarios.Add(user);
            await db.SaveChangesAsync();
            db.CredencialSuperusuarios.Add(new CredencialSuperusuario
            {
                UsuarioId = user.Id,
                Contrasena = passwordHash,
                FechaActualizacion = null
            });
            await db.SaveChangesAsync();

            var auth = new AuthService(db, hasher, ldap.Object, dgaa.Object, ea.Object, TimeProvider.System);
            var loginContrasenaIncorrecta = await auth.LoginAsync(user.Correo, "incorrecta");
            Assert.False(loginContrasenaIncorrecta.Exitoso);

            var login = await auth.LoginAsync(user.Correo, passwordTemporal);
            Assert.True(login.Exitoso);
            Assert.Equal(user.Id, login.Usuario?.Id);
            Assert.True(login.Usuario?.RequiereCambioContrasena);
            ldap.Verify(x => x.Autenticar(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            Assert.False(await auth.CambiarContrasenaSuperusuarioAsync(user.Id,
                "incorrecta", passwordNueva));
            Assert.True(await auth.CambiarContrasenaSuperusuarioAsync(user.Id,
                passwordTemporal, passwordNueva));

            var storedCredential = await db.CredencialSuperusuarios.AsNoTracking().SingleAsync(x => x.UsuarioId == user.Id);
            Assert.NotNull(storedCredential.FechaActualizacion);
            Assert.NotEqual(passwordHash, storedCredential.Contrasena);
            Assert.True(hasher.Verify(passwordNueva, storedCredential.Contrasena).EsCorrecta);

            var loginContrasenaNueva = await auth.LoginAsync(user.Correo, passwordNueva);
            Assert.True(loginContrasenaNueva.Exitoso);
            Assert.False(loginContrasenaNueva.Usuario?.RequiereCambioContrasena);
            Assert.False((await auth.LoginAsync(user.Correo, passwordTemporal)).Exitoso);
            ldap.Verify(x => x.Autenticar(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        finally
        {
            if (user.Id > 0)
            {
                await db.CredencialSuperusuarios.IgnoreQueryFilters()
                    .Where(x => x.UsuarioId == user.Id).ExecuteDeleteAsync();
                await db.Usuarios.IgnoreQueryFilters().Where(x => x.Id == user.Id).ExecuteDeleteAsync();
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
