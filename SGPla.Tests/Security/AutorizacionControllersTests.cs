using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [InlineData(nameof(ProgramacionesAcademicasController.Ver), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(ProgramacionesAcademicasController.ProgramacionPlanea), PoliticasAutorizacion.Dgaa)]
    [InlineData(nameof(ProgramacionesAcademicasController.AprobarProgramacionPlanea), PoliticasAutorizacion.Dgaa)]
    [InlineData(nameof(ProgramacionesAcademicasController.RestaurarCopiaPlanea), PoliticasAutorizacion.Dgaa)]
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

    [Theory]
    [InlineData(nameof(ProgramacionesAcademicasController.AprobarProgramacionPlanea))]
    [InlineData(nameof(ProgramacionesAcademicasController.RestaurarCopiaPlanea))]
    public void AccionesPostDeAprobacionPlanea_ValidanTokenAntiforgery(string action)
    {
        var method = typeof(ProgramacionesAcademicasController).GetMethods()
            .Single(m => m.Name == action);

        Assert.NotEmpty(method.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true));
    }

    [Fact]
    public void Login_NoExponeCambioDeModo()
    {
        Assert.Null(typeof(LoginController).GetMethod("CambiarModo"));
    }
}
