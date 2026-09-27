using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Moq;
using SGPla.Controllers;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Controllers;

public sealed class SincronizacionPlaneaControllerTests
{
    [Fact]
    public async Task Post_encola_y_responde_sin_invocar_la_sincronizacion_en_linea()
    {
        var sync = new Mock<ISincronizacionPlaneaService>();
        sync.Setup(x => x.SolicitarAsync(77, It.IsAny<CancellationToken>())).ReturnsAsync(123);
        sync.Setup(x => x.SincronizarAsync(77, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No debe invocarse desde el POST."));
        var controller = new ProgramacionesAcademicasMvcController(
            Mock.Of<IProgramacionAcademicaMvcService>(), Mock.Of<ICatalogosMvcService>(), sync.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        var resultado = await controller.SincronizarPlanea(77, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Contains("fue encolada", controller.TempData["Success"]?.ToString());
        sync.Verify(x => x.SolicitarAsync(77, It.IsAny<CancellationToken>()), Times.Once);
        sync.Verify(x => x.SincronizarAsync(77, It.IsAny<CancellationToken>()), Times.Never);
    }
}
