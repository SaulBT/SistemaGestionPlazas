using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Controllers;
using SGPla.Models.ViewModels.Avisos;

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

    [Fact]
    public void CambioDeContrasenaDeSuperusuario_ExigeRolYAntiforgery()
    {
        var authorize = Assert.Single(typeof(CuentaController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.SuperUsuario, authorize.Policy);
        var post = Assert.Single(typeof(CuentaController).GetMethods(), x =>
            x.Name == "CambiarContrasena" && x.GetParameters().Length == 2);
        Assert.NotNull(post.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void ProgramacionAcademicaUsaRutaMvcNuevaYDesactivaElControladorLegacy()
    {
        var route = typeof(ProgramacionesAcademicasMvcController)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>()
            .Single();
        Assert.Equal("ProgramacionesAcademicas", route.Template);
        Assert.NotNull(typeof(ProgramacionesAcademicasController)
            .GetCustomAttributes(typeof(NonControllerAttribute), inherit: true).SingleOrDefault());

        var importar = typeof(ProgramacionesAcademicasMvcController).GetMethod("Importar", [typeof(CancellationToken)]);
        Assert.NotNull(importar);
        var policy = Assert.Single(importar!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.Dgaa, policy.Policy);

        var sincronizar = typeof(ProgramacionesAcademicasMvcController).GetMethod("SincronizarPlanea");
        Assert.NotNull(sincronizar);
        var sincronizarPolicy = Assert.Single(sincronizar!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.Dgaa, sincronizarPolicy.Policy);
        Assert.NotNull(sincronizar.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void DirectorioDocentesUsaControladorNormalizadoYDesactivaLasEscriturasLegacy()
    {
        var ruta = typeof(DocentesMvcController).GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>().Single();
        Assert.Equal("Docentes", ruta.Template);
        Assert.NotNull(typeof(DocentesController).GetCustomAttributes(typeof(NonControllerAttribute), inherit: true).SingleOrDefault());
        var policy = Assert.Single(typeof(DocentesMvcController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica, policy.Policy);
        Assert.Single(typeof(DocentesMvcController).GetMethods(), x => x.Name == "Index");
    }

    [Fact]
    public void CrearOfertaMvc_RequiereAmbitoEntidadAcademica()
    {
        var authorize = Assert.Single(typeof(OfertasMvcController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica, authorize.Policy);
        var post = Assert.Single(typeof(OfertasMvcController).GetMethods(), x => x.Name == "Nueva" && x.GetParameters().Length == 3);
        Assert.NotNull(post.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void AvisosUsaControladorNormalizadoYProtegeLaCreacionDeBorradores()
    {
        var route = Assert.Single(typeof(AvisosMvcController)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: true).Cast<RouteAttribute>());
        Assert.Equal("Avisos", route.Template);
        var policy = Assert.Single(typeof(AvisosMvcController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.OperadorAcademico, policy.Policy);
        Assert.NotNull(typeof(AvisosController)
            .GetCustomAttributes(typeof(NonControllerAttribute), inherit: true).SingleOrDefault());

        var createPolicy = Assert.Single(typeof(AvisosMvcController).GetMethod("Nuevo",
                [typeof(CrearAvisoMvcViewModel), typeof(CancellationToken)])!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica, createPolicy.Policy);
        Assert.NotNull(typeof(AvisosMvcController).GetMethod("Nuevo",
                [typeof(CrearAvisoMvcViewModel), typeof(CancellationToken)])!
            .GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());

        var upload = typeof(AvisosMvcController).GetMethod("SubirDocumentoOriginal")!;
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica,
            Assert.Single(upload.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(upload.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        var submit = typeof(AvisosMvcController).GetMethod("EnviarARevision",
            [typeof(int), typeof(AvisoMvcDetalleViewModel), typeof(CancellationToken)])!;
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica,
            Assert.Single(submit.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(submit.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());

        var resolve = typeof(AvisosMvcController).GetMethod("ResolverRevision",
            [typeof(int), typeof(bool), typeof(string), typeof(CancellationToken)])!;
        Assert.Equal(PoliticasAutorizacion.Dgaa,
            Assert.Single(resolve.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(resolve.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(AvisosMvcController).GetMethod("DescargarDocumento")!
            .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void ConsejoTecnicoUsaAmbitoNormalizadoYProtegeEscrituras()
    {
        var route = Assert.Single(typeof(IntegranteConsejoTecnicoMvcController)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: true).Cast<RouteAttribute>());
        Assert.Equal("IntegranteCT", route.Template);
        var policy = Assert.Single(typeof(IntegranteConsejoTecnicoMvcController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica, policy.Policy);
        Assert.NotNull(typeof(IntegranteCtController)
            .GetCustomAttributes(typeof(NonControllerAttribute), inherit: true).SingleOrDefault());

        foreach (var actionName in new[] { "Crear", "NuevaVigencia", "EliminarCapturaErronea" })
        {
            var action = typeof(IntegranteConsejoTecnicoMvcController).GetMethod(actionName)!;
            Assert.NotNull(action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        }
    }
}
