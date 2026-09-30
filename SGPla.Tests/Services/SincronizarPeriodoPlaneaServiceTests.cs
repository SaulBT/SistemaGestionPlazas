using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public class SincronizarPeriodoPlaneaServiceTests
{
    private static readonly PeriodoPorSincronizar Periodo = new(7, "202701");
    private static PlaneaRespuesta Valida() => new() { Periodo = "202701", Total = 1, Resultado = [new() { Nrc="10676", Materia="CVCB", Curso="18003", CodigoPlan="CIVI-20-E-CR", Titulo="FISICA" }] };
    private static (Mock<IPlaneaCliente> Cliente, Mock<ISincronizacionPlaneaRepository> Repo, SincronizarPeriodoPlaneaService Servicio) Crear(PlaneaRespuesta? respuesta = null)
    {
        var cliente = new Mock<IPlaneaCliente>(); cliente.Setup(c => c.ObtenerPeriodoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(respuesta ?? Valida());
        var repo = new Mock<ISincronizacionPlaneaRepository>(); repo.Setup(r => r.IniciarBitacoraAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(123);
        repo.Setup(r => r.RegistrarNuevasAsync(7, "202701", 123, It.IsAny<DatosPeriodoPlanea>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ResumenAplicacionPlanea(1,0,0,0));
        var servicio = new SincronizarPeriodoPlaneaService(cliente.Object, repo.Object, Options.Create(new PlaneaOpciones { EsperaEntreIntentos = TimeSpan.Zero, Intentos = 3 }), NullLogger<SincronizarPeriodoPlaneaService>.Instance);
        return (cliente, repo, servicio);
    }

    [Fact]
    public async Task SincronizarAsync_RegistraYCierraExitosa()
    {
        var (cliente, repo, servicio) = Crear(); var resultado = await servicio.SincronizarAsync(Periodo);
        Assert.Equal(PlaneaConstantes.ESTADO_EXITOSA, resultado.Estado);
        repo.Verify(r => r.RegistrarNuevasAsync(7, "202701", 123, It.IsAny<DatosPeriodoPlanea>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.CerrarBitacoraAsync(123, PlaneaConstantes.ESTADO_EXITOSA, 1, It.IsAny<DatosPeriodoPlanea>(), It.IsAny<ResumenAplicacionPlanea>(), It.IsAny<string?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SincronizarAsync_TotalNoCoincide_NoRegistraYFalla()
    {
        var r = Valida(); r.Total = 2; var (_, repo, servicio) = Crear(r); var result = await servicio.SincronizarAsync(Periodo);
        Assert.Equal(PlaneaConstantes.ESTADO_FALLIDA, result.Estado); repo.Verify(x => x.RegistrarNuevasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DatosPeriodoPlanea>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(x => x.CerrarBitacoraAsync(123, PlaneaConstantes.ESTADO_FALLIDA, 1, null, null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SincronizarAsync_PeriodoDistinto_NoRegistraYFalla()
    {
        var r = Valida(); r.Periodo = "202751"; var (_, repo, servicio) = Crear(r); Assert.Equal(PlaneaConstantes.ESTADO_FALLIDA, (await servicio.SincronizarAsync(Periodo)).Estado);
        repo.Verify(x => x.RegistrarNuevasAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DatosPeriodoPlanea>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task SincronizarAsync_TotalCero_SinDatos()
    {
        var r = new PlaneaRespuesta { Periodo="202701", Total=0, Resultado=[] }; var (_, repo, service) = Crear(r);
        Assert.Equal(PlaneaConstantes.ESTADO_SIN_DATOS, (await service.SincronizarAsync(Periodo)).Estado);
        repo.Verify(x => x.CerrarBitacoraAsync(123, PlaneaConstantes.ESTADO_SIN_DATOS, 0, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SincronizarAsync_EnCurso_Omitida()
    {
        var (_, repo, service) = Crear(); repo.Setup(r => r.RegistrarNuevasAsync(7,"202701",123,It.IsAny<DatosPeriodoPlanea>(),It.IsAny<CancellationToken>())).ThrowsAsync(new SincronizacionEnCursoException("202701"));
        Assert.Equal(PlaneaConstantes.ESTADO_OMITIDA, (await service.SincronizarAsync(Periodo)).Estado);
        repo.Verify(r => r.CerrarBitacoraAsync(123, PlaneaConstantes.ESTADO_OMITIDA, 1, It.IsAny<DatosPeriodoPlanea>(), null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SincronizarAsync_ReintentaYLuegoExito()
    {
        var (cliente, _, service) = Crear(); var count=0;
        cliente.Setup(c => c.ObtenerPeriodoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(() => ++count < 3 ? Task.FromException<PlaneaRespuesta>(new HttpRequestException()) : Task.FromResult(Valida()));
        Assert.Equal(PlaneaConstantes.ESTADO_EXITOSA, (await service.SincronizarAsync(Periodo)).Estado); Assert.Equal(3,count);
    }
    [Fact]
    public async Task SincronizarAsync_AgotaReintentos_Fallida()
    {
        var (cliente, _, service) = Crear(); cliente.Setup(c => c.ObtenerPeriodoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("offline"));
        Assert.Equal(PlaneaConstantes.ESTADO_FALLIDA, (await service.SincronizarAsync(Periodo)).Estado); cliente.Verify(c => c.ObtenerPeriodoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
