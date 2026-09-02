using Moq;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Articulo;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Validations.Interfaces;

public class ArticuloValidatorTests
{
    private readonly Mock<IArticuloRepository> _articuloRepositoryMock;
    private readonly ArticuloValidator _articuloValidator;


    public ArticuloValidatorTests()
    {
       _articuloRepositoryMock = new Mock<IArticuloRepository>();

        _articuloValidator = new ArticuloValidator(_articuloRepositoryMock.Object);
    }


    //CP-02-02
    [Fact]
    public async Task CrearArticuloSinNumero()
    {
        var dto = new CrearArticuloDTO
        {
            Numero = "Cincuenta",
            Descripcion = "Ejemplo de descripcion de articulo",
        };

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El número del artículo debe contener al menos un número.", ex.Message);
    }

    //CP-02-03
    [Fact]
    public async Task CrearArticuloConNumeroRepetido()
    {
        var dto = new CrearArticuloDTO
        {
            Numero = "30",
            Descripcion = "Ejemplo de descripcion de articulo",
        };

        var articuloPrevio = new Articulo
        {
            Numero = "30",
            Descripcion = "Ejemplo de articulo repetido",
            IdArticulo = 1
        };

        _articuloRepositoryMock
            .Setup(r => r.ExisteAsync(dto.Numero))
            .ReturnsAsync(articuloPrevio);


        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal($"El número de articulo '{dto.Numero}' ya existe. Por favor, elija un número diferente.", ex.Message);
    }

    //CP-02-04
    [Fact]
    public async Task CrearArticuloConValoresNulos()
    {
        var dto = new CrearArticuloDTO
        {
            Numero = null,
            Descripcion = null,
        };

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El número del artículo es obligatorio.", ex.Message);
    }

    //CP-02-07
    [Fact]
    public async Task ObtenerArticulosConCadenaVacia()
    {
        string busqueda = "";

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarBusquedaPorTerminoAsync(busqueda));

        Assert.NotNull(ex);
        Assert.Equal("La cadena de búsqueda no puede estar vacía.", ex.Message);
    }

    //CP-02-09
    [Fact]
    public async Task ObtenerArticuloPorIdConIdInvalido()
    {
        int id = 0;

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarObtenerPorIdAsync(id));  
        Assert.NotNull(ex);
        Assert.Equal("El ID del artículo no es válido.", ex.Message);
    }

    //CP-02-11
    [Fact]
    public async Task EditarArticuloConValoresNulos()
    {
        var dto = new EditarArticuloDTO
        {
            Numero = null,
            Descripcion = null,
        };

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El número del artículo es obligatorio.", ex.Message);
    }

    //CP-02-12
    [Fact]
    public async Task EditarArticuloNumeroRepetido()
    {
        var articulo = new Articulo
        {
            IdArticulo = 1,
            Numero = "70",
            Descripcion = "Ejemplo de descripcion"
        };
        var dto = new EditarArticuloDTO
        {
            IdArticulo = 2,
            Numero = "70",
            Descripcion = "Nuevo ejemplo de descripcion",
        };

        _articuloRepositoryMock.Setup(r => r.ObtenerArticuloPorIdAsync(dto.IdArticulo))
            .ReturnsAsync(new Articulo
            {
                IdArticulo = dto.IdArticulo,
                Numero = "70",
                Descripcion = "Ejemplo de descripcion"
            });

        _articuloRepositoryMock
           .Setup(r => r.ExisteAsync(dto.Numero))
           .ReturnsAsync(articulo);

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal($"El número de articulo '{dto.Numero}' ya existe. Por favor, elija un número diferente.", ex.Message);
    }

    //CP-02-14
    [Fact]
    public async Task EliminarArticuloConIdInvalido()
    {
        int id = 0;

        var ex = await Record.ExceptionAsync(() => _articuloValidator.ValidarEliminarAsync(id));
        Assert.NotNull(ex);
        Assert.Equal("El ID del artículo no es válido.", ex.Message);
    }
}