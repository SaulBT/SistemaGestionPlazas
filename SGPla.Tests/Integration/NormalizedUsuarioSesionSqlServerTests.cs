using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedUsuarioSesionSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Revalida_cuenta_rol_y_ambito_en_el_esquema_normalizado()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!).Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var tokenCorreo = token.ToLowerInvariant();
        var entidad = new EntidadAcademica
        {
            Clave = $"SES{token[..12]}", Nombre = $"__SGPLA_TEST_SESION__{token}", Calle = "Calle",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        var dgaa = new Usuario { Correo = $"dgaa-{tokenCorreo}@integration.invalid", Nombre = "DGAA de prueba", RolId = 2 };
        var ea = new Usuario { Correo = $"ea-{tokenCorreo}@integration.invalid", Nombre = "EA de prueba", RolId = 3 };
        var superusuario = new Usuario { Correo = $"super-{tokenCorreo}@integration.invalid", Nombre = "Superusuario de prueba", RolId = 1 };

        try
        {
            db.EntidadAcademicas.Add(entidad);
            await db.SaveChangesAsync();
            db.Usuarios.AddRange(dgaa, ea, superusuario);
            await db.SaveChangesAsync();
            db.UsuariosDgaa.Add(new UsuarioDgaa { UsuarioId = dgaa.Id, AreaAcademicaId = areaId });
            db.UsuariosEntidadAcademica.Add(new UsuarioEntidadAcademica { UsuarioId = ea.Id, EntidadAcademicaId = entidad.Id });
            db.CredencialSuperusuarios.Add(new CredencialSuperusuario
            {
                UsuarioId = superusuario.Id, Contrasena = "$argon2id$v=19$fixture", FechaActualizacion = null
            });
            await db.SaveChangesAsync();

            var validator = new NormalizedUsuarioSesionValidator(db);
            Assert.True(await validator.EsSesionVigenteAsync(Principal(dgaa.Id, Constantes.COORDINADOR_DGAA,
                (Constantes.ID_AREA_ACADEMICA, areaId))));
            Assert.False(await validator.EsSesionVigenteAsync(Principal(dgaa.Id, Constantes.COORDINADOR_DGAA,
                (Constantes.ID_AREA_ACADEMICA, areaId + 100000))));

            Assert.True(await validator.EsSesionVigenteAsync(Principal(ea.Id, Constantes.COORDINADOR_EA,
                ("EntidadAcademicaId", entidad.Id))));
            Assert.False(await validator.EsSesionVigenteAsync(Principal(ea.Id, Constantes.COORDINADOR_EA,
                ("EntidadAcademicaId", entidad.Id + 100000))));
            Assert.False(await validator.EsSesionVigenteAsync(Principal(ea.Id, Constantes.COORDINADOR_EA,
                (Constantes.ID_AREA_ACADEMICA, areaId))));

            Assert.True(await validator.EsSesionVigenteAsync(Principal(superusuario.Id, Constantes.SUPERUSUARIO,
                ("DebeCambiarContrasena", 1))));
            Assert.False(await validator.EsSesionVigenteAsync(Principal(superusuario.Id, Constantes.COORDINADOR_EA,
                ("EntidadAcademicaId", entidad.Id))));

            Assert.False(await validator.EsSesionVigenteAsync(Principal(superusuario.Id, Constantes.SUPERUSUARIO,
                ("DebeCambiarContrasena", 0))));

            ea.FechaEliminacion = TimeProvider.System.GetUtcNow().UtcDateTime;
            await db.SaveChangesAsync();
            Assert.False(await validator.EsSesionVigenteAsync(Principal(ea.Id, Constantes.COORDINADOR_EA,
                ("EntidadAcademicaId", entidad.Id))));
        }
        finally
        {
            if (dgaa.Id > 0)
                await db.UsuariosDgaa.IgnoreQueryFilters().Where(x => x.UsuarioId == dgaa.Id).ExecuteDeleteAsync();
            if (ea.Id > 0)
                await db.UsuariosEntidadAcademica.IgnoreQueryFilters().Where(x => x.UsuarioId == ea.Id).ExecuteDeleteAsync();
            if (superusuario.Id > 0)
                await db.CredencialSuperusuarios.IgnoreQueryFilters().Where(x => x.UsuarioId == superusuario.Id).ExecuteDeleteAsync();
            var userIds = new[] { dgaa.Id, ea.Id, superusuario.Id }.Where(x => x > 0).ToArray();
            if (userIds.Length > 0)
                await db.Usuarios.IgnoreQueryFilters().Where(x => userIds.Contains(x.Id)).ExecuteDeleteAsync();
            if (entidad.Id > 0)
                await db.EntidadAcademicas.IgnoreQueryFilters().Where(x => x.Id == entidad.Id).ExecuteDeleteAsync();
        }
    }

    private static ClaimsPrincipal Principal(int usuarioId, string rol, params (string Tipo, int Valor)[] ambitos)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Role, rol)
        };
        if (rol == Constantes.SUPERUSUARIO && ambitos.All(x => x.Tipo != "DebeCambiarContrasena"))
            claims.Add(new Claim("DebeCambiarContrasena", "false"));
        claims.AddRange(ambitos.Select(x => new Claim(x.Tipo,
            x.Tipo == "DebeCambiarContrasena" ? (x.Valor == 1 ? "true" : "false") : x.Valor.ToString())));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test-cookie"));
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
