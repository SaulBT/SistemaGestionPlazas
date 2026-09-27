using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SGPla.Commons;
using SGPla.Controllers;

namespace SGPla.Tests.Security;

public sealed class MvcAuthorizationHttpPipelineTests
{
    [Theory]
    [InlineData("GET", "/Usuarios", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/Usuarios/CrearUsuario", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/Usuarios/EliminarUsuario/7", Constantes.COORDINADOR_DGAA)]
    [InlineData("GET", "/RegionesCampus", Constantes.COORDINADOR_DGAA)]
    [InlineData("GET", "/Catalogos/Campus", Constantes.COORDINADOR_EA)]
    [InlineData("GET", "/EntidadesAcademicas", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/EntidadesAcademicas/CrearEntidadAcademica", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/EntidadesAcademicas/EliminarEntidadAcademica/7", Constantes.COORDINADOR_EA)]
    [InlineData("GET", "/ProgramasEducativos", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/ProgramasEducativos/EliminarProgramaEducativo/7", Constantes.COORDINADOR_EA)]
    [InlineData("GET", "/PlanesEstudios", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/PlanesEstudios/CrearExperiencia", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/PlanesEstudios/EliminarExperiencia", Constantes.COORDINADOR_EA)]
    [InlineData("GET", "/ProgramacionesAcademicas", Constantes.SUPERUSUARIO)]
    [InlineData("GET", "/ProgramacionesAcademicas/Importar", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/ProgramacionesAcademicas/SincronizarPlanea", Constantes.COORDINADOR_EA)]
    [InlineData("GET", "/ProgramacionesAcademicas/EstadoSincronizacionPlanea/7", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/ProgramacionesAcademicas/123/Ofertas/Nueva", Constantes.COORDINADOR_DGAA)]
    [InlineData("GET", "/Avisos", Constantes.SUPERUSUARIO)]
    [InlineData("GET", "/Avisos/123/Solicitudes", Constantes.COORDINADOR_DGAA)]
    [InlineData("POST", "/Avisos/123/Solicitudes/Nueva/456", Constantes.COORDINADOR_DGAA)]
    [InlineData("GET", "/Avisos/123/Solicitudes/789/Documentos/42", Constantes.SUPERUSUARIO)]
    [InlineData("GET", "/Docentes", Constantes.SUPERUSUARIO)]
    [InlineData("GET", "/IntegranteCT", Constantes.COORDINADOR_DGAA)]
    [InlineData("GET", "/IntegranteCT", Constantes.SUPERUSUARIO)]
    [InlineData("GET", "/Administracion/Superusuarios", Constantes.COORDINADOR_EA)]
    [InlineData("POST", "/Cuenta/CambiarContrasena", Constantes.COORDINADOR_EA)]
    public async Task RutasMvc_ProhibenElRolEquivocadoEnElPipeline(string metodo, string ruta, string rol)
    {
        using var host = await CrearHostAsync();
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", rol);

        using var response = metodo == "POST"
            ? await client.PostAsync(ruta, new FormUrlEncodedContent([]))
            : await client.GetAsync(ruta);

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task RutaMvc_SinCookieNiSesionEsDesafiadaPorFallbackPolicy()
    {
        using var host = await CrearHostAsync();
        using var client = host.GetTestClient();

        using var response = await client.GetAsync("/Catalogos/Campus");

        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("/Usuarios/CrearUsuario", Constantes.SUPERUSUARIO)]
    [InlineData("/RegionesCampus/CrearRegion", Constantes.SUPERUSUARIO)]
    [InlineData("/Catalogos/CrearClasificacion", Constantes.SUPERUSUARIO)]
    [InlineData("/EntidadesAcademicas/CrearEntidadAcademica", Constantes.SUPERUSUARIO)]
    [InlineData("/EntidadesAcademicas/EliminarEntidadAcademica/7", Constantes.SUPERUSUARIO)]
    [InlineData("/ProgramasEducativos/CrearProgramaEducativo", Constantes.SUPERUSUARIO)]
    [InlineData("/PlanesEstudios/Crear", Constantes.SUPERUSUARIO)]
    [InlineData("/PlanesEstudios/CrearExperiencia", Constantes.SUPERUSUARIO)]
    [InlineData("/PlanesEstudios/EliminarExperiencia", Constantes.SUPERUSUARIO)]
    [InlineData("/PlanesEstudios/ImportarExcel", Constantes.SUPERUSUARIO)]
    [InlineData("/ProgramacionesAcademicas/Importar", Constantes.COORDINADOR_DGAA)]
    [InlineData("/ProgramacionesAcademicas/SincronizarPlanea", Constantes.COORDINADOR_DGAA)]
    [InlineData("/ProgramacionesAcademicas/123/Ofertas/Nueva", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/Nuevo", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/DocumentoOriginal", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/DocumentoFirmado", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/Publicar", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/Cancelar", Constantes.COORDINADOR_DGAA)]
    [InlineData("/Avisos/123/Solicitudes/Nueva/456", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/Solicitudes/789/Resolver", Constantes.COORDINADOR_EA)]
    [InlineData("/Avisos/123/Solicitudes/789/Documentos", Constantes.COORDINADOR_EA)]
    [InlineData("/IntegranteCT/Crear", Constantes.COORDINADOR_EA)]
    [InlineData("/IntegranteCT/7/NuevaVigencia", Constantes.COORDINADOR_EA)]
    [InlineData("/Administracion/Superusuarios/4/Desactivar", Constantes.SUPERUSUARIO)]
    [InlineData("/Cuenta/CambiarContrasena", Constantes.SUPERUSUARIO)]
    public async Task EscriturasMvc_AutenticadasSinTokenAntiforgerySeRechazan(string ruta, string rol)
    {
        using var host = await CrearHostAsync();
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", rol);

        using var response = await client.PostAsync(ruta, new FormUrlEncodedContent([]));

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static Task<IHost> CrearHostAsync() => new HostBuilder()
        .ConfigureWebHost(webHost => webHost
            .UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddControllersWithViews().AddApplicationPart(typeof(CatalogosController).Assembly);
                services.AddAuthentication("TestRole")
                    .AddScheme<AuthenticationSchemeOptions, TestRoleAuthenticationHandler>("TestRole", _ => { });
                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.AddPolicy(PoliticasAutorizacion.SuperUsuario, p => p.RequireRole(Constantes.SUPERUSUARIO));
                    options.AddPolicy(PoliticasAutorizacion.Dgaa, p => p.RequireRole(Constantes.COORDINADOR_DGAA));
                    options.AddPolicy(PoliticasAutorizacion.EntidadAcademica, p => p.RequireRole(Constantes.COORDINADOR_EA));
                    options.AddPolicy(PoliticasAutorizacion.OperadorAcademico,
                        p => p.RequireRole(Constantes.COORDINADOR_DGAA, Constantes.COORDINADOR_EA));
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                });
            })
        )
        .StartAsync();

    private sealed class TestRoleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestRoleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Role", out var role) || string.IsNullOrWhiteSpace(role))
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "7001"),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("EntidadAcademicaId", "7002"),
                new Claim("IdAreaAcademica", "7003")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
