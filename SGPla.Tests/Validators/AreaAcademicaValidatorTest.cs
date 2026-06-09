using Moq;
using SGPla.Commons;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

public class AreaAcademicaValidatorTests
{
    private readonly Mock<IAreaAcademicaRepository> _areaAcademicaRepositoryMock;
    private readonly AreaAcademicaValidator _areaAcademicaValidator;

    public AreaAcademicaValidatorTests()
    {
        _areaAcademicaRepositoryMock = new Mock<IAreaAcademicaRepository>();
        _areaAcademicaValidator = new AreaAcademicaValidator(_areaAcademicaRepositoryMock.Object);
    }

    //CP-03-02
    [Fact]
    public void CrearAreaAcademicaConCamposNulos()
    {
        var dto = new CrearAreaAcademicaDTO
        {
            Nombre = null,
            Telefono = null,
            Extension = null
        };

        var ex = Record.Exception(() => _areaAcademicaValidator.ValidarCreacion(dto));

        Assert.NotNull(ex);
        Assert.Contains("Nombre", ex.Message);
        Assert.Contains("obligatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    //CP-03-06
    [Fact]
    public async Task ObtenerAreaAcademicaConIdInvalida()
    {
        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(-12));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-03-07
    [Fact]
    public async Task ObtenerAreaAcademicaInexistente()
    {
        const int idAreaAcademica = 6;

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(idAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(idAreaAcademica));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-03-09
    [Fact]
    public async Task EditarAreaAcademicaConIdInvalida()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 0,
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-67",
            Extension = "11706"
        };

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-03-10
    [Fact]
    public async Task EditarAreaAcademicaConCamposNulos()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 1,
            Nombre = null,
            Telefono = null,
            Extension = null
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Contains("Nombre", ex.Message);
        Assert.Contains("obligatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    //CP-03-11
    [Fact]
    public async Task EditarAreaAcademicaInexistente()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 90,
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-67",
            Extension = "11706"
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-03-13
    [Fact]
    public async Task EliminarAreaAcademicaConIdInvalida()
    {
        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(0));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-03-14
    [Fact]
    public async Task EliminarAreaAcademicaInexistente()
    {
        const int idAreaAcademica = 5;

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(idAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(idAreaAcademica));

        Assert.NotNull(ex);
        Assert.IsType<ValidacionExcepction>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }
}
