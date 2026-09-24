using Moq;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Models.ViewModels.Catalogos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public sealed class ProgramaEducativoMvcServiceTests
{
    private readonly Mock<IProgramaEducativoMvcRepository> _repository = new();
    private readonly Mock<ICatalogosMvcService> _catalogos = new();

    [Fact]
    public async Task Actualizar_rechaza_cambiar_entidad_academica()
    {
        ConfigureExisting(hasPlans: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().GuardarAsync(Input(entidadId: 8)));

        Assert.Contains("inmutable", exception.Message);
        _repository.Verify(x => x.ActualizarAsync(It.IsAny<GuardarProgramaEducativoMvcDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_rechaza_cambiar_sistema_o_nivel_si_hubo_un_plan()
    {
        ConfigureExisting(hasPlans: true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().GuardarAsync(Input(sistemaId: 9)));

        Assert.Contains("ya tuvo planes", exception.Message);
        _repository.Verify(x => x.ActualizarAsync(It.IsAny<GuardarProgramaEducativoMvcDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_permite_cambiar_sistema_y_nivel_si_nunca_hubo_planes()
    {
        ConfigureExisting(hasPlans: false);
        _repository.Setup(x => x.ActualizarAsync(It.IsAny<GuardarProgramaEducativoMvcDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var id = await CreateService().GuardarAsync(Input(sistemaId: 9, nivelId: 10));

        Assert.Equal(4, id);
        _repository.Verify(x => x.ActualizarAsync(It.Is<GuardarProgramaEducativoMvcDto>(d =>
            d.SistemaEducativoId == 9 && d.NivelFormacionId == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    private ProgramaEducativoMvcService CreateService() =>
        new(_repository.Object, _catalogos.Object, TimeProvider.System);

    private void ConfigureExisting(bool hasPlans)
    {
        _repository.Setup(x => x.CatalogosActivosAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repository.Setup(x => x.ObtenerAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProgramaEducativoMvcDto(4, "Programa", 7, "Entidad", 2, "Área", 3, "Campus", 1, "Región", 5, "Sistema", 6, "Nivel"));
        _repository.Setup(x => x.TienePlanesAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(hasPlans);
    }

    private static GuardarProgramaEducativoMvcDto Input(int entidadId = 7, int sistemaId = 5, int nivelId = 6) => new()
    {
        Id = 4,
        Nombre = "Programa actualizado",
        EntidadAcademicaId = entidadId,
        SistemaEducativoId = sistemaId,
        NivelFormacionId = nivelId
    };
}
