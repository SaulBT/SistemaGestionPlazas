using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.AreaAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;

public class AreaAcademicaServiceTests
{
    private readonly Mock<IAreaAcademicaRepository> _areaAcademicaRepositoryMock;
    private readonly Mock<IAreaAcademicaValidator> _areaAcademicaValidatorMock;
    private readonly AreaAcademicaService _areaAcademicaService;

    public AreaAcademicaServiceTests()
    {
        _areaAcademicaRepositoryMock = new Mock<IAreaAcademicaRepository>();
        _areaAcademicaValidatorMock = new Mock<IAreaAcademicaValidator>();

        _areaAcademicaService = new AreaAcademicaService(
            _areaAcademicaRepositoryMock.Object,
            _areaAcademicaValidatorMock.Object);
    }

    //CP-34
    [Fact]
    public async Task CrearAreaAcademica()
    {
        var dto = new CrearAreaAcademicaDTO
        {
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-07",
            Extension = "11707"
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.CrearAsync(It.IsAny<AreaAcademica>()))
            .ReturnsAsync(new AreaAcademica
            {
                IdAreaAcademica = 1,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Extension = dto.Extension
            });

        var idAreaAcademicaCreada = await _areaAcademicaService.CrearAsync(dto);

        Assert.Equal(1, idAreaAcademicaCreada);

        _areaAcademicaValidatorMock.Verify(validator => validator.ValidarCreacion(dto), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.CrearAsync(
            It.Is<AreaAcademica>(areaAcademica =>
                areaAcademica.Nombre == dto.Nombre &&
                areaAcademica.Telefono == dto.Telefono &&
                areaAcademica.Extension == dto.Extension)),
            Times.Once);
    }

    //CP-36
    [Fact]
    public async Task ObtenerListaDeAreasAcademicas()
    {
        var areasAcademicas = new List<AreaAcademica>
        {
            new AreaAcademica
            {
                IdAreaAcademica = 1,
                Nombre = "Dirección General del Área Académica de Artes",
                Telefono = "(228) 842-17-07",
                Extension = "11707"
            },
            new AreaAcademica
            {
                IdAreaAcademica = 2,
                Nombre = "Dirección General del Área Académica de Humanidades",
                Telefono = "(228) 863-18-90",
                Extension = "12708"
            }
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerTodosAsync())
            .ReturnsAsync(areasAcademicas);

        var resultado = await _areaAcademicaService.ObtenerTodasAsync();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Equal("Dirección General del Área Académica de Artes", resultado[0].Nombre);
        Assert.Equal("Teléfono: (228) 842-17-07 Ext: 11707", resultado[0].Telefono);

        Assert.Equal("Dirección General del Área Académica de Humanidades", resultado[1].Nombre);
        Assert.Equal("Teléfono: (228) 863-18-90 Ext: 12708", resultado[1].Telefono);
    }

    //CP-37
    [Fact]
    public async Task ObtenerListaDeAreasAcademicasPorNombre()
    {
        var nombre = "Humanidades";

        var areasAcademicas = new List<AreaAcademica>
        {
            new AreaAcademica
            {
                IdAreaAcademica = 2,
                Nombre = "Dirección General del Área Académica de Humanidades",
                Telefono = "(228) 863-18-90",
                Extension = "12708"
            }
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorNombreAsync(nombre))
            .ReturnsAsync(areasAcademicas);

        var resultado = await _areaAcademicaService.ObtenerPorNombreAsync(nombre);

        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Dirección General del Área Académica de Humanidades", resultado[0].Nombre);
        Assert.Equal("Teléfono: (228) 863-18-90 Ext: 12708", resultado[0].Telefono);
    }

    //CP-38
    [Fact]
    public async Task ObtenerAreaAcademicaPorId()
    {
        const int idAreaAcademica = 1;

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 1,
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-07",
            Extension = "11707"
        };

        _areaAcademicaValidatorMock
            .Setup(validator => validator.ValidarIdAsync(idAreaAcademica))
            .Returns(Task.CompletedTask);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(idAreaAcademica))
            .ReturnsAsync(areaAcademica);

        var resultado = await _areaAcademicaService.ObtenerPorIdAsync(idAreaAcademica);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.IdAreaAcademica);
        Assert.Equal("Dirección General del Área Académica de Artes", resultado.Nombre);
        Assert.Equal("(228) 842-17-07", resultado.Telefono);
        Assert.Equal("11707", resultado.Extension);

        _areaAcademicaValidatorMock.Verify(validator => validator.ValidarIdAsync(idAreaAcademica), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.ObtenerPorIdAsync(idAreaAcademica), Times.Once);
    }

    //CP-41
    [Fact]
    public async Task EditarAreaAcademica()
    {
        var dto = new DatosAreaAcademicaDTO
        {
            IdAreaAcademica = 1,
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-67",
            Extension = "11706"
        };

        _areaAcademicaValidatorMock
            .Setup(validator => validator.ValidarEdicionAsync(dto))
            .Returns(Task.CompletedTask);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ActualizarAsync(It.IsAny<AreaAcademica>()))
            .Returns(Task.CompletedTask);

        await _areaAcademicaService.EditarAsync(dto);

        _areaAcademicaValidatorMock.Verify(validator => validator.ValidarEdicionAsync(dto), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.ActualizarAsync(
            It.Is<AreaAcademica>(areaAcademica =>
                areaAcademica.IdAreaAcademica == dto.IdAreaAcademica &&
                areaAcademica.Nombre == dto.Nombre &&
                areaAcademica.Telefono == dto.Telefono &&
                areaAcademica.Extension == dto.Extension)),
            Times.Once);
    }

    //CP-45
    [Fact]
    public async Task EliminarAreaAcademica()
    {
        const int idAreaAcademica = 1;

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 1,
            Nombre = "Dirección General del Área Académica de Artes",
            Telefono = "(228) 842-17-07",
            Extension = "11707"
        };

        _areaAcademicaValidatorMock
            .Setup(validator => validator.ValidarIdAsync(idAreaAcademica))
            .Returns(Task.CompletedTask);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(idAreaAcademica))
            .ReturnsAsync(areaAcademica);

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.EliminarAsync(areaAcademica))
            .Returns(Task.CompletedTask);

        await _areaAcademicaService.EliminarAsync(idAreaAcademica);

        _areaAcademicaValidatorMock.Verify(validator => validator.ValidarIdAsync(idAreaAcademica), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.ObtenerPorIdAsync(idAreaAcademica), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.EliminarAsync(areaAcademica), Times.Once);
    }
}
