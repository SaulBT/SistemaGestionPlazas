using Moq;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models;
using SGPla.Commons;
using SGPla.Validations.Interfaces;
using SGPla.Models.DTOs.Articulo;
public class ArticuloServiceTests
{
    private readonly Mock<IArticuloRepository> _articuloRepositoryMock;
    private readonly Mock<IArticuloValidator> _articuloValidatorMock;

    private readonly ArticuloService _articuloService;

    public ArticuloServiceTests()
    {
        _articuloValidatorMock = new Mock<IArticuloValidator>();
        _articuloRepositoryMock = new Mock<IArticuloRepository>();

        _articuloService = new ArticuloService(
            _articuloRepositoryMock.Object,
        _articuloValidatorMock.Object);
    }

    //CP-67
    [Fact]
    public async Task CrearArticulo()
    {
        var dto = new CrearArticuloDTO
        {
            Numero = "70",
            Descripcion = "Ejemplo de descripcion de articulo",
        };

        _articuloValidatorMock
            .Setup(v => v.ValidarCreacionAsync(It.IsAny<CrearArticuloDTO>()))
            .Returns(Task.CompletedTask);

        _articuloRepositoryMock
            .Setup(r => r.CrearArticuloAsync(It.IsAny<Articulo>()))
            .ReturnsAsync(new Articulo
            {
                IdArticulo = 15,
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            });

        var resultado = await _articuloService.CrearArticuloAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(15, resultado.IdArticulo);
       
        _articuloValidatorMock.Verify(v => v.ValidarCreacionAsync(
            It.Is<CrearArticuloDTO>(x =>
                x.Numero == dto.Numero &&
                x.Descripcion == dto.Descripcion)),
            Times.Once);

        _articuloRepositoryMock.Verify(r => r.CrearArticuloAsync(
            It.Is<Articulo>(a =>
                a.Numero == dto.Numero &&
                a.Descripcion == dto.Descripcion)),
            Times.Once);
    }

    //CP-71
    [Fact]
    public async Task ObtenerTodosArticulos()
    {
        var listaArticulos = new List<Articulo>
        {
            new Articulo
            {
                IdArticulo = 1,
                Numero = "70",
                Descripcion = "Ejemplo de descripcion de articulo",
            },
            new Articulo
            {
                IdArticulo = 2,
                Numero = "73",
                Descripcion = "Ejemplo de descripcion de articulo"
            }
        };

        _articuloRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(listaArticulos);

        var resultado = (await _articuloService.ObtenerTodosAsync()).ToList();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Contains(resultado, x =>
            x.IdArticulo == 1 &&
            x.Numero == "70");

        Assert.Contains(resultado, x =>
            x.IdArticulo == 2 &&
            x.Numero == "73");

        _articuloRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }


    //CP-72
    [Fact]
    public async Task ObtenerArticulosPorTermino()
    {
        var listaArticulos = new List<Articulo>
        {
            new Articulo
            {
                IdArticulo = 2,
                Numero = "73",
                Descripcion = "Ejemplo de descripcion de articulo"
            }
        };

        _articuloRepositoryMock
            .Setup(r => r.BuscarPorTerminoAsync(It.IsAny<string>()))
            .ReturnsAsync(listaArticulos);

        var resultado = (await _articuloService.BuscarPorTerminoAsync("73")).ToList();

        Assert.NotNull(resultado);
        Assert.Single(resultado);

        Assert.Contains(resultado, x =>
            x.IdArticulo == 2 &&
            x.Numero == "73");

        _articuloRepositoryMock.Verify(r => r.BuscarPorTerminoAsync(It.IsAny<string>()), Times.Once);
    }

    //CP-74
    [Fact]
    public async Task ObtenerArticuloPorId()
    {
        var articulo =
            new Articulo
            {
                IdArticulo = 2,
                Numero = "73",
                Descripcion = "Ejemplo de descripcion de articulo"
            };

        _articuloRepositoryMock
            .Setup(r => r.ObtenerArticuloPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(articulo);

        var resultado = await _articuloService.ObtenerArticuloPorIdAsync(2);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.IdArticulo);
        Assert.Equal("73", resultado.Numero);
        Assert.Equal("Ejemplo de descripcion de articulo", resultado.Descripcion);

       

        _articuloRepositoryMock.Verify(r => r.ObtenerArticuloPorIdAsync(It.IsAny<int>()), Times.Once);
    }

    //CP-76
    [Fact]
    public async Task EditarArticulo()
    {
        var dto = new EditarArticuloDTO
        {
            Numero = "70",
            Descripcion = "Ejemplo de nueva descripcion de articulo",
        };

        _articuloValidatorMock
            .Setup(v => v.ValidarEdicionAsync(It.IsAny<EditarArticuloDTO>()))
            .Returns(Task.CompletedTask);

        _articuloRepositoryMock
            .Setup(r => r.EditarArticuloAsync(It.IsAny<Articulo>()))
            .ReturnsAsync(new Articulo
            {
                IdArticulo = 70,
                Numero = dto.Numero,
                Descripcion = dto.Descripcion
            });

        var resultado = await _articuloService.EditarArticuloAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(70, resultado.IdArticulo);
        Assert.Equal(dto.Numero, resultado.Numero);
        Assert.Equal(dto.Descripcion, resultado.Descripcion);

        _articuloValidatorMock.Verify(v => v.ValidarEdicionAsync(
            It.Is<EditarArticuloDTO>(x =>
                x.Numero == dto.Numero &&
                x.Descripcion == dto.Descripcion)),
            Times.Once);

        _articuloRepositoryMock.Verify(r => r.EditarArticuloAsync(
            It.Is<Articulo>(a =>
                a.Numero == dto.Numero &&
                a.Descripcion == dto.Descripcion)),
            Times.Once);
    }

    //CP-79
    [Fact]
    public async Task EliminarArticulo()
    {
        int idArticulo = 2;

        _articuloRepositoryMock
            .Setup(r => r.EliminarArticuloAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        var resultado = (await _articuloService.EliminarArticuloAsync(idArticulo));

        Assert.True(resultado);

        _articuloRepositoryMock.Verify(r => r.EliminarArticuloAsync(It.IsAny<int>()), Times.Once);
    }
}