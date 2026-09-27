using Moq;
using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class AvisoMvcServiceTests
{
    [Fact]
    public async Task CrearBorrador_NormalizaTipoYConservaIdsDelFormulario()
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CrearBorradorAsync(7, 12,
                It.Is<CrearAvisoMvcDatos>(d => d.TipoComunicado == "CONVOCATORIA"
                    && d.OfertaIds.SequenceEqual(new[] { 31, 32 })), It.IsAny<CancellationToken>()))
            .ReturnsAsync(81);
        var service = new AvisoMvcService(repository.Object);

        var id = await service.CrearBorradorAsync(7, 12,
            new CrearAvisoMvcDatos(2, 3, 4, " convocatoria ", [31, 32]));

        Assert.Equal(81, id);
    }

    [Fact]
    public async Task CrearBorrador_RechazaOfertasDuplicadasAntesDePersistir()
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        var service = new AvisoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearBorradorAsync(7, 12,
            new CrearAvisoMvcDatos(2, 3, 4, "AVISO", [31, 31])));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CrearBorrador_RechazaIdsOComunicadoInvalido()
    {
        var service = new AvisoMvcService(Mock.Of<IAvisoMvcRepository>());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearBorradorAsync(7, 12,
            new CrearAvisoMvcDatos(2, 3, 4, "CARTA", [31])));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearBorradorAsync(7, 12,
            new CrearAvisoMvcDatos(0, 3, 4, "AVISO", [31])));
    }

    [Fact]
    public async Task CrearBorrador_RechazaMasDeCincuentaOfertas()
    {
        var service = new AvisoMvcService(Mock.Of<IAvisoMvcRepository>());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearBorradorAsync(7, 12,
            new CrearAvisoMvcDatos(2, 3, 4, "AVISO", Enumerable.Range(1, 51).ToArray())));
    }

    [Fact]
    public async Task EnviarARevision_RechazaSinHorarioAntesDeLlamarAlRepositorio()
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        var service = new AvisoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.EnviarARevisionAsync(7, 12, 81,
            new ConfigurarYEnviarAvisoMvcDatos(1, "Requisitos", null, "contacto@example.org",
                "Titular", new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12), [])));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResolverRevision_ExigeComentariosAlDevolver()
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        var service = new AvisoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ResolverRevisionAsync(7, 81,
            avalar: false, comentarios: " "));

        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ftp://example.org/aviso")]
    [InlineData("/avisos/123")]
    [InlineData("")]
    public async Task Publicar_RechazaUrlNoHttpAntesDePersistir(string url)
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        var service = new AvisoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.PublicarAsync(7, 12, 81,
            new PublicarAvisoMvcDatos(new DateOnly(2026, 10, 1), url)));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cancelar_ExigeMotivoYLoNormalizaAntesDePersistir()
    {
        var repository = new Mock<IAvisoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CancelarAsync(7, 81,
                It.Is<CancelarAvisoMvcDatos>(d => d.Motivo == "Retiro autorizado"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = new AvisoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CancelarAsync(7, 81,
            new CancelarAvisoMvcDatos("  ")));
        await service.CancelarAsync(7, 81, new CancelarAvisoMvcDatos("  Retiro autorizado  "));

        repository.Verify(x => x.CancelarAsync(7, 81,
            It.Is<CancelarAvisoMvcDatos>(d => d.Motivo == "Retiro autorizado"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
