using Moq;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Security;

public class AuthServiceTests
{
    private readonly Mock<ILdapAuthService> _ldap = new();
    private readonly Mock<ICoordinadorDgaaRepository> _dgaa = new();
    private readonly Mock<ICoordinadorEaRepository> _entidad = new();

    [Fact]
    public async Task LoginAsync_Superusuario_DevuelveSoloElRolSuperusuario()
    {
        const string correo = "super@uv.mx";
        ConfigurarLdapExitoso(correo);
        _dgaa.Setup(r => r.ObtenerSuperUsuarioPorCorreoAsync(correo)).ReturnsAsync(new SuperUsuario
        {
            IdSuperUsuario = 7,
            Correo = correo,
            Nombre = "Cuenta administrativa"
        });
        _dgaa.Setup(r => r.ExisteCorreoAsync(correo)).ReturnsAsync(false);
        _entidad.Setup(r => r.ExisteCorreoAsync(correo)).ReturnsAsync(false);

        var resultado = await CrearServicio().LoginAsync(correo, "secreto");

        Assert.True(resultado.Exitoso);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal(Constantes.SUPERUSUARIO, resultado.Usuario.Rol);
    }

    [Fact]
    public async Task LoginAsync_CoordinadorEntidad_IncluyeElAmbitoDeEntidad()
    {
        const string correo = "entidad@uv.mx";
        ConfigurarLdapExitoso(correo);
        _dgaa.Setup(r => r.ObtenerSuperUsuarioPorCorreoAsync(correo)).ReturnsAsync((SuperUsuario?)null);
        _dgaa.Setup(r => r.ExisteCorreoAsync(correo)).ReturnsAsync(false);
        _entidad.Setup(r => r.ExisteCorreoAsync(correo)).ReturnsAsync(true);
        _entidad.Setup(r => r.ObtenerPorCorreoAsync(correo)).ReturnsAsync(new CoordinadorEa
        {
            IdCoordinadorEa = 8,
            IdEntidadAcademica = 42,
            Correo = correo,
            Nombre = "Coordinación de entidad"
        });

        var resultado = await CrearServicio().LoginAsync(correo, "secreto");

        Assert.True(resultado.Exitoso);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal(Constantes.COORDINADOR_EA, resultado.Usuario.Rol);
        Assert.Equal(42, resultado.Usuario.EntidadAcademicaId);
    }

    private AuthService CrearServicio() => new(_ldap.Object, _dgaa.Object, _entidad.Object);

    private void ConfigurarLdapExitoso(string correo) =>
        _ldap.Setup(s => s.Autenticar(correo, It.IsAny<string>())).Returns(true);
}
