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
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 2",
            Colonia = "Zona Universitaria",
            Cp = "91090",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-07",
            Conmutador = "(228) 842-17-00",
            Extension = "11707",
            Fax = "(228) 842-27-57"
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.CrearAsync(It.IsAny<AreaAcademica>()))
            .ReturnsAsync(new AreaAcademica
            {
                IdAreaAcademica = 1,
                Nombre = dto.Nombre,
                CalleNumero = dto.CalleNumero,
                Colonia = dto.Colonia,
                Cp = dto.Cp,
                Municipio = dto.Municipio,
                Telefono = dto.Telefono,
                Conmutador = dto.Conmutador,
                Extension = dto.Extension,
                Fax = dto.Fax
            });

        var idAreaAcademicaCreada = await _areaAcademicaService.CrearAsync(dto);

        Assert.Equal(1, idAreaAcademicaCreada);

        _areaAcademicaValidatorMock.Verify(validator => validator.ValidarCreacion(dto), Times.Once);
        _areaAcademicaRepositoryMock.Verify(repository => repository.CrearAsync(
            It.Is<AreaAcademica>(areaAcademica =>
                areaAcademica.Nombre == dto.Nombre &&
                areaAcademica.CalleNumero == dto.CalleNumero &&
                areaAcademica.Colonia == dto.Colonia &&
                areaAcademica.Cp == dto.Cp &&
                areaAcademica.Municipio == dto.Municipio &&
                areaAcademica.Telefono == dto.Telefono &&
                areaAcademica.Conmutador == dto.Conmutador &&
                areaAcademica.Extension == dto.Extension &&
                areaAcademica.Fax == dto.Fax)),
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
                CalleNumero = "Lomas del Estadio S/N Edificio A Piso 2",
                Colonia = "Zona Universitaria",
                Cp = "91090",
                Municipio = "Xalapa",
                Telefono = "(228) 842-17-07",
                Conmutador = "(228) 842-17-00",
                Extension = "11707",
                Fax = "(228) 842-27-57"
            },
            new AreaAcademica
            {
                IdAreaAcademica = 2,
                Nombre = "Dirección General del Área Académica de Humanidades",
                CalleNumero = "Lomas del Estadio S/N Edificio A Piso 3",
                Colonia = "Zona Universitaria",
                Cp = "91090",
                Municipio = "Xalapa",
                Telefono = "(228) 863-18-90",
                Conmutador = "(228) 863-44-01",
                Extension = "12708",
                Fax = "(228) 863-49-61"
            }
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerTodosAsync())
            .ReturnsAsync(areasAcademicas);

        var resultado = await _areaAcademicaService.ObtenerTodasAsync();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Equal("Dirección General del Área Académica de Artes", resultado[0].Nombre);
        Assert.Equal("Lomas del Estadio S/N Edificio A Piso 2 Col. Zona Universitaria C.P. 91090 Xalapa", resultado[0].Domicilio);
        Assert.Equal("Teléfono: (228) 842-17-07\nConmutador: (228) 842-17-00 Ext: 11707\nFax: (228) 842-27-57", resultado[0].Telefono);

        Assert.Equal("Dirección General del Área Académica de Humanidades", resultado[1].Nombre);
        Assert.Equal("Lomas del Estadio S/N Edificio A Piso 3 Col. Zona Universitaria C.P. 91090 Xalapa", resultado[1].Domicilio);
        Assert.Equal("Teléfono: (228) 863-18-90\nConmutador: (228) 863-44-01 Ext: 12708\nFax: (228) 863-49-61", resultado[1].Telefono);
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
                CalleNumero = "Lomas del Estadio S/N Edificio A Piso 3",
                Colonia = "Zona Universitaria",
                Cp = "91090",
                Municipio = "Xalapa",
                Telefono = "(228) 863-18-90",
                Conmutador = "(228) 863-44-01",
                Extension = "12708",
                Fax = "(228) 863-49-61"
            }
        };

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorNombreAsync(nombre))
            .ReturnsAsync(areasAcademicas);

        var resultado = await _areaAcademicaService.ObtenerPorNombreAsync(nombre);

        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Dirección General del Área Académica de Humanidades", resultado[0].Nombre);
        Assert.Equal("Lomas del Estadio S/N Edificio A Piso 3 Col. Zona Universitaria C.P. 91090 Xalapa", resultado[0].Domicilio);
        Assert.Equal("Teléfono: (228) 863-18-90\nConmutador: (228) 863-44-01 Ext: 12708\nFax: (228) 863-49-61", resultado[0].Telefono);
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
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 2",
            Colonia = "Zona Universitaria",
            Cp = "91090",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-07",
            Conmutador = "(228) 842-17-00",
            Extension = "11707",
            Fax = "(228) 842-27-57"
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
        Assert.Equal("Lomas del Estadio S/N Edificio A Piso 2", resultado.CalleNumero);
        Assert.Equal("Zona Universitaria", resultado.Colonia);
        Assert.Equal("91090", resultado.Cp);
        Assert.Equal("Xalapa", resultado.Municipio);
        Assert.Equal("(228) 842-17-07", resultado.Telefono);
        Assert.Equal("(228) 842-17-00", resultado.Conmutador);
        Assert.Equal("11707", resultado.Extension);
        Assert.Equal("(228) 842-27-57", resultado.Fax);

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
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 5",
            Colonia = "Zona Universitaria",
            Cp = "91096",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-67",
            Conmutador = "(228) 842-27-00",
            Extension = "11706",
            Fax = "(228) 842-72-57"
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
                areaAcademica.CalleNumero == dto.CalleNumero &&
                areaAcademica.Colonia == dto.Colonia &&
                areaAcademica.Cp == dto.Cp &&
                areaAcademica.Municipio == dto.Municipio &&
                areaAcademica.Telefono == dto.Telefono &&
                areaAcademica.Conmutador == dto.Conmutador &&
                areaAcademica.Extension == dto.Extension &&
                areaAcademica.Fax == dto.Fax)),
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
            CalleNumero = "Lomas del Estadio S/N Edificio A Piso 2",
            Colonia = "Zona Universitaria",
            Cp = "91090",
            Municipio = "Xalapa",
            Telefono = "(228) 842-17-07",
            Conmutador = "(228) 842-17-00",
            Extension = "11707",
            Fax = "(228) 842-27-57"
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
