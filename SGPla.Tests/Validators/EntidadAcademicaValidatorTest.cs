using Moq;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Models.DTOs.EntidadAcademica;

public class EntidadAcademicaValidatorTests
{
    private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
    private readonly Mock<IAreaAcademicaRepository> _areaAcademicaRepositoryMock;
    private readonly EntidadAcademicaValidator _entidadAcademicaValidator;

    public EntidadAcademicaValidatorTests()
    {
        _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
        _areaAcademicaRepositoryMock = new Mock<IAreaAcademicaRepository>();
        _entidadAcademicaValidator = new EntidadAcademicaValidator(
            _entidadAcademicaRepositoryMock.Object,
            _areaAcademicaRepositoryMock.Object);
    }

    //CP-49
    [Fact]
    public async Task CrearEntidadAcademicaConDatosNulos()
    {
        var dto = new CrearEntidadAcademicaDTO
        {
            IdAreaAcademica = 1,
            Clave = null!,
            Nombre = null!,
            CalleNumero = null!,
            Colonia = null!,
            Cp = null!,
            Municipio = null!,
            Telefono = null!,
            Conmutador = null!,
            Extension = null!,
            Fax = null!,
            Region = null!
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("El Nombre es obligatorio.", ex.Message);
    }

    //CP-50
    [Fact]
    public async Task CrearEntidadAcademicaConIdAreaAcademicaInvalida()
    {
        var dto = CrearEntidadAcademicaDTOValido();
        dto.IdAreaAcademica = -67;

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La IdAreaAcademica es inválida.", ex.Message);
    }

    //CP-51
    [Fact]
    public async Task CrearEntidadAcademicaConAreaAcademicaInexistente()
    {
        var dto = CrearEntidadAcademicaDTOValido();
        dto.IdAreaAcademica = 10;

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<KeyNotFoundException>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-52
    [Fact]
    public async Task CrearEntidadAcademicaConClaveInvalida()
    {
        var dto = CrearEntidadAcademicaDTOValido();
        dto.Clave = "673";

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La Clave es inválida.", ex.Message);
    }

    //CP-53
    [Fact]
    public async Task CrearEntidadAcademicaConClaveYRegionErroneos()
    {
        var dto = CrearEntidadAcademicaDTOValido();
        dto.Clave = "51932";

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorClaveAsync(dto.Clave))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La Clave no coincide con la Región seleccionada.", ex.Message);
    }

    //CP-57
    [Fact]
    public async Task ObtenerEntidadAcademicaConIdInvalido()
    {
        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarIdAsync(0));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La IdEntidadAcademica es inválida.", ex.Message);
    }

    //CP-59
    [Fact]
    public async Task EditarEntidadAcademicaConValoresNulos()
    {
        var dto = new DatosEntidadAcademicaDTO
        {
            IdEntidadAcademica = 12,
            IdAreaAcademica = 2,
            Clave = null!,
            Nombre = null!,
            CalleNumero = null!,
            Colonia = null!,
            Cp = null!,
            Municipio = null!,
            Telefono = null!,
            Conmutador = null!,
            Extension = null!,
            Fax = null!,
            Region = null!
        };

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(true);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("El Nombre es obligatorio.", ex.Message);
    }

    //CP-60
    [Fact]
    public async Task EditarEntidadAcademicaConIdEntidadAcademicaInvalida()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        dto.IdEntidadAcademica = 0;

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La IdEntidadAcademica es inválida.", ex.Message);
    }

    //CP-61
    [Fact]
    public async Task EditarEntidadAcademicaConIdAreaAcademicaInvalida()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        dto.IdAreaAcademica = 0;

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La IdAreaAcademica es inválida.", ex.Message);
    }

    //CP-62
    [Fact]
    public async Task EditarEntidadAcademicaConClaveInvalida()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        dto.Clave = "67";

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(true);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La Clave es inválida.", ex.Message);
    }

    //CP-63
    [Fact]
    public async Task EditarEntidadAcademicaConClaveYRegionErroneos()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        dto.Clave = "23067";

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(true);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(true);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorClaveAsync(dto.Clave))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La Clave no coincide con la Región seleccionada.", ex.Message);
    }

    //CP-63
    [Fact]
    public async Task EditarEntidadAcademicaInexistente()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        dto.IdEntidadAcademica = 56;
        dto.Clave = "13067";

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<KeyNotFoundException>(ex);
        Assert.Contains("No existe esa Entidad Académica.", ex.Message);
    }

    //CP-64
    [Fact]
    public async Task EditarEntidadAcademicaConAreaAcademicaInexistente()
    {
        var dto = DatosEntidadAcademicaDTOValido();

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdEntidadAcademica))
            .ReturnsAsync(true);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(dto.IdAreaAcademica))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.IsType<KeyNotFoundException>(ex);
        Assert.Contains("No existe esa Área Académica.", ex.Message);
    }

    //CP-66
    [Fact]
    public async Task EliminarEntidadAcademicaConIdInvalido()
    {
        var ex = await Record.ExceptionAsync(() => _entidadAcademicaValidator.ValidarIdAsync(0));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("La IdEntidadAcademica es inválida.", ex.Message);
    }

    private static CrearEntidadAcademicaDTO CrearEntidadAcademicaDTOValido()
    {
        return new CrearEntidadAcademicaDTO
        {
            IdAreaAcademica = 1,
            Clave = "11304",
            Nombre = "Facultad de Estadística e Informática",
            CalleNumero = "Av. Xalapa Esq. Manuel Ávila Camacho S/N",
            Colonia = "Obrero Campesina",
            Cp = "91020",
            Municipio = "Xalapa",
            Telefono = "(228) 815-03-74",
            Conmutador = "(228) 842-17-00",
            Extension = "14155",
            Fax = "(228) 814-99-90",
            Region = "1-Xalapa"
        };
    }

    private static DatosEntidadAcademicaDTO DatosEntidadAcademicaDTOValido()
    {
        return new DatosEntidadAcademicaDTO
        {
            IdEntidadAcademica = 12,
            IdAreaAcademica = 2,
            Clave = "11304",
            Nombre = "Facultad de Informática",
            CalleNumero = "Av. Xalapa Esq. Manuel Ávila Camacho 10",
            Colonia = "Obrero Alemán",
            Cp = "91021",
            Municipio = "Xalapa",
            Telefono = "(228) 815-74-74",
            Conmutador = "(228) 019-17-00",
            Extension = "12345",
            Fax = "(228) 814-99-43",
            Region = "1-Xalapa"
        };
    }
}
