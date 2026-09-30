using Moq;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public class ConsultaPlaneaServiceTests
{
    private readonly Mock<IConsultaPlaneaRepository> _repo = new();
    private ConsultaPlaneaService Servicio => new(_repo.Object);

    [Fact]
    public async Task ObtenerBitacoraAsync_NormalizaPaginacionInvalida()
    {
        var f = new FiltroBitacoraPlaneaDTO { Pagina=0, Cantidad=200 };
        _repo.Setup(r => r.ObtenerBitacoraAsync(f, It.IsAny<CancellationToken>())).ReturnsAsync((new List<BitacoraPlaneaDTO>(),0));
        await Servicio.ObtenerBitacoraAsync(f); Assert.Equal(1,f.Pagina); Assert.Equal(10,f.Cantidad);
    }
    [Fact]
    public async Task ObtenerCopiasAsync_RecortaBusquedaYNormalizaPaginacion()
    {
        var f = new FiltroCopiaPlaneaDTO { Busqueda="  " + new string('x',120) + "  ", Pagina=-1, Cantidad=0 };
        _repo.Setup(r => r.ObtenerCopiasAsync(f, It.IsAny<CancellationToken>())).ReturnsAsync((new List<CopiaPlaneaFilaDTO>(),0));
        await Servicio.ObtenerCopiasAsync(f); Assert.Equal(1,f.Pagina); Assert.Equal(20,f.Cantidad); Assert.Equal(100,f.Busqueda!.Length); Assert.DoesNotContain(' ',f.Busqueda);
    }
    [Fact]
    public async Task ObtenerBitacoraAsync_IgnoraEstadoInvalido()
    {
        var f = new FiltroBitacoraPlaneaDTO { Estado="inventado" };
        _repo.Setup(r => r.ObtenerBitacoraAsync(f, It.IsAny<CancellationToken>())).ReturnsAsync((new List<BitacoraPlaneaDTO>(),0));
        await Servicio.ObtenerBitacoraAsync(f); Assert.Null(f.Estado);
    }
    [Fact]
    public async Task ObtenerDetalleCopiaAsync_DevuelveNullSiNoExiste()
    {
        _repo.Setup(r => r.ObtenerDetalleCopiaAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync((DetalleCopiaPlaneaDTO?)null);
        Assert.Null(await Servicio.ObtenerDetalleCopiaAsync(4));
    }
}
