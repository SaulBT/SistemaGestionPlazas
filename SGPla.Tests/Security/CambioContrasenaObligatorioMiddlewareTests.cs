using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Security;

public sealed class CambioContrasenaObligatorioMiddlewareTests
{
    [Fact]
    public async Task UsuarioConCambioObligatorio_NoAccedeAOtrasRutasMvc()
    {
        var context = CrearContexto("/Usuarios");
        var continuo = false;
        var middleware = new CambioContrasenaObligatorioMiddleware(_ =>
        {
            continuo = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.False(continuo);
        Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.Equal("/Cuenta/CambiarContrasena", context.Response.Headers.Location);
    }

    [Theory]
    [InlineData("/Cuenta/CambiarContrasena")]
    [InlineData("/Login/Logout")]
    [InlineData("/api/v1/articulos")]
    [InlineData("/css/site.css")]
    public async Task PermiteCambioLogoutApiYRecursosEstaticos(string path)
    {
        var context = CrearContexto(path);
        var continuo = false;
        var middleware = new CambioContrasenaObligatorioMiddleware(_ =>
        {
            continuo = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(continuo);
    }

    private static DefaultHttpContext CrearContexto(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = HttpMethods.Get;
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim("DebeCambiarContrasena", "true")], "test-cookie"));
        return context;
    }
}
