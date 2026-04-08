using Moq;
using Xunit;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models;
using SGPla.Commons;
public class UsuarioServiceTests
{
    private readonly Mock<ICoordinadorEaRepository> _coordinadorEaRepositoryMock;
    private readonly Mock<ICoordinadorDgaaRepository> _coordinadorDgaaRepositoryMock;
    private readonly UsuarioService _usuarioService;

    public UsuarioServiceTests()
    {
        _coordinadorEaRepositoryMock = new Mock<ICoordinadorEaRepository>();
        _coordinadorDgaaRepositoryMock = new Mock<ICoordinadorDgaaRepository>();

        _usuarioService = new UsuarioService(
            _coordinadorEaRepositoryMock.Object,
            _coordinadorDgaaRepositoryMock.Object);
    }

    //CP-01
    [Fact]
    public async Task CrearAsync_CoordinadorEa()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Juan Pérez",
            Correo = "juan@uv.mx",
            Cargo = "Coordinador",
            Rol = Constantes.CoordinadorEa,
            IdEntidadAcademica = 10
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<CoordinadorEa>()))
            .ReturnsAsync(15);

        var resultado = await _usuarioService.CrearAsync(dto);

        Assert.Equal(15, resultado);

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
    public async Task CrearAsync_CoordinadorDgaa_RegresarId()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "María López",
            Correo = "maria@uv.mx",
            Cargo = "Coordinadora",
            Rol = Constantes.CoordinadorDgaa,
            IdAreaAcademica = 20
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<CoordinadorDgaa>()))
            .ReturnsAsync(30);

        var resultado = await _usuarioService.CrearAsync(dto);

        Assert.Equal(30, resultado);

        _coordinadorDgaaRepositoryMock.Verify(r => r.CrearAsync(
            It.Is<CoordinadorDgaa>(c =>
                c.Nombre == dto.Nombre &&
                c.Correo == dto.Correo &&
                c.Cargo == dto.Cargo &&
                c.IdAreaAcademica == dto.IdAreaAcademica)),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<CoordinadorEa>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerTodosAsync_ListaCombinadaOrdenadaPorNombre()
    {
        var coordinadoresEa = new List<CoordinadorEa>
        {
            new CoordinadorEa
            {
                IdCoordinadorEa = 2,
                Nombre = "Zaira",
                Correo = "zaira@uv.mx",
                Cargo = "Coordinadora EA",
                IdEntidadAcademica = 100,
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    IdEntidadAcademica = 100,
                    Nombre = "Facultad de Contaduría",
                    Region = "Xalapa",
                    IdAreaAcademica = 10,
                    IdAreaAcademicaNavigation = new AreaAcademica
                    {
                        IdAreaAcademica = 10,
                        Nombre = "Económico-Administrativa"
                    }
                }
            }
        };

        var coordinadoresDgaa = new List<CoordinadorDgaa>
        {
            new CoordinadorDgaa
            {
                IdCoordinadorDgaa = 1,
                Nombre = "Ana",
                Correo = "ana@uv.mx",
                Cargo = "Coordinadora DGAA",
                IdAreaAcademica = 20,
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    IdAreaAcademica = 20,
                    Nombre = "Técnica"
                }
            }
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(coordinadoresEa);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(coordinadoresDgaa);

        var resultado = await _usuarioService.ObtenerTodosAsync();

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Equal("Ana", resultado[0].Nombre);
        Assert.Equal("Zaira", resultado[1].Nombre);

        Assert.Equal(1, resultado[0].IdUsuario);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
        Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
        Assert.Null(resultado[0].NombreEntidadAcademica);
        Assert.Null(resultado[0].Region);

        Assert.Equal(2, resultado[1].IdUsuario);
        Assert.Equal(Constantes.CoordinadorEa, resultado[1].Rol);
        Assert.Equal("Económico-Administrativa", resultado[1].NombreAreaAcademica);
        Assert.Equal("Facultad de Contaduría", resultado[1].NombreEntidadAcademica);
        Assert.Equal("Xalapa", resultado[1].Region);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosAsync_ListaVacia()
    {
        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(new List<CoordinadorEa>());

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(new List<CoordinadorDgaa>());

        var resultado = await _usuarioService.ObtenerTodosAsync();

        Assert.NotNull(resultado);
        Assert.Empty(resultado);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosAsync_CoordinadorEaYCoordinadorDgaa()
    {
        var coordinadoresEa = new List<CoordinadorEa>
        {
            new CoordinadorEa
            {
                IdCoordinadorEa = 15,
                Nombre = "Luis",
                Correo = "luis@uv.mx",
                Cargo = "Coordinador EA",
                IdEntidadAcademica = 200,
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    IdEntidadAcademica = 200,
                    Nombre = "Facultad de Derecho",
                    Region = "Veracruz",
                    IdAreaAcademica = 30,
                    IdAreaAcademicaNavigation = new AreaAcademica
                    {
                        IdAreaAcademica = 30,
                        Nombre = "Humanidades"
                    }
                }
            }
        };

        var coordinadoresDgaa = new List<CoordinadorDgaa>
        {
            new CoordinadorDgaa
            {
                IdCoordinadorDgaa = 25,
                Nombre = "Mario",
                Correo = "mario@uv.mx",
                Cargo = "Coordinador DGAA",
                IdAreaAcademica = 40,
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    IdAreaAcademica = 40,
                    Nombre = "Biológico-Agropecuaria"
                }
            }
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(coordinadoresEa);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(coordinadoresDgaa);

        var resultado = await _usuarioService.ObtenerTodosAsync();

        var usuarioEa = resultado.First(x => x.Rol == Constantes.CoordinadorEa);
        var usuarioDgaa = resultado.First(x => x.Rol == Constantes.CoordinadorDgaa);

        Assert.Equal(15, usuarioEa.IdUsuario);
        Assert.Equal("Luis", usuarioEa.Nombre);
        Assert.Equal("luis@uv.mx", usuarioEa.Correo);
        Assert.Equal("Coordinador EA", usuarioEa.Cargo);
        Assert.Equal("Humanidades", usuarioEa.NombreAreaAcademica);
        Assert.Equal("Facultad de Derecho", usuarioEa.NombreEntidadAcademica);
        Assert.Equal("Veracruz", usuarioEa.Region);

        Assert.Equal(25, usuarioDgaa.IdUsuario);
        Assert.Equal("Mario", usuarioDgaa.Nombre);
        Assert.Equal("mario@uv.mx", usuarioDgaa.Correo);
        Assert.Equal("Coordinador DGAA", usuarioDgaa.Cargo);
        Assert.Equal("Biológico-Agropecuaria", usuarioDgaa.NombreAreaAcademica);
        Assert.Null(usuarioDgaa.NombreEntidadAcademica);
        Assert.Null(usuarioDgaa.Region);
    }

    //ObtenerPorFiltro
    [Fact]
    public async Task ObtenerPorFiltroAsync_CoordinadorEa()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = Constantes.CoordinadorEa,
            Region = "Xalapa",
            IdAreaAcademica = 10,
            IdEntidadAcademica = 100
        };

        var coordinadoresEa = new List<CoordinadorEa>
        {
            new CoordinadorEa
            {
                IdCoordinadorEa = 1,
                Nombre = "Brenda",
                Correo = "brenda@uv.mx",
                Cargo = "Coordinadora EA",
                IdEntidadAcademica = 100,
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    IdEntidadAcademica = 100,
                    Nombre = "Facultad de Pedagogía",
                    Region = "Xalapa",
                    IdAreaAcademica = 10,
                    IdAreaAcademicaNavigation = new AreaAcademica
                    {
                        IdAreaAcademica = 10,
                        Nombre = "Humanidades"
                    }
                }
            }
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica))
            .ReturnsAsync(coordinadoresEa);

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Single(resultado);
        Assert.Equal("Brenda", resultado[0].Nombre);
        Assert.Equal(Constantes.CoordinadorEa, resultado[0].Rol);
        Assert.Equal("Humanidades", resultado[0].NombreAreaAcademica);
        Assert.Equal("Facultad de Pedagogía", resultado[0].NombreEntidadAcademica);
        Assert.Equal("Xalapa", resultado[0].Region);

        _coordinadorEaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(It.IsAny<int?>()),
            Times.Never);
    }

    [Fact]
    public async Task ObtenerPorFiltroAsync_CoordinadorDgaa()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = Constantes.CoordinadorDgaa,
            IdAreaAcademica = 20
        };

        var coordinadoresDgaa = new List<CoordinadorDgaa>
        {
            new CoordinadorDgaa
            {
                IdCoordinadorDgaa = 2,
                Nombre = "Carlos",
                Correo = "carlos@uv.mx",
                Cargo = "Coordinador DGAA",
                IdAreaAcademica = 20,
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    IdAreaAcademica = 20,
                    Nombre = "Técnica"
                }
            }
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorFiltrosAsync(filtro.IdAreaAcademica))
            .ReturnsAsync(coordinadoresDgaa);

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Single(resultado);
        Assert.Equal("Carlos", resultado[0].Nombre);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
        Assert.Equal("Técnica", resultado[0].NombreAreaAcademica);
        Assert.Null(resultado[0].NombreEntidadAcademica);
        Assert.Null(resultado[0].Region);

        _coordinadorDgaaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(filtro.IdAreaAcademica),
            Times.Once);

        _coordinadorEaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>()),
            Times.Never);
    }

    [Fact]
    public async Task ObtenerPorFiltroAsync_NoSeEspecificaRol()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Region = "Veracruz",
            IdAreaAcademica = 30,
            IdEntidadAcademica = 200
        };

        var coordinadoresEa = new List<CoordinadorEa>
        {
            new CoordinadorEa
            {
                IdCoordinadorEa = 3,
                Nombre = "Zulema",
                Correo = "zulema@uv.mx",
                Cargo = "Coordinadora EA",
                IdEntidadAcademica = 200,
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    IdEntidadAcademica = 200,
                    Nombre = "Facultad de Derecho",
                    Region = "Veracruz",
                    IdAreaAcademica = 30,
                    IdAreaAcademicaNavigation = new AreaAcademica
                    {
                        IdAreaAcademica = 30,
                        Nombre = "Humanidades"
                    }
                }
            }
        };

        var coordinadoresDgaa = new List<CoordinadorDgaa>
        {
            new CoordinadorDgaa
            {
                IdCoordinadorDgaa = 4,
                Nombre = "Adriana",
                Correo = "adriana@uv.mx",
                Cargo = "Coordinadora DGAA",
                IdAreaAcademica = 30,
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    IdAreaAcademica = 30,
                    Nombre = "Humanidades"
                }
            }
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica))
            .ReturnsAsync(coordinadoresEa);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorFiltrosAsync(filtro.IdAreaAcademica))
            .ReturnsAsync(coordinadoresDgaa);

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Adriana", resultado[0].Nombre);
        Assert.Equal("Zulema", resultado[1].Nombre);

        Assert.Equal(Constantes.CoordinadorDgaa, resultado[0].Rol);
        Assert.Equal(Constantes.CoordinadorEa, resultado[1].Rol);

        _coordinadorEaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(filtro.IdAreaAcademica),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerPorFiltroAsync_ListaVacia()
    {
        var filtro = new FiltrosUsuarioDTO
        {
            Rol = Constantes.CoordinadorEa,
            Region = "Xalapa",
            IdAreaAcademica = 10,
            IdEntidadAcademica = 100
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica))
            .ReturnsAsync(new List<CoordinadorEa>());

        var resultado = await _usuarioService.ObtenerPorFiltroAsync(filtro);

        Assert.NotNull(resultado);
        Assert.Empty(resultado);

        _coordinadorEaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(filtro.Region, filtro.IdAreaAcademica, filtro.IdEntidadAcademica),
            Times.Once);

        _coordinadorDgaaRepositoryMock.Verify(
            r => r.ObtenerPorFiltrosAsync(It.IsAny<int?>()),
            Times.Never);
    }

    //ObtenerPorId
    [Fact]
    public async Task ObtenerPorIdAsync_CoordinadorEa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 1,
            Rol = Constantes.CoordinadorEa
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 1,
            Nombre = "Laura",
            Correo = "laura@uv.mx",
            Cargo = "Coordinadora EA",
            IdEntidadAcademica = 100,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                IdEntidadAcademica = 100,
                Nombre = "Facultad de Arquitectura",
                Region = "Xalapa",
                IdAreaAcademica = 10,
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    IdAreaAcademica = 10,
                    Nombre = "Técnica"
                }
            }
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorEa);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.IdUsuario);
        Assert.Equal("Laura", resultado.Nombre);
        Assert.Equal("laura@uv.mx", resultado.Correo);
        Assert.Equal("Coordinadora EA", resultado.Cargo);
        Assert.Equal(Constantes.CoordinadorEa, resultado.Rol);
        Assert.Equal(10, resultado.IdAreaAcademica);
        Assert.Equal("Técnica", resultado.NombreAreaAcademica);
        Assert.Equal(100, resultado.IdEntidadAcademica);
        Assert.Equal("Facultad de Arquitectura", resultado.NombreEntidadAcademica);
        Assert.Equal("Xalapa", resultado.Region);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_CoordinadorDgaa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 2,
            Rol = Constantes.CoordinadorDgaa
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 2,
            Nombre = "Pedro",
            Correo = "pedro@uv.mx",
            Cargo = "Coordinador DGAA",
            IdAreaAcademica = 20,
            IdAreaAcademicaNavigation = new AreaAcademica
            {
                IdAreaAcademica = 20,
                Nombre = "Económico-Administrativa"
            }
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorDgaa);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.IdUsuario);
        Assert.Equal("Pedro", resultado.Nombre);
        Assert.Equal("pedro@uv.mx", resultado.Correo);
        Assert.Equal("Coordinador DGAA", resultado.Cargo);
        Assert.Equal(Constantes.CoordinadorDgaa, resultado.Rol);
        Assert.Equal(20, resultado.IdAreaAcademica);
        Assert.Equal("Económico-Administrativa", resultado.NombreAreaAcademica);
        Assert.Null(resultado.IdEntidadAcademica);
        Assert.Null(resultado.NombreEntidadAcademica);
        Assert.Null(resultado.Region);

        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_NoExisteCoordinadorEa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 3,
            Rol = Constantes.CoordinadorEa
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync((CoordinadorEa?)null);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.Null(resultado);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_NoExisteCoordinadorDgaa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 4,
            Rol = Constantes.CoordinadorDgaa
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync((CoordinadorDgaa?)null);

        var resultado = await _usuarioService.ObtenerPorIdAsync(dto);

        Assert.Null(resultado);

        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
    }

    //Editar
    [Fact]
    public async Task EditarAsync_CoordinadorEa()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 1,
            Rol = Constantes.CoordinadorEa,
            Nombre = "Nombre Editado",
            Cargo = "Cargo Editado",
            IdEntidadAcademica = 200
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 1,
            Nombre = "Nombre Original",
            Correo = "correo@uv.mx",
            Cargo = "Cargo Original",
            IdEntidadAcademica = 100
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorEa);

        await _usuarioService.EditarAsync(dto);

        Assert.Equal("Nombre Editado", coordinadorEa.Nombre);
        Assert.Equal("Cargo Editado", coordinadorEa.Cargo);
        Assert.Equal("correo@uv.mx", coordinadorEa.Correo);
        Assert.Equal(200, coordinadorEa.IdEntidadAcademica);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorEaRepositoryMock.Verify(r => r.ActualizarAsync(coordinadorEa), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<CoordinadorDgaa>()), Times.Never);
    }

    [Fact]
    public async Task EditarAsync_CoordinadorDgaa()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 2,
            Rol = Constantes.CoordinadorDgaa,
            Nombre = "Nombre Editado",
            Cargo = "Cargo Editado",
            IdAreaAcademica = 300
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 2,
            Nombre = "Nombre Original",
            Correo = "correo@uv.mx",
            Cargo = "Cargo Original",
            IdAreaAcademica = 150
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorDgaa);

        await _usuarioService.EditarAsync(dto);

        Assert.Equal("Nombre Editado", coordinadorDgaa.Nombre);
        Assert.Equal("Cargo Editado", coordinadorDgaa.Cargo);
        Assert.Equal("correo@uv.mx", coordinadorDgaa.Correo);
        Assert.Equal(300, coordinadorDgaa.IdAreaAcademica);

        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ActualizarAsync(coordinadorDgaa), Times.Once);
        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
        _coordinadorEaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<CoordinadorEa>()), Times.Never);
    }

    //Eliminar
    [Fact]
    public async Task EliminarAsync_CoordinadorEa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 3,
            Rol = Constantes.CoordinadorEa
        };

        var coordinadorEa = new CoordinadorEa
        {
            IdCoordinadorEa = 3,
            Nombre = "Andrea",
            Correo = "andrea@uv.mx",
            Cargo = "Coordinadora EA",
            IdEntidadAcademica = 100
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorEa);

        await _usuarioService.EliminarAsync(dto);

        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorEaRepositoryMock.Verify(r => r.EliminarAsync(coordinadorEa), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
        _coordinadorDgaaRepositoryMock.Verify(r => r.EliminarAsync(It.IsAny<CoordinadorDgaa>()), Times.Never);
    }

    [Fact]
    public async Task EliminarAsync_CoordinadorDgaa()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 4,
            Rol = Constantes.CoordinadorDgaa
        };

        var coordinadorDgaa = new CoordinadorDgaa
        {
            IdCoordinadorDgaa = 4,
            Nombre = "Roberto",
            Correo = "roberto@uv.mx",
            Cargo = "Coordinador DGAA",
            IdAreaAcademica = 200
        };

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(coordinadorDgaa);

        await _usuarioService.EliminarAsync(dto);

        _coordinadorDgaaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(dto.IdUsuario), Times.Once);
        _coordinadorDgaaRepositoryMock.Verify(r => r.EliminarAsync(coordinadorDgaa), Times.Once);
        _coordinadorEaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
        _coordinadorEaRepositoryMock.Verify(r => r.EliminarAsync(It.IsAny<CoordinadorEa>()), Times.Never);
    }
}