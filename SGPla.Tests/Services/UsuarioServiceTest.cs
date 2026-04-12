using Moq;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models;
using SGPla.Commons;
using SGPla.Validations.Interfaz;
public class UsuarioServiceTests
{
    private readonly Mock<ICoordinadorEaRepository> _coordinadorEaRepositoryMock;
    private readonly Mock<ICoordinadorDgaaRepository> _coordinadorDgaaRepositoryMock;
    private readonly Mock<IUsuarioValidator> _usuarioValidatorMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceTests()
    {
        _coordinadorEaRepositoryMock = new Mock<ICoordinadorEaRepository>();
        _coordinadorDgaaRepositoryMock = new Mock<ICoordinadorDgaaRepository>();
        _usuarioValidatorMock = new Mock<IUsuarioValidator>();

        _usuarioService = new UsuarioService(
            _coordinadorEaRepositoryMock.Object,
            _coordinadorDgaaRepositoryMock.Object,
            _usuarioValidatorMock.Object);
    }

    //CP-01
    [Fact]
    public async Task CrearCoordinadorEa()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Juan Pérez",
            Correo = "juan@uv.mx",
            Cargo = "Coordinador",
            Rol = Constantes.CoordinadorEa,
            IdEntidadAcademica = 10
        };

        _usuarioValidatorMock
            .Setup(v => v.ValidarCreacionAsync(It.IsAny<CrearUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorEaRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<CoordinadorEa>()))
            .ReturnsAsync(15);

        var resultado = await _usuarioService.CrearAsync(dto);

        Assert.Equal(15, resultado);

        _usuarioValidatorMock.Verify(v => v.ValidarCreacionAsync(
            It.Is<CrearUsuarioDTO>(x =>
                x.Nombre == dto.Nombre &&
                x.Correo == dto.Correo &&
                x.Cargo == dto.Cargo &&
                x.Rol == dto.Rol &&
                x.IdEntidadAcademica == dto.IdEntidadAcademica)),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(r => r.CrearAsync(
            It.Is<CoordinadorEa>(c =>
                c.Nombre == dto.Nombre &&
                c.Correo == dto.Correo &&
                c.Cargo == dto.Cargo &&
                c.IdEntidadAcademica == dto.IdEntidadAcademica)),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<CoordinadorDgaa>()), Times.Never);
    }

    //CP-02
    [Fact]
    public async Task CrearCoordinadorDgaa()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "María López",
            Correo = "maria@uv.mx",
            Cargo = "Coordinadora",
            Rol = Constantes.CoordinadorDgaa,
            IdAreaAcademica = 20
        };

        _usuarioValidatorMock
            .Setup(v => v.ValidarCreacionAsync(It.IsAny<CrearUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<CoordinadorDgaa>()))
            .ReturnsAsync(30);

        var resultado = await _usuarioService.CrearAsync(dto);

        Assert.Equal(30, resultado);

        _usuarioValidatorMock.Verify(v => v.ValidarCreacionAsync(
            It.Is<CrearUsuarioDTO>(x =>
                x.Nombre == dto.Nombre &&
                x.Correo == dto.Correo &&
                x.Cargo == dto.Cargo &&
                x.Rol == dto.Rol &&
                x.IdAreaAcademica == dto.IdAreaAcademica)),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(r => r.CrearAsync(
            It.Is<CoordinadorDgaa>(c =>
                c.Nombre == dto.Nombre &&
                c.Correo == dto.Correo &&
                c.Cargo == dto.Cargo &&
                c.IdAreaAcademica == dto.IdAreaAcademica)),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<CoordinadorEa>()), Times.Never);
    }

    //CP-11
    [Fact]
    public async Task ObtenerListaDeUsuarios()
    {
        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 20,
            Nombre = "Técnica"
        };

        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 10,
            IdAreaAcademica = 20,
            Nombre = "Facultad de Psicología",
            Region = "Xalapa",
            IdAreaAcademicaNavigation = areaAcademica
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 2,
            Nombre = "Zaira Alarcón",
            Correo = "zaria@uv.mx",
            Cargo = "Directora de Facultad",
            IdEntidadAcademica = 10,
            IdEntidadAcademicaNavigation = entidadAcademica
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 1,
            Nombre = "Ana Lourdes",
            Correo = "ana@uv.mx",
            Cargo = "Jefa de Unidad",
            IdAreaAcademica = 20,
            IdAreaAcademicaNavigation = areaAcademica
        };

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerTodosAsync())
            .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerTodosAsync())
            .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

        var resultado = await _usuarioService.ObtenerTodosAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Ana Lourdes", resultado[0].Nombre);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
        Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
        Assert.Equal("Zaira Alarcón", resultado[1].Nombre);
        Assert.Equal(Constantes.CoordinadorEa, resultado[1].Rol);
        Assert.Equal("Facultad de Psicología", resultado[1].NombreEntidadAcademica);
        Assert.Equal("Técnica", resultado[1].NombreAreaAcademica);
        Assert.Equal("Xalapa", resultado[1].Region);
    }

    //CP-12
    [Fact]
    public async Task ObtenerListaDeUsuariosConFiltroDeCoordinadorDgaa()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = Constantes.CoordinadorDgaa,
            Region = null,
            IdAreaAcademica = 20,
            IdEntidadAcademica = null
        };

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 20,
            Nombre = "Técnica"
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 1,
            Nombre = "Ana Lourdes",
            Correo = "ana@uv.mx",
            Cargo = "Jefa de Unidad",
            IdAreaAcademica = 20,
            IdAreaAcademicaNavigation = areaAcademica
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorFiltrosAsync(20))
            .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Single(resultado);
        Assert.Equal(1, resultado[0].IdUsuario);
        Assert.Equal("Ana Lourdes", resultado[0].Nombre);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
        Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);

        _coordinadorEaRepositoryMock.Verify(
            repository => repository.ObtenerPorFiltrosAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>()),
            Times.Never);
    }

    //CP-13
    [Fact]
    public async Task ObtenerListaDeUsuariosConFiltroDeCoordinadorEa()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = Constantes.CoordinadorEa,
            Region = "Xalapa",
            IdAreaAcademica = 8,
            IdEntidadAcademica = 10
        };

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 8,
            Nombre = "Técnica"
        };

        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 10,
            IdAreaAcademica = 8,
            Nombre = "Facultad de Psicología",
            Region = "Xalapa",
            IdAreaAcademicaNavigation = areaAcademica
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 2,
            Nombre = "Zaira Alarcón",
            Correo = "zaria@uv.mx",
            Cargo = "Directora de Facultad",
            IdEntidadAcademica = 10,
            IdEntidadAcademicaNavigation = entidadAcademica
        };

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorFiltrosAsync("Xalapa", 8, 10))
            .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Single(resultado);
        Assert.Equal(2, resultado[0].IdUsuario);
        Assert.Equal("Zaira Alarcón", resultado[0].Nombre);
        Assert.Equal(Constantes.CoordinadorEa, resultado[0].Rol);
        Assert.Equal("Facultad de Psicología", resultado[0].NombreEntidadAcademica);
        Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
        Assert.Equal("Xalapa", resultado[0].Region);

        _coordinadorDgaaRepositoryMock.Verify(repository => repository.ObtenerPorFiltrosAsync(It.IsAny<int?>()), Times.Never);
    }

    //CP-14
    [Fact]
    public async Task ObtenerListaDeUsuariosConFiltrosVacios()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = null,
            Region = null,
            IdAreaAcademica = null,
            IdEntidadAcademica = null
        };

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 20,
            Nombre = "Técnica"
        };

        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 10,
            IdAreaAcademica = 20,
            Nombre = "Facultad de Psicología",
            Region = "Xalapa",
            IdAreaAcademicaNavigation = areaAcademica
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 2,
            Nombre = "Zaira Alarcón",
            Correo = "zaria@uv.mx",
            Cargo = "Directora de Facultad",
            IdEntidadAcademica = 10,
            IdEntidadAcademicaNavigation = entidadAcademica
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 1,
            Nombre = "Ana Lourdes",
            Correo = "ana@uv.mx",
            Cargo = "Jefa de Unidad",
            IdAreaAcademica = 20,
            IdAreaAcademicaNavigation = areaAcademica
        };

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorFiltrosAsync(null, null, null))
            .ReturnsAsync(new List<CoordinadorEa> { coordinadorEa });

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorFiltrosAsync((int?)null))
            .ReturnsAsync(new List<CoordinadorDgaa> { coordinadorDgaa });

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Ana Lourdes", resultado[0].Nombre);
        Assert.Equal("Zaira Alarcón", resultado[1].Nombre);
    }

    //CP-15
    [Fact]
    public async Task ObtenerCoordinadorEa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 16,
            Rol = Constantes.CoordinadorEa
        };

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 6,
            Nombre = "Artes"
        };

        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 321,
            IdAreaAcademica = 6,
            Nombre = "Facultad de Música",
            Region = "Xalapa",
            IdAreaAcademicaNavigation = areaAcademica
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 16,
            IdEntidadAcademica = 321,
            Nombre = "Eliberto Charis",
            Correo = "eliberto@uv.mx",
            Cargo = "Secretario Académico",
            IdEntidadAcademicaNavigation = entidadAcademica
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(16))
            .ReturnsAsync(coordinadorEa);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(16, resultado.IdUsuario);
        Assert.Equal("Eliberto Charis", resultado.Nombre);
        Assert.Equal("eliberto@uv.mx", resultado.Correo);
        Assert.Equal("Secretario Académico", resultado.Cargo);
        Assert.Equal(Constantes.CoordinadorEa, resultado.Rol);
        Assert.Equal(6, resultado.IdAreaAcademica);
        Assert.Equal("Artes", resultado.NombreAreaAcademica);
        Assert.Equal(321, resultado.IdEntidadAcademica);
        Assert.Equal("Facultad de Música", resultado.NombreEntidadAcademica);
        Assert.Equal("Xalapa", resultado.Region);

        _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
            It.Is<ReferenciaUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol)),
            Times.Once);
    }

    //CP-16
    [Fact]
    public async Task ObtenerCoordinadorDgaa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 56,
            Rol = Constantes.CoordinadorDgaa
        };

        var areaAcademica = new AreaAcademica
        {
            IdAreaAcademica = 3,
            Nombre = "Ciencias"
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 56,
            IdAreaAcademica = 3,
            Nombre = "Jaime Nunó",
            Correo = "jaime@uv.mx",
            Cargo = "Director General",
            IdAreaAcademicaNavigation = areaAcademica
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(56))
            .ReturnsAsync(coordinadorDgaa);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(56, resultado.IdUsuario);
        Assert.Equal("Jaime Nunó", resultado.Nombre);
        Assert.Equal("jaime@uv.mx", resultado.Correo);
        Assert.Equal("Director General", resultado.Cargo);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado.Rol);
        Assert.Equal(3, resultado.IdAreaAcademica);
        Assert.Equal("Ciencias", resultado.NombreAreaAcademica);
        Assert.Null(resultado.IdEntidadAcademica);
        Assert.Null(resultado.NombreEntidadAcademica);
        Assert.Null(resultado.Region);

        _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
            It.Is<ReferenciaUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol)),
            Times.Once);
    }

    //CP-20
    [Fact]
    public async Task EditarCoordinadorEa()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 534,
            Rol = Constantes.CoordinadorEa,
            Nombre = "Miguel Ángel Bocanegra",
            Cargo = "Coordinador",
            IdEntidadAcademica = 200
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 534,
            IdEntidadAcademica = 200,
            Nombre = "Ángel Bocanegra",
            Correo = "angel@uv.mx",
            Cargo = "Jefe de Unidad"
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarEdicionAsync(It.IsAny<EditarUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(534))
            .ReturnsAsync(coordinadorEa);

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ActualizarAsync(It.IsAny<CoordinadorEa>()))
            .Returns(Task.CompletedTask);

        await _usuarioService.EditarAsync(dto);

        _usuarioValidatorMock.Verify(validator => validator.ValidarEdicionAsync(
            It.Is<EditarUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol &&
                usuario.Nombre == dto.Nombre &&
                usuario.Cargo == dto.Cargo &&
                usuario.IdEntidadAcademica == dto.IdEntidadAcademica)),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(repository => repository.ActualizarAsync(
            It.Is<CoordinadorEa>(usuario =>
                usuario.IdCoordinadorEa == 534 &&
                usuario.Nombre == "Miguel Ángel Bocanegra" &&
                usuario.Cargo == "Coordinador" &&
                usuario.IdEntidadAcademica == 200)),
            Times.Once);
    }

    //CP-21
    [Fact]
    public async Task EditarCoordinadorDgaa()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 453,
            Rol = Constantes.CoordinadorDgaa,
            Nombre = "Luisa Londóñes",
            Cargo = "Administradora Jefe",
            IdAreaAcademica = 2
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 453,
            IdAreaAcademica = 2,
            Nombre = "Luisa Londoño",
            Correo = "luisa@uv.mx",
            Cargo = "Administradora Jefe"
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarEdicionAsync(It.IsAny<EditarUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(453))
            .ReturnsAsync(coordinadorDgaa);

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ActualizarAsync(It.IsAny<CoordinadorDgaa>()))
            .Returns(Task.CompletedTask);

        await _usuarioService.EditarAsync(dto);

        _usuarioValidatorMock.Verify(validator => validator.ValidarEdicionAsync(
            It.Is<EditarUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol &&
                usuario.Nombre == dto.Nombre &&
                usuario.Cargo == dto.Cargo &&
                usuario.IdAreaAcademica == dto.IdAreaAcademica)),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(repository => repository.ActualizarAsync(
            It.Is<CoordinadorDgaa>(usuario =>
                usuario.IdCoordinadorDgaa == 453 &&
                usuario.Nombre == "Luisa Londóñes" &&
                usuario.Cargo == "Administradora Jefe" &&
                usuario.IdAreaAcademica == 2)),
            Times.Once);
    }

    //CP-29
    [Fact]
    public async Task EliminarCoordinadorEa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 16,
            Rol = Constantes.CoordinadorEa
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 16,
            IdEntidadAcademica = 321,
            Nombre = "Eliberto Charis",
            Correo = "eliberto@uv.mx",
            Cargo = "Secretario Académico"
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(16))
            .ReturnsAsync(coordinadorEa);

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.EliminarAsync(It.IsAny<CoordinadorEa>()))
            .Returns(Task.CompletedTask);

        await _usuarioService.EliminarAsync(dto);

        _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
            It.Is<ReferenciaUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol)),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(repository => repository.EliminarAsync(
            It.Is<CoordinadorEa>(usuario => usuario.IdCoordinadorEa == 16)),
            Times.Once);
    }

    //CP-30
    [Fact]
    public async Task EliminarCoordinadorDgaa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 56,
            Rol = Constantes.CoordinadorDgaa
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 56,
            IdAreaAcademica = 3,
            Nombre = "Jaime Nunó",
            Correo = "jaime@uv.mx",
            Cargo = "Director General"
        };

        _usuarioValidatorMock
            .Setup(validator => validator.ValidarReferenciaAsync(It.IsAny<ReferenciaUsuarioDTO>()))
            .Returns(Task.CompletedTask);

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(56))
            .ReturnsAsync(coordinadorDgaa);

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.EliminarAsync(It.IsAny<CoordinadorDgaa>()))
            .Returns(Task.CompletedTask);

        await _usuarioService.EliminarAsync(dto);

        _usuarioValidatorMock.Verify(validator => validator.ValidarReferenciaAsync(
            It.Is<ReferenciaUsuarioDTO>(usuario =>
                usuario.IdUsuario == dto.IdUsuario &&
                usuario.Rol == dto.Rol)),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(repository => repository.EliminarAsync(
            It.Is<CoordinadorDgaa>(usuario => usuario.IdCoordinadorDgaa == 56)),
            Times.Once);
    }
}