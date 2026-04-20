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

    //CP-01
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

    //CP-05
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


    //CP-06
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

    //CP-07
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

    //CP-09
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

    //CP-12
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

    ////CP-02
    //[Fact]
    //public async Task CrearCoordinadorDgaa()
    //{
    //    var dto = new CrearUsuarioDTO
    //    {
    //        Nombre = "María López",
    //        Correo = "maria@uv.mx",
    //        Cargo = "Coordinadora",
    //        Rol = Constantes.CoordinadorDgaa,
    //        IdAreaAcademica = 20
    //    };

    //    _usuarioValidatorMock
    //        .Setup(v => v.ValidarCreacionAsync(It.IsAny<CrearUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(r => r.CrearAsync(It.IsAny<CoordinadorDgaa>()))
    //        .ReturnsAsync(30);

    //    var resultado = await _usuarioService.CrearAsync(dto);

    //    Assert.Equal(30, resultado);

    //    _usuarioValidatorMock.Verify(v => v.ValidarCreacionAsync(
    //        It.Is<CrearUsuarioDTO>(x =>
    //            x.Nombre == dto.Nombre &&
    //            x.Correo == dto.Correo &&
    //            x.Cargo == dto.Cargo &&
    //            x.Rol == dto.Rol &&
    //            x.IdAreaAcademica == dto.IdAreaAcademica)),
    //        Times.Once);

    //    _coordinadorDgaaRepositoryMock.Verify(r => r.CrearAsync(
    //        It.Is<CoordinadorDgaa>(c =>
    //            c.Nombre == dto.Nombre &&
    //            c.Correo == dto.Correo &&
    //            c.Cargo == dto.Cargo &&
    //            c.IdAreaAcademica == dto.IdAreaAcademica)),
    //        Times.Once);

    //    _coordinadorEaRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<CoordinadorEa>()), Times.Never);
    //}

    ////CP-11
    //[Fact]
    //public async Task ObtenerListaDeUsuarios()
    //{
    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 20,
    //        Nombre = "Técnica"
    //    };

    //    var entidadAcademica = new EntidadAcademica
    //    {
    //        IdEntidadAcademica = 10,
    //        IdAreaAcademica = 20,
    //        Nombre = "Facultad de Psicología",
    //        Region = "Xalapa",
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 2,
    //        Nombre = "Zaira Alarcón",
    //        Correo = "zaria@uv.mx",
    //        Cargo = "Directora de Facultad",
    //        IdEntidadAcademica = 10,
    //        IdEntidadAcademicaNavigation = entidadAcademica
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 1,
    //        Nombre = "Ana Lourdes",
    //        Correo = "ana@uv.mx",
    //        Cargo = "Jefa de Unidad",
    //        IdAreaAcademica = 20,
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerTodosAsync())
    //        .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerTodosAsync())
    //        .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

    //    var resultado = await _usuarioService.ObtenerTodosAsync();

    //    Assert.Equal(2, resultado.Count);
    //    Assert.Equal("Ana Lourdes", resultado[0].Nombre);
    //    Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
    //    Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
    //    Assert.Equal("Zaira Alarcón", resultado[1].Nombre);
    //    Assert.Equal(Constantes.CoordinadorEa, resultado[1].Rol);
    //    Assert.Equal("Facultad de Psicología", resultado[1].NombreEntidadAcademica);
    //    Assert.Equal("Técnica", resultado[1].NombreAreaAcademica);
    //    Assert.Equal("Xalapa", resultado[1].Region);
    //}

    ////CP-12
    //[Fact]
    //public async Task ObtenerListaDeUsuariosConFiltroDeCoordinadorDgaa()
    //{
    //    var filtro = new FiltrosUsuarioDTO
    //    {
    //        Rol = Constantes.CoordinadorDgaa,
    //        Region = null,
    //        IdAreaAcademica = 20,
    //        IdEntidadAcademica = null
    //    };

    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 20,
    //        Nombre = "Técnica"
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 1,
    //        Nombre = "Ana Lourdes",
    //        Correo = "ana@uv.mx",
    //        Cargo = "Jefa de Unidad",
    //        IdAreaAcademica = 20,
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorFiltrosAsync(20))
    //        .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

    //    var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

    //    Assert.Single(resultado);
    //    Assert.Equal(1, resultado[0].IdUsuario);
    //    Assert.Equal("Ana Lourdes", resultado[0].Nombre);
    //    Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
    //    Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);

    //    _coordinadorEaRepositoryMock.Verify(
    //        repository => repository.ObtenerPorFiltrosAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>()),
    //        Times.Never);
    //}

    ////CP-13
    //[Fact]
    //public async Task ObtenerListaDeUsuariosConFiltroDeCoordinadorEa()
    //{
    //    var filtro = new FiltrosUsuarioDTO
    //    {
    //        Rol = Constantes.CoordinadorEa,
    //        Region = "Xalapa",
    //        IdAreaAcademica = 8,
    //        IdEntidadAcademica = 10
    //    };

    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 8,
    //        Nombre = "Técnica"
    //    };

    //    var entidadAcademica = new EntidadAcademica
    //    {
    //        IdEntidadAcademica = 10,
    //        IdAreaAcademica = 8,
    //        Nombre = "Facultad de Psicología",
    //        Region = "Xalapa",
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 2,
    //        Nombre = "Zaira Alarcón",
    //        Correo = "zaria@uv.mx",
    //        Cargo = "Directora de Facultad",
    //        IdEntidadAcademica = 10,
    //        IdEntidadAcademicaNavigation = entidadAcademica
    //    };

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorFiltrosAsync("Xalapa", 8, 10))
    //        .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

    //    var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

    //    Assert.Single(resultado);
    //    Assert.Equal(2, resultado[0].IdUsuario);
    //    Assert.Equal("Zaira Alarcón", resultado[0].Nombre);
    //    Assert.Equal(Constantes.CoordinadorEa, resultado[0].Rol);
    //    Assert.Equal("Facultad de Psicología", resultado[0].NombreEntidadAcademica);
    //    Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
    //    Assert.Equal("Xalapa", resultado[0].Region);

    //    _coordinadorDgaaRepositoryMock.Verify(repository => repository.ObtenerPorFiltrosAsync(It.IsAny<int?>()), Times.Never);
    //}

    ////CP-14
    //[Fact]
    //public async Task ObtenerListaDeUsuariosConFiltrosVacios()
    //{
    //    var filtro = new FiltrosUsuarioDTO
    //    {
    //        Rol = null,
    //        Region = null,
    //        IdAreaAcademica = null,
    //        IdEntidadAcademica = null
    //    };

    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 20,
    //        Nombre = "Técnica"
    //    };

    //    var entidadAcademica = new EntidadAcademica
    //    {
    //        IdEntidadAcademica = 10,
    //        IdAreaAcademica = 20,
    //        Nombre = "Facultad de Psicología",
    //        Region = "Xalapa",
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 2,
    //        Nombre = "Zaira Alarcón",
    //        Correo = "zaria@uv.mx",
    //        Cargo = "Directora de Facultad",
    //        IdEntidadAcademica = 10,
    //        IdEntidadAcademicaNavigation = entidadAcademica
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 1,
    //        Nombre = "Ana Lourdes",
    //        Correo = "ana@uv.mx",
    //        Cargo = "Jefa de Unidad",
    //        IdAreaAcademica = 20,
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorFiltrosAsync(null, null, null))
    //        .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorFiltrosAsync((int?)null))
    //        .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

    //    var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

    //    Assert.Equal(2, resultado.Count);
    //    Assert.Equal("Ana Lourdes", resultado[0].Nombre);
    //    Assert.Equal("Zaira Alarcón", resultado[1].Nombre);
    //}

    ////CP-15
    //[Fact]
    //public async Task ObtenerCoordinadorEa()
    //{
    //    var dto = new ReferenciaUsuarioDTO
    //    {
    //        IdUsuario = 16,
    //        Rol = Constantes.CoordinadorEa
    //    };

    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 6,
    //        Nombre = "Artes"
    //    };

    //    var entidadAcademica = new EntidadAcademica
    //    {
    //        IdEntidadAcademica = 321,
    //        IdAreaAcademica = 6,
    //        Nombre = "Facultad de Música",
    //        Region = "Xalapa",
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 16,
    //        IdEntidadAcademica = 321,
    //        Nombre = "Eliberto Charis",
    //        Correo = "eliberto@uv.mx",
    //        Cargo = "Secretario Académico",
    //        IdEntidadAcademicaNavigation = entidadAcademica
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(16))
    //        .ReturnsAsync(coordinadorEa);

    //    var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

    //    Assert.NotNull(resultado);
    //    Assert.Equal(16, resultado.IdUsuario);
    //    Assert.Equal("Eliberto Charis", resultado.Nombre);
    //    Assert.Equal("eliberto@uv.mx", resultado.Correo);
    //    Assert.Equal("Secretario Académico", resultado.Cargo);
    //    Assert.Equal(Constantes.CoordinadorEa, resultado.Rol);
    //    Assert.Equal(6, resultado.IdAreaAcademica);
    //    Assert.Equal("Artes", resultado.NombreAreaAcademica);
    //    Assert.Equal(321, resultado.IdEntidadAcademica);
    //    Assert.Equal("Facultad de Música", resultado.NombreEntidadAcademica);
    //    Assert.Equal("Xalapa", resultado.Region);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
    //        It.Is<ReferenciaUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol)),
    //        Times.Once);
    //}

    ////CP-16
    //[Fact]
    //public async Task ObtenerCoordinadorDgaa()
    //{
    //    var dto = new ReferenciaUsuarioDTO
    //    {
    //        IdUsuario = 56,
    //        Rol = Constantes.CoordinadorDgaa
    //    };

    //    var areaAcademica = new AreaAcademica
    //    {
    //        IdAreaAcademica = 3,
    //        Nombre = "Ciencias"
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 56,
    //        IdAreaAcademica = 3,
    //        Nombre = "Jaime Nunó",
    //        Correo = "jaime@uv.mx",
    //        Cargo = "Director General",
    //        IdAreaAcademicaNavigation = areaAcademica
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(56))
    //        .ReturnsAsync(coordinadorDgaa);

    //    var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

    //    Assert.NotNull(resultado);
    //    Assert.Equal(56, resultado.IdUsuario);
    //    Assert.Equal("Jaime Nunó", resultado.Nombre);
    //    Assert.Equal("jaime@uv.mx", resultado.Correo);
    //    Assert.Equal("Director General", resultado.Cargo);
    //    Assert.Equal(Constantes.CoordinadorDgaa, resultado.Rol);
    //    Assert.Equal(3, resultado.IdAreaAcademica);
    //    Assert.Equal("Ciencias", resultado.NombreAreaAcademica);
    //    Assert.Null(resultado.IdEntidadAcademica);
    //    Assert.Null(resultado.NombreEntidadAcademica);
    //    Assert.Null(resultado.Region);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
    //        It.Is<ReferenciaUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol)),
    //        Times.Once);
    //}

    ////CP-20
    //[Fact]
    //public async Task EditarCoordinadorEa()
    //{
    //    var dto = new EditarUsuarioDTO
    //    {
    //        IdUsuario = 534,
    //        Rol = Constantes.CoordinadorEa,
    //        Nombre = "Miguel Ángel Bocanegra",
    //        Cargo = "Coordinador",
    //        IdEntidadAcademica = 200
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 534,
    //        IdEntidadAcademica = 200,
    //        Nombre = "Ángel Bocanegra",
    //        Correo = "angel@uv.mx",
    //        Cargo = "Jefe de Unidad"
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarEdicionAsync(It.IsAny<EditarUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(534))
    //        .ReturnsAsync(coordinadorEa);

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ActualizarAsync(It.IsAny<CoordinadorEa>()))
    //        .Returns(Task.CompletedTask);

    //    await _usuarioService.EditarAsync(dto);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarEdicionAsync(
    //        It.Is<EditarUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol &&
    //            usuario.Nombre == dto.Nombre &&
    //            usuario.Cargo == dto.Cargo &&
    //            usuario.IdEntidadAcademica == dto.IdEntidadAcademica)),
    //        Times.Once);

    //    _coordinadorEaRepositoryMock.Verify(repository => repository.ActualizarAsync(
    //        It.Is<CoordinadorEa>(usuario =>
    //            usuario.IdCoordinadorEa == 534 &&
    //            usuario.Nombre == "Miguel Ángel Bocanegra" &&
    //            usuario.Cargo == "Coordinador" &&
    //            usuario.IdEntidadAcademica == 200)),
    //        Times.Once);
    //}

    ////CP-21
    //[Fact]
    //public async Task EditarCoordinadorDgaa()
    //{
    //    var dto = new EditarUsuarioDTO
    //    {
    //        IdUsuario = 453,
    //        Rol = Constantes.CoordinadorDgaa,
    //        Nombre = "Luisa Londóñes",
    //        Cargo = "Administradora Jefe",
    //        IdAreaAcademica = 2
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 453,
    //        IdAreaAcademica = 2,
    //        Nombre = "Luisa Londoño",
    //        Correo = "luisa@uv.mx",
    //        Cargo = "Administradora Jefe"
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarEdicionAsync(It.IsAny<EditarUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(453))
    //        .ReturnsAsync(coordinadorDgaa);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ActualizarAsync(It.IsAny<CoordinadorDgaa>()))
    //        .Returns(Task.CompletedTask);

    //    await _usuarioService.EditarAsync(dto);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarEdicionAsync(
    //        It.Is<EditarUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol &&
    //            usuario.Nombre == dto.Nombre &&
    //            usuario.Cargo == dto.Cargo &&
    //            usuario.IdAreaAcademica == dto.IdAreaAcademica)),
    //        Times.Once);

    //    _coordinadorDgaaRepositoryMock.Verify(repository => repository.ActualizarAsync(
    //        It.Is<CoordinadorDgaa>(usuario =>
    //            usuario.IdCoordinadorDgaa == 453 &&
    //            usuario.Nombre == "Luisa Londóñes" &&
    //            usuario.Cargo == "Administradora Jefe" &&
    //            usuario.IdAreaAcademica == 2)),
    //        Times.Once);
    //}

    ////CP-29
    //[Fact]
    //public async Task EliminarCoordinadorEa()
    //{
    //    var dto = new ReferenciaUsuarioDTO
    //    {
    //        IdUsuario = 16,
    //        Rol = Constantes.CoordinadorEa
    //    };

    //    var coordinadorEa = new CoordinadorEa
    //    {
    //        IdCoordinadorEa = 16,
    //        IdEntidadAcademica = 321,
    //        Nombre = "Eliberto Charis",
    //        Correo = "eliberto@uv.mx",
    //        Cargo = "Secretario Académico"
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(16))
    //        .ReturnsAsync(coordinadorEa);

    //    _coordinadorEaRepositoryMock
    //        .Setup(repository => repository.EliminarAsync(It.IsAny<CoordinadorEa>()))
    //        .Returns(Task.CompletedTask);

    //    await _usuarioService.EliminarAsync(dto);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
    //        It.Is<ReferenciaUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol)),
    //        Times.Once);

    //    _coordinadorEaRepositoryMock.Verify(repository => repository.EliminarAsync(
    //        It.Is<CoordinadorEa>(usuario => usuario.IdCoordinadorEa == 16)),
    //        Times.Once);
    //}

    ////CP-30
    //[Fact]
    //public async Task EliminarCoordinadorDgaa()
    //{
    //    var dto = new ReferenciaUsuarioDTO
    //    {
    //        IdUsuario = 56,
    //        Rol = Constantes.CoordinadorDgaa
    //    };

    //    var coordinadorDgaa = new CoordinadorDgaa
    //    {
    //        IdCoordinadorDgaa = 56,
    //        IdAreaAcademica = 3,
    //        Nombre = "Jaime Nunó",
    //        Correo = "jaime@uv.mx",
    //        Cargo = "Director General"
    //    };

    //    _usuarioValidatorMock
    //        .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
    //        .Returns(Task.CompletedTask);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.ObtenerPorIdAsync(56))
    //        .ReturnsAsync(coordinadorDgaa);

    //    _coordinadorDgaaRepositoryMock
    //        .Setup(repository => repository.EliminarAsync(It.IsAny<CoordinadorDgaa>()))
    //        .Returns(Task.CompletedTask);

    //    await _usuarioService.EliminarAsync(dto);

    //    _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
    //        It.Is<ReferenciaUsuarioDTO>(usuario =>
    //            usuario.IdUsuario == dto.IdUsuario &&
    //            usuario.Rol == dto.Rol)),
    //        Times.Once);

    //    _coordinadorDgaaRepositoryMock.Verify(repository => repository.EliminarAsync(
    //        It.Is<CoordinadorDgaa>(usuario => usuario.IdCoordinadorDgaa == 56)),
    //        Times.Once);
    //}
}