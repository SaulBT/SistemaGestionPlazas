using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
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
    public void AdministracionSuperusuarios_ExigeSesionLocalYProtegeSusEscrituras()
    {
        var policy = Assert.Single(typeof(AdministracionSuperusuariosController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(PoliticasAutorizacion.SuperUsuario, policy.Policy);
        foreach (var actionName in new[] { "Restablecer", "Desactivar" })
        {
            var action = typeof(AdministracionSuperusuariosController).GetMethods().Single(x => x.Name == actionName);
            Assert.NotNull(action.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true).SingleOrDefault());
            Assert.NotNull(action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        }
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
        var uploadSigned = typeof(AvisosMvcController).GetMethod("SubirDocumentoFirmado")!;
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica,
            Assert.Single(uploadSigned.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(uploadSigned.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        var publish = typeof(AvisosMvcController).GetMethod("Publicar",
            [typeof(int), typeof(DateOnly), typeof(string), typeof(CancellationToken)])!;
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica,
            Assert.Single(publish.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(publish.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        var cancel = typeof(AvisosMvcController).GetMethod("Cancelar",
            [typeof(int), typeof(string), typeof(CancellationToken)])!;
        Assert.Equal(PoliticasAutorizacion.Dgaa,
            Assert.Single(cancel.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(cancel.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        var archive = typeof(AvisosMvcController).GetMethod("Archivar",
            [typeof(int), typeof(CancellationToken)])!;
        Assert.NotNull(archive.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
        var deleteDraft = typeof(AvisosMvcController).GetMethod("EliminarBorrador",
            [typeof(int), typeof(CancellationToken)])!;
        Assert.Equal(PoliticasAutorizacion.EntidadAcademica,
            Assert.Single(deleteDraft.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()).Policy);
        Assert.NotNull(deleteDraft.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true).SingleOrDefault());
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

    [Fact]
    public void ControladoresMvcNormalizados_ProtegenTodasLasAccionesYPosts()
    {
        Type[] controladores =
        [
            typeof(UsuariosController),
            typeof(RegionesCampusController),
            typeof(CatalogosController),
            typeof(EntidadesAcademicasController),
            typeof(ProgramasEducativosController),
            typeof(PlanesEstudiosController),
            typeof(ProgramacionesAcademicasMvcController),
            typeof(OfertasMvcController),
            typeof(AvisosMvcController),
            typeof(DocentesMvcController),
            typeof(IntegranteConsejoTecnicoMvcController),
            typeof(SolicitudesMvcController),
            typeof(AdministracionSuperusuariosController),
            typeof(CuentaController)
        ];

        foreach (var controlador in controladores)
        {
            var politicaDeControlador = controlador.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any();
            var metodosAccion = controlador.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(metodo => metodo.ReturnType == typeof(Task<IActionResult>)
                    || typeof(IActionResult).IsAssignableFrom(metodo.ReturnType));

            Assert.NotEmpty(metodosAccion);
            foreach (var accion in metodosAccion)
            {
                Assert.True(politicaDeControlador || accion.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any(),
                    $"{controlador.Name}.{accion.Name} no exige autenticación/autorización.");

                if (accion.GetCustomAttributes<HttpPostAttribute>(inherit: true).Any())
                {
                    Assert.True(accion.GetCustomAttributes<ValidateAntiForgeryTokenAttribute>(inherit: true).Any(),
                        $"{controlador.Name}.{accion.Name} acepta POST sin antiforgery.");
                }
            }
        }
    }
}
