using Moq;
using SGPla.Models.DTOs.Oferta;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

namespace SGPla.Tests.Validators;

public class ProgramacionAcademicaValidatorTests
{
    private readonly Mock<IProgramaEducativoRepository> _programas = new();

    private ProgramacionAcademicaValidator CrearValidator() => new(
        Mock.Of<IDocenteRepository>(), _programas.Object,
        Mock.Of<IProgramacionAcademicaRepository>(), Mock.Of<IArticuloRepository>());

    [Theory]
    [InlineData("14140-Contaduria")]
    [InlineData("14140-CONTADURÍA")]
    [InlineData(" 14140 - Contaduria")]
    public async Task AceptaCodigoRegistradoSinDependerDelNombre(string programaArchivo)
    {
        _programas.Setup(p => p.ObtenerIdsProgramasAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, int> { ["14140"] = 2013 });

        Assert.True(await CrearValidator().ValidarProgramas(
            [new OfertaDTO { Programa = programaArchivo }]));

        _programas.Verify(p => p.ObtenerNombresProgramasRegistradosAsync(It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task ReportaSoloLosCodigosQueRealmenteFaltan()
    {
        _programas.Setup(p => p.ObtenerIdsProgramasAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, int> { ["14140"] = 2013 });

        var error = await Assert.ThrowsAsync<Exception>(() => CrearValidator().ValidarProgramas(
            [new OfertaDTO { Programa = "14140-Contaduria" },
             new OfertaDTO { Programa = "14141-Administracion" }]));

        Assert.Contains("14141-Administracion", error.Message);
        Assert.DoesNotContain("14140", error.Message);
    }

    [Fact]
    public async Task RechazaListaSinProgramas()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CrearValidator().ValidarProgramas([]));
        _programas.Verify(p => p.ObtenerIdsProgramasAsync(It.IsAny<List<string>>()), Times.Never);
    }
}
