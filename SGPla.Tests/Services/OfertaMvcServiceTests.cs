using Moq;
using SGPla.Models.DTOs.Ofertas;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class OfertaMvcServiceTests
{
    [Fact]
    public async Task Crear_normaliza_clave_y_textos_antes_de_persistir()
    {
        var repo = new Mock<IOfertaMvcRepository>();
        repo.Setup(x => x.CrearAsync(It.IsAny<CrearOfertaMvcDatos>(), 7, It.IsAny<CancellationToken>())).ReturnsAsync(42);
        var service = new OfertaMvcService(repo.Object);

        var id = await service.CrearAsync(new CrearOfertaMvcDatos(12, "  prof-01 ", 2, 3,
            "  Perfil solicitado  ", "  Justificación  "), 7);

        Assert.Equal(42, id);
        repo.Verify(x => x.CrearAsync(It.Is<CrearOfertaMvcDatos>(d => d.ClavePlaza == "PROF-01"
            && d.PerfilSolicitado == "Perfil solicitado" && d.Justificacion == "Justificación"), 7,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("plaza/uno")]
    [InlineData("")]
    public async Task Crear_rechaza_clave_incompatible_con_esquema(string clave)
    {
        var repo = new Mock<IOfertaMvcRepository>();
        var service = new OfertaMvcService(repo.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearAsync(
            new CrearOfertaMvcDatos(12, clave, 2, 3, "Perfil", null), 7));

        repo.Verify(x => x.CrearAsync(It.IsAny<CrearOfertaMvcDatos>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
