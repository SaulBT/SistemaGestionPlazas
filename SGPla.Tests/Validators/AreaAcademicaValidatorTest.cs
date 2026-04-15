using Moq;
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

    //CP-35
    [Fact]
    public void CrearAreaAcademicaConCamposNulos()
    {
        var dto = new CrearAreaAcademicaDTO
        {
            Nombre = null,
            CalleNumero = null,
            Colonia = null,
            Cp = null,
            Municipio = null,
            Telefono = null,
            Conmutador = null,
            Extension = null,
            Fax = null
        };

        var ex = Record.Exception(() => _areaAcademicaValidator.ValidarCreacion(dto));

        Assert.NotNull(ex);
        Assert.Contains("Nombre", ex.Message);
        Assert.Contains("obligatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    //CP-39
    [Fact]
    public async Task ObtenerAreaAcademicaConIdInvalida()
    {
        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(-12));

        Assert.NotNull(ex);
        Assert.IsType<ArithmeticException>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-40
    [Fact]
    public async Task ObtenerAreaAcademicaInexistente()
    {
        const int idAreaAcademica = 6;

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(idAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(idAreaAcademica));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-42
    [Fact]
    public async Task EditarAreaAcademicaConIdInvalida()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 0,
            Nombre = "Dirección General del Área Académica de Artes",
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 5",
            Colonia = "Zona Universitaria",
            Cp = "91096",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-67",
            Conmutador = "(228) 842-27-00",
            Extension = "11706",
            Fax = "(228) 842-72-57"
        };

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArithmeticException>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-43
    [Fact]
    public async Task EditarAreaAcademicaConCamposNulos()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 1,
            Nombre = null,
            CalleNumero = null,
            Colonia = null,
            Cp = null,
            Municipio = null,
            Telefono = null,
            Conmutador = null,
            Extension = null,
            Fax = null
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Contains("Nombre", ex.Message);
        Assert.Contains("obligatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    //CP-44
    [Fact]
    public async Task EditarAreaAcademicaInexistente()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 90,
            Nombre = "Dirección General del Área Académica de Artes",
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 5",
            Colonia = "Zona Universitaria",
            Cp = "91096",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-67",
            Conmutador = "(228) 842-27-00",
            Extension = "11706",
            Fax = "(228) 842-72-57"
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-46
    [Fact]
    public async Task EliminarAreaAcademicaConIdInvalida()
    {
        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(0));

        Assert.NotNull(ex);
        Assert.IsType<ArithmeticException>(ex);
        Assert.Contains("La Id es inválida", ex.Message);
    }

    //CP-47
    [Fact]
    public async Task EliminarAreaAcademicaInexistente()
    {
        const int idAreaAcademica = 5;

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(idAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _areaAcademicaValidator.ValidarIdAsync(idAreaAcademica));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }
}
