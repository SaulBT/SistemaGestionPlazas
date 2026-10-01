using Microsoft.Extensions.Options;
using Moq;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public class ProgramacionPlaneaServiceTests
{
    private readonly Mock<IProgramacionPlaneaRepository> _repositorio = new();
    private readonly Mock<ISincronizarPeriodoPlaneaService> _sincronizarPeriodo = new();
    private readonly Mock<ISincronizarPeriodosVigentesService> _sincronizarVigentes = new();

    private ProgramacionPlaneaService Crear(string apiKey = "token") =>
        new(_repositorio.Object, _sincronizarPeriodo.Object, _sincronizarVigentes.Object,
            Options.Create(new PlaneaOpciones { ApiKey = apiKey }));

    [Fact]
    public async Task SincronizarAsync_SinToken_NoSincroniza()
    {
        var (exito, mensaje) = await Crear(apiKey: "").SincronizarAsync(5);

        Assert.False(exito);
        Assert.Contains("token", mensaje);
        _sincronizarPeriodo.VerifyNoOtherCalls();
        _sincronizarVigentes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SincronizarAsync_PeriodoInexistente_DevuelveError()
    {
        _repositorio.Setup(r => r.ObtenerPeriodoAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((PeriodoPorSincronizar?)null);

        var (exito, _) = await Crear().SincronizarAsync(99);

        Assert.False(exito);
        _sincronizarPeriodo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SincronizarAsync_ConPeriodo_SincronizaSoloEsePeriodoYResume()
    {
        var periodo = new PeriodoPorSincronizar(5, "202701");
        _repositorio.Setup(r => r.ObtenerPeriodoAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(periodo);
        _sincronizarPeriodo.Setup(s => s.SincronizarAsync(periodo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSincronizacionPlanea("202701", PlaneaConstantes.ESTADO_EXITOSA, new ResumenAplicacionPlanea(8, 0, 17114, 20), null));

        var (exito, mensaje) = await Crear().SincronizarAsync(5);

        Assert.True(exito);
        Assert.Contains("202701: 8 NRC nuevos", mensaje);
        Assert.Contains("20 horarios", mensaje);
        _sincronizarVigentes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SincronizarAsync_SinPeriodo_UsaVigentesYReportaFallas()
    {
        _sincronizarVigentes.Setup(s => s.EjecutarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ResultadoSincronizacionPlanea("202701", PlaneaConstantes.ESTADO_EXITOSA, new ResumenAplicacionPlanea(0, 8, 17114, 0), null),
            new ResultadoSincronizacionPlanea("202751", PlaneaConstantes.ESTADO_FALLIDA, null, "Response status code does not indicate success: 500")
        ]);

        var (exito, mensaje) = await Crear().SincronizarAsync(null);

        Assert.False(exito);
        Assert.Contains("202751: Fallida", mensaje);
    }

    [Fact]
    public async Task ObtenerAsync_NormalizaBusquedaPaginaYLimite()
    {
        FiltroProgramacionPlaneaDTO? recibido = null;
        _repositorio.Setup(r => r.ObtenerCopiasAsync(It.IsAny<FiltroProgramacionPlaneaDTO>(), It.IsAny<CancellationToken>()))
            .Callback<FiltroProgramacionPlaneaDTO, CancellationToken>((f, _) => recibido = f)
            .ReturnsAsync((new List<CopiaProgramacionPlaneaDTO>(), 0, 1));

        await Crear().ObtenerAsync(new FiltroProgramacionPlaneaDTO { Busqueda = "  10676  ", Pagina = 0, Limite = 0 });

        Assert.NotNull(recibido);
        Assert.Equal("10676", recibido.Busqueda);
        Assert.Equal(1, recibido.Pagina);
        Assert.Equal(10, recibido.Limite);
    }

    [Fact]
    public async Task ObtenerEncabezadoAsync_PlanOPeriodoInexistente_DevuelveNull()
    {
        _repositorio.Setup(r => r.ObtenerEncabezadoAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EncabezadoProgramacionPlaneaDTO?)null);

        Assert.Null(await Crear().ObtenerEncabezadoAsync(1, 99));
    }

    [Fact]
    public async Task ObtenerEncabezadoAsync_AgregaElPeriodoLegible()
    {
        _repositorio.Setup(r => r.ObtenerEncabezadoAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EncabezadoProgramacionPlaneaDTO(3, "Facultad de Estadística", "XAL", "Estadística", "ESTA-24-E-CR", "202701"));

        var encabezado = await Crear().ObtenerEncabezadoAsync(1, 5);

        Assert.NotNull(encabezado);
        Assert.Equal("Facultad de Estadística", encabezado.EntidadAcademica);
        Assert.False(string.IsNullOrWhiteSpace(encabezado.PeriodoMostrar));
        Assert.NotEqual("202701", encabezado.PeriodoMostrar);
    }

    [Fact]
    public async Task ObtenerAsync_DevuelveTotalYPaginaDelRepositorio()
    {
        _repositorio.Setup(r => r.ObtenerCopiasAsync(It.IsAny<FiltroProgramacionPlaneaDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CopiaProgramacionPlaneaDTO>(), 256, 26));

        var resultado = await Crear().ObtenerAsync(new FiltroProgramacionPlaneaDTO { Pagina = 99 });

        Assert.Equal(256, resultado.Total);
        Assert.Equal(26, resultado.Pagina);
    }
}
