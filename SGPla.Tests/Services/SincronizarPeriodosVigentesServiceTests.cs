using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public class SincronizarPeriodosVigentesServiceTests
{
    private static SincronizarPeriodosVigentesService Crear(Mock<ISincronizacionPlaneaRepository> repo, Mock<ISincronizarPeriodoPlaneaService> periodo) =>
        new(repo.Object, periodo.Object, Options.Create(new PlaneaOpciones()), NullLogger<SincronizarPeriodosVigentesService>.Instance);

    [Fact]
    public async Task EjecutarAsync_ProcesaEnOrdenAunqueHayaFallida()
    {
        var repo = new Mock<ISincronizacionPlaneaRepository>(); var p = new Mock<ISincronizarPeriodoPlaneaService>();
        var periodos = new[] { new PeriodoPorSincronizar(1,"202701"), new PeriodoPorSincronizar(2,"202751") };
        repo.Setup(r => r.ObtenerPeriodosVigentesAsync(It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(periodos);
        var codigos = new List<string>(); p.Setup(s => s.SincronizarAsync(It.IsAny<PeriodoPorSincronizar>(), It.IsAny<CancellationToken>())).Returns<PeriodoPorSincronizar,CancellationToken>((x,_) => { codigos.Add(x.Codigo); return Task.FromResult(new ResultadoSincronizacionPlanea(x.Codigo, x.IdPeriodo == 1 ? PlaneaConstantes.ESTADO_FALLIDA : PlaneaConstantes.ESTADO_EXITOSA,null,null)); });
        var result = await Crear(repo,p).EjecutarAsync(); Assert.Equal(new[] { "202701", "202751" }, codigos); Assert.Equal(2,result.Count);
        repo.Verify(r => r.MarcarInterrumpidasAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task EjecutarAsync_SinPeriodos_NoSincroniza()
    {
        var repo = new Mock<ISincronizacionPlaneaRepository>(); var p = new Mock<ISincronizarPeriodoPlaneaService>();
        repo.Setup(r => r.ObtenerPeriodosVigentesAsync(It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<PeriodoPorSincronizar>());
        Assert.Empty(await Crear(repo,p).EjecutarAsync()); p.Verify(s => s.SincronizarAsync(It.IsAny<PeriodoPorSincronizar>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
