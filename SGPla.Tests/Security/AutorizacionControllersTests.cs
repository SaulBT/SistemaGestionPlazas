using Microsoft.AspNetCore.Authorization;
using SGPla.Commons;
using SGPla.Controllers;

namespace SGPla.Tests.Security;

public class AutorizacionControllersTests
{
    [Theory]
    [InlineData(typeof(UsuariosController), PoliticasAutorizacion.SuperUsuario)]
    [InlineData(typeof(ArticulosController), PoliticasAutorizacion.SuperUsuario)]
    [InlineData(typeof(DocentesController), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(typeof(IntegranteCtController), PoliticasAutorizacion.EntidadAcademica)]
    public void ControladoresDeRolExclusivo_ExigenLaPoliticaCorrecta(Type controller, string policy)
    {
        var attribute = controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(policy, attribute.Policy);
    }

    [Theory]
    [InlineData(nameof(ProgramacionesAcademicasController.CargarProgramacionAcademicaPaso1), PoliticasAutorizacion.Dgaa)]
    [InlineData(nameof(ProgramacionesAcademicasController.Ver), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(ProgramacionesAcademicasController.CambiarInclusionOferta), PoliticasAutorizacion.EntidadAcademica)]
    public void AccionesDeProgramacion_ExigenElRolDefinido(string action, string policy)
    {
        var method = typeof(ProgramacionesAcademicasController).GetMethods()
            .Single(m => m.Name == action);
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(policy, attribute.Policy);
    }

    [Fact]
    public void Login_NoExponeCambioDeModo()
    {
        Assert.Null(typeof(LoginController).GetMethod("CambiarModo"));
    }
}
