using Moq;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class IntegranteConsejoTecnicoMvcServiceTests
{
    [Fact]
    public async Task Crear_NormalizaTextoYReenviaAmbitoEIds()
    {
        var repository = new Mock<IIntegranteConsejoTecnicoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CrearAsync(10, 20,
                It.Is<IntegranteConsejoTecnicoMvcCambio>(dto => dto.Nombre == "Nombre Apellido"
                    && dto.Cargo == "Directora" && dto.TratamientoAcademicoId == 4),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = new IntegranteConsejoTecnicoMvcService(repository.Object);

        await service.CrearAsync(10, 20, new IntegranteConsejoTecnicoMvcCambio(
            "  Nombre Apellido ", " Directora ", 4, new DateOnly(2026, 1, 1), null));

        repository.VerifyAll();
    }

    [Fact]
    public async Task Crear_RechazaFechaVaciaAntesDeConsultarRepositorio()
    {
        var repository = new Mock<IIntegranteConsejoTecnicoMvcRepository>(MockBehavior.Strict);
        var service = new IntegranteConsejoTecnicoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(10, 20,
            new IntegranteConsejoTecnicoMvcCambio("Nombre", "Cargo", 4, default, null)));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CambiarVigencia_RechazaTratamientoSinId()
    {
        var repository = new Mock<IIntegranteConsejoTecnicoMvcRepository>(MockBehavior.Strict);
        var service = new IntegranteConsejoTecnicoMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CambiarVigenciaAsync(10, 20, 30,
            new IntegranteConsejoTecnicoMvcCambio("Nombre", "Cargo", 0, new DateOnly(2026, 1, 1), null)));

        repository.VerifyNoOtherCalls();
    }
}
