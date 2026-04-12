using Moq;
using SGPla.Repositories.Interfaces;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Commons;
using SGPla.Validations.Implementation;
using SGPla.Models;

public class UsuarioValidatorTests
{
    private readonly Mock<ICoordinadorEaRepository> _coordinadorEaRepositoryMock;
    private readonly Mock<ICoordinadorDgaaRepository> _coordinadorDgaaRepositoryMock;
    private readonly Mock<IAreaAcademicaRepository> _areaAcademicaRepositoryMock;
    private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
    private readonly UsuarioValidator _usuarioValidator;

    public UsuarioValidatorTests()
    {
        _coordinadorEaRepositoryMock = new Mock<ICoordinadorEaRepository>();
        _coordinadorDgaaRepositoryMock = new Mock<ICoordinadorDgaaRepository>();
        _areaAcademicaRepositoryMock = new Mock<IAreaAcademicaRepository>();
        _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();

        _usuarioValidator = new UsuarioValidator(
            _coordinadorEaRepositoryMock.Object,
            _coordinadorDgaaRepositoryMock.Object,
            _areaAcademicaRepositoryMock.Object,
            _entidadAcademicaRepositoryMock.Object);
    }

    //CP-03
    [Fact]
    public async Task CrearCoordinadorEaConValoresNulos()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = null,
            Correo = null,
            Cargo = null,
            Rol = null,
            IdAreaAcademica = 0
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El nombre es obligatorio.", ex.Message);
    }

    //CP-04
    [Fact]
    public async Task CrearCoordinadorEaConCorreoInvalido()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "María López",
            Correo = "maria.com",
            Cargo = "Coordinadora",
            Rol = Constantes.CoordinadorEa,
            IdEntidadAcademica = 20
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El formato del correo no es válido.", ex.Message);
    }

    //CP-05
    [Fact]
    public async Task CrearCoordinadorEaCorreoRepetido()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Enrique Gonzáles",
            Correo = "enrique@uv.mx",
            Cargo = "Coordinador de Economía",
            Rol = Constantes.CoordinadorEa,
            IdEntidadAcademica = 3
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(true);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El correo ya está en uso.", ex.Message);
    }

    //CP-06
    [Fact]
    public async Task CrearUsuarioConRolInvalido()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Santiago Abascal",
            Correo = "santiago@uv.com",
            Cargo = "Coordinador",
            Rol = "Coordinador General",
            IdAreaAcademica = 64
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no es válido.", ex.Message);
    }

    //CP-07
    [Fact]
    public async Task CrearCoordinadorDgaaConIdEntiadAcademica()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "María López",
            Correo = "maria@uv.com",
            Cargo = "Coordinadora",
            Rol = Constantes.CoordinadorDgaa,
            IdEntidadAcademica =31
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no corresponde con la dependencia seleccionada.", ex.Message);
    }

    //CP-08
    [Fact]
    public async Task CrearCoordinadorEaConIdAreaAcademica()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Entique Guzmán",
            Correo = "enrique@uv.com",
            Cargo = "Coordinador",
            Rol = Constantes.CoordinadorEa,
            IdAreaAcademica = 7
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no corresponde con la dependencia seleccionada.", ex.Message);
    }

    //CP-09
    [Fact]
    public async Task CrearCoordinadorEaConEntidadAcademicaInexistente()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Saúl Barragán",
            Correo = "saul@uv.mx",
            Cargo = "Coordinador",
            Rol = Constantes.CoordinadorEa,
            IdEntidadAcademica = 12
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(false);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(false);

        _entidadAcademicaRepositoryMock
            .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica.Value))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("No existe esa Entidad Académica.", ex.Message);
    }

    //CP-10
    [Fact]
    public async Task CrearCoordinadorDgaaConAreaAcademicaInexistente()
    {
        var dto = new CrearUsuarioDTO
        {
            Nombre = "Evelyn López",
            Correo = "evelyn@uv.mx",
            Cargo = "Coordinadora",
            Rol = Constantes.CoordinadorDgaa,
            IdAreaAcademica = 66
        };

        _coordinadorEaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(false);

        _coordinadorDgaaRepositoryMock
            .Setup(r => r.ExisteCorreoAsync(dto.Correo))
            .ReturnsAsync(false);

        _areaAcademicaRepositoryMock
            .Setup(r => r.ExistePorIdAsync(dto.IdAreaAcademica.Value))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarCreacionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("No existe esa Área Académica.", ex.Message);
    }

    //CP-17
    [Fact]
    public async Task ObtenerUsuarioConValoresNulos()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 0,
            Rol = null
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El IdUsuario es obligatorio.", ex.Message);
    }

    //CP-18
    [Fact]
    public async Task ObtenerUsuarioConRolInvalido()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 101,
            Rol = "Jefe de Carrera"
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no es válido.", ex.Message);
    }

    //CP-19
    [Fact]
    public async Task ObtenerUsuarioInexistente()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 6767,
            Rol = Constantes.CoordinadorDgaa
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync((CoordinadorDgaa?)null);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El Usuario no existe.", ex.Message);
    }

    //CP-22
    [Fact]
    public async Task EditarCoordinadorEaConValoresNulos()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 0,
            Rol = null,
            Nombre = null,
            Cargo = null,
            IdEntidadAcademica = 0
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("La IdUsuario es obligatoria.", ex.Message);
    }

    //CP-23
    [Fact]
    public async Task EditarUsuarioConRolInvalido()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 534,
            Rol = "Compositor",
            Nombre = "Miguel Ángel Bocanegra",
            Cargo = "Coordinador",
            IdEntidadAcademica = 200
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no es válido.", ex.Message);
    }

    //CP-24
    [Fact]
    public async Task EditarCoordinadorDgaaConIdEntidadAcademica()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 453,
            Rol = Constantes.CoordinadorDgaa,
            Nombre = "Luisa Londóñes",
            Cargo = "Administradora Jefe",
            IdEntidadAcademica = 2
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(new CoordinadorDgaa { IdCoordinadorDgaa = 453, IdAreaAcademica = 2, Nombre = "Luisa Londoño", Correo = "luisa@uv.mx", Cargo = "Administradora Jefe" });

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no corresponde con la dependencia editada.", ex.Message);
    }

    //CP-25
    [Fact]
    public async Task EditarCoordinadorEaConIdAreaAcademica()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 534,
            Rol = Constantes.CoordinadorEa,
            Nombre = "Miguel Ángel Bocanegra",
            Cargo = "Coordinador",
            IdAreaAcademica = 200
        };

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(new CoordinadorEa { IdCoordinadorEa = 534, IdEntidadAcademica = 200, Nombre = "Ángel Bocanegra", Correo = "angel@uv.mx", Cargo = "Jefe de Unidad" });

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no corresponde con la dependencia editada.", ex.Message);
    }

    //CP-26
    [Fact]
    public async Task EditarCoordinadorEaConEntidadAcademicaInexistente()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 534,
            Rol = Constantes.CoordinadorEa,
            Nombre = "Miguel Ángel Bocanegra",
            Cargo = "Coordinador",
            IdEntidadAcademica = 76
        };

        _coordinadorEaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(new CoordinadorEa { IdCoordinadorEa = 534, IdEntidadAcademica = 200, Nombre = "Ángel Bocanegra", Correo = "angel@uv.mx", Cargo = "Jefe de Unidad" });

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(76))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("No existe esa Entidad Académica.", ex.Message);
    }

    //CP-27
    [Fact]
    public async Task EditarCoordinadorDgaaConAreaAcademicaInexistente()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 453,
            Rol = Constantes.CoordinadorDgaa,
            Nombre = "Luisa Londóñes",
            Cargo = "Administradora Jefe",
            IdAreaAcademica = 10
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync(new CoordinadorDgaa { IdCoordinadorDgaa = 453, IdAreaAcademica = 2, Nombre = "Luisa Londoño", Correo = "luisa@uv.mx", Cargo = "Administradora Jefe" });

        _areaAcademicaRepositoryMock
            .Setup(repository => repository.ExistePorIdAsync(10))
            .ReturnsAsync(false);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("No existe esa Área Académica.", ex.Message);
    }

    //CP-28
    [Fact]
    public async Task EditarUsuarioInexistente()
    {
        var dto = new EditarUsuarioDTO
        {
            IdUsuario = 912,
            Rol = Constantes.CoordinadorDgaa,
            Nombre = "Luisa Londóñes",
            Cargo = "Administradora Jefe",
            IdAreaAcademica = 10
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync((CoordinadorDgaa?)null);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarEdicionAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("No existe ese usuario.", ex.Message);
    }

    //CP-31
    [Fact]
    public async Task EliminarUsuarioConValoresNulos()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 0,
            Rol = null
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El IdUsuario es obligatorio.", ex.Message);
    }

    //CP-32
    [Fact]
    public async Task EliminarUsuarioConRolInvalido()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 101,
            Rol = "Jefe de Carrera"
        };

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El rol no es válido.", ex.Message);
    }

    //CP-33
    [Fact]
    public async Task EliminarUsuarioInexistente()
    {
        var dto = new ReferenciaUsuarioDTO
        {
            IdUsuario = 6767,
            Rol = Constantes.CoordinadorDgaa
        };

        _coordinadorDgaaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(dto.IdUsuario))
            .ReturnsAsync((CoordinadorDgaa?)null);

        var ex = await Record.ExceptionAsync(() => _usuarioValidator.ValidarReferenciaAsync(dto));

        Assert.NotNull(ex);
        Assert.Equal("El Usuario no existe.", ex.Message);
    }
}