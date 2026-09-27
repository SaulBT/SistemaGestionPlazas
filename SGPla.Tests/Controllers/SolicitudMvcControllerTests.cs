using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SGPla.Controllers;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Models.ViewModels.Solicitudes;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Controllers;

public sealed class SolicitudMvcControllerTests
{
    private readonly Mock<ISolicitudMvcService> _service = new();

    private SolicitudesMvcController Crear(int? usuario = 7, int? entidad = 12)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, "Coordinador EA") };
        if (usuario.HasValue) claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.Value.ToString()));
        if (entidad.HasValue) claims.Add(new Claim("EntidadAcademicaId", entidad.Value.ToString()));
        var controller = new SolicitudesMvcController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
                }
            }
        };
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    [Fact]
    public async Task Registrar_UsaIdentidadYEntidadDeClaims()
    {
        _service.Setup(x => x.RegistrarAsync(7, 12, It.IsAny<RegistrarSolicitudMvcDatos>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(41);
        var controller = Crear();
        var modelo = new SolicitudMvcRegistroViewModel
        {
            AvisoId = 31,
            AvisoOfertaId = 51,
            Nombre = "Aspirante",
            Correo = "aspirante@example.org",
            DescripcionPerfil = "Perfil",
            Formaciones = [new FormacionSolicitudMvcViewModel { GradoAcademicoId = 2, Descripcion = "Doctorado" }]
        };

        var result = Assert.IsType<RedirectToActionResult>(await controller.Nueva(31, 51, modelo, default));

        Assert.Equal(nameof(SolicitudesMvcController.Detalle), result.ActionName);
        _service.Verify(x => x.RegistrarAsync(7, 12,
            It.Is<RegistrarSolicitudMvcDatos>(d => d.AvisoOfertaId == 51 && d.Correo == "aspirante@example.org"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Registrar_SinEntidadEnCookieDevuelveForbidSinPersistir()
    {
        var result = await Crear(entidad: null).Nueva(31, 51, new SolicitudMvcRegistroViewModel(), default);
        Assert.IsType<ForbidResult>(result);
        _service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Resolver_ReenviaActorYMotivoCapturado()
    {
        var result = await Crear().Resolver(31, 41, false, "No reúne requisitos", default);
        Assert.IsType<RedirectToActionResult>(result);
        _service.Verify(x => x.ResolverAsync(7, 12, 31, 41,
            It.Is<ResolverSolicitudMvcDatos>(d => !d.Admitir && d.MotivoNoAdmision == "No reúne requisitos"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
