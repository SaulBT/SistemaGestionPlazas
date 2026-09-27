using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SGPla.Controllers;
using SGPla.Models.DTOs.Avisos;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Controllers;

public sealed class AvisosMvcWorkflowTests
{
    private readonly Mock<IAvisoMvcService> _avisos = new();
    private readonly Mock<IDocumentoAvisoMvcService> _documentos = new();

    private AvisosMvcController Crear(string rol, int? usuario = 7, int? entidad = 12)
    {
        var controller = new AvisosMvcController(_avisos.Object, _documentos.Object);
        var claims = new List<Claim> { new(ClaimTypes.Role, rol) };
        if (usuario.HasValue) claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.Value.ToString()));
        if (entidad.HasValue) claims.Add(new Claim("EntidadAcademicaId", entidad.Value.ToString()));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        };
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    [Fact]
    public async Task Cancelar_UsaActorDeSesionYMotivoDelFormulario()
    {
        var controller = Crear("Coordinador DGAA");

        var resultado = await controller.Cancelar(31, "Retiro autorizado", CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(resultado);
        _avisos.Verify(x => x.CancelarAsync(7, 31,
            It.Is<CancelarAvisoMvcDatos>(d => d.Motivo == "Retiro autorizado"), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Cancelar_SinIdentidadNoInvocaServicio()
    {
        var controller = Crear("Coordinador DGAA", usuario: null);

        Assert.IsType<ForbidResult>(await controller.Cancelar(31, "Retiro", CancellationToken.None));
        _avisos.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Archivar_UsaIdentidadDeSesion()
    {
        var controller = Crear("Coordinador EA");

        Assert.IsType<RedirectToActionResult>(await controller.Archivar(31, CancellationToken.None));
        _avisos.Verify(x => x.ArchivarAsync(7, 31, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task EliminarBorrador_UsaAmbitoEADeSesionYRedirigeAlListado()
    {
        var controller = Crear("Coordinador EA");

        var resultado = Assert.IsType<RedirectToActionResult>(
            await controller.EliminarBorrador(31, CancellationToken.None));

        Assert.Equal("Index", resultado.ActionName);
        _documentos.Verify(x => x.EliminarBorradorAsync(31, 12, 7, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task EliminarBorrador_SinAmbitoNoInvocaServicio()
    {
        var controller = Crear("Coordinador EA", entidad: null);

        Assert.IsType<ForbidResult>(await controller.EliminarBorrador(31, CancellationToken.None));
        _documentos.VerifyNoOtherCalls();
    }
}
