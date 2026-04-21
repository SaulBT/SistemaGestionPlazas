using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;

public class EntidadAcademicaServiceTests
{
    private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
    private readonly Mock<IEntidadAcademicaValidator> _entidadAcademicaValidatorMock;
    private readonly EntidadAcademicaService _entidadAcademicaService;

    public EntidadAcademicaServiceTests()
    {
        _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
        _entidadAcademicaValidatorMock = new Mock<IEntidadAcademicaValidator>();
        _entidadAcademicaService = new EntidadAcademicaService(
            _entidadAcademicaRepositoryMock.Object,
            _entidadAcademicaValidatorMock.Object);
    }

    //CP-48
    [Fact]
    public async Task CrearEntidadAcademica()
    {
        var dto = CrearEntidadAcademicaDTOValido();
        EntidadAcademica? entidadCreada = null;

        _entidadAcademicaValidatorMock
            .Setup(validator => validator.ValidarCreacionAsync(dto))
            .Returns(Task.CompletedTask);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.CrearAsync(It.IsAny<EntidadAcademica>()))
            .Callback<EntidadAcademica>(entidad => entidadCreada = entidad)
            .ReturnsAsync(new EntidadAcademica
            {
                IdEntidadAcademica = 7
            });

        var idEntidadAcademica = await _entidadAcademicaService.CrearAsync(dto);

        Assert.Equal(7, idEntidadAcademica);
        Assert.NotNull(entidadCreada);
        Assert.Equal(dto.IdAreaAcademica, entidadCreada!.IdAreaAcademica);
        Assert.Equal($"{dto.Clave}-{dto.Nombre}", entidadCreada.Nombre);
        Assert.Equal(dto.CalleNumero, entidadCreada.CalleNumero);
        Assert.Equal(dto.Colonia, entidadCreada.Colonia);
        Assert.Equal(dto.Cp, entidadCreada.Cp);
        Assert.Equal(dto.Municipio, entidadCreada.Municipio);
        Assert.Equal(dto.Telefono, entidadCreada.Telefono);
        Assert.Equal(dto.Conmutador, entidadCreada.Conmutador);
        Assert.Equal(dto.Extension, entidadCreada.Extension);
        Assert.Equal(dto.Fax, entidadCreada.Fax);
        Assert.Equal(dto.Region, entidadCreada.Region);

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarCreacionAsync(dto), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.CrearAsync(It.IsAny<EntidadAcademica>()), Times.Once);
    }

    //CP-54
    [Fact]
    public async Task ObtenerListaDeEntidadesAcademicas()
    {
        var entidadesAcademicas = new List<EntidadAcademica>
        {
            new EntidadAcademica
            {
                IdEntidadAcademica = 12,
                IdAreaAcademica = 1,
                Nombre = "11304-Facultad de Estadística e Informática",
                CalleNumero = "Av. Xalapa Esq. Manuel Ávila Camacho S/N",
                Colonia = "Obrero Campesina",
                Cp = "91020",
                Municipio = "Xalapa",
                Telefono = "(228) 815-03-74",
                Conmutador = "(228) 842-17-00",
                Extension = "14155",
                Fax = "(228) 814-99-90",
                Region = "1-Xalapa",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Económico-Administrativa"
                }
            },
            new EntidadAcademica
            {
                IdEntidadAcademica = 97,
                IdAreaAcademica = 2,
                Nombre = "21052-Facultad de Ingeniería Mecánica y Ciencias Navales",
                CalleNumero = "Bv. Adolfo Ruíz Cortines 455",
                Colonia = "Costa Verde",
                Cp = "94294",
                Municipio = "Veracruz",
                Telefono = "(229) 775-20-00",
                Conmutador = "(229) 742-17-00",
                Extension = "25123",
                Fax = "(229) 714-99-90",
                Region = "2-Veracruz",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Técnica"
                }
            }
        };

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerDiezAsync(1))
            .ReturnsAsync(entidadesAcademicas);

        var resultado = await _entidadAcademicaService.ObtenerListaAsync(1);

        Assert.Equal(2, resultado.Count);

        Assert.Collection(resultado,
            primerDto =>
            {
                Assert.Equal(12, primerDto.IdEntidadAcademica);
                Assert.Equal(1, primerDto.IdAreaAcademica);
                Assert.Equal("11304-Facultad de Estadística e Informática", primerDto.Nombre);
                Assert.Contains("Av. Xalapa Esq. Manuel Ávila Camacho S/N", primerDto.Domicilio);
                Assert.Contains("Obrero Campesina", primerDto.Domicilio);
                Assert.Contains("91020", primerDto.Domicilio);
                Assert.Contains("Xalapa", primerDto.Domicilio);
                Assert.Contains("(228) 815-03-74", NormalizarTexto(primerDto.Telefono));
                Assert.Contains("(228) 842-17-00", NormalizarTexto(primerDto.Telefono));
                Assert.Contains("14155", NormalizarTexto(primerDto.Telefono));
                Assert.Contains("(228) 814-99-90", NormalizarTexto(primerDto.Telefono));
                Assert.Equal("Económico-Administrativa", primerDto.NombreEntidadAcademica);
                Assert.Equal("1-Xalapa", primerDto.Region);
            },
            segundoDto =>
            {
                Assert.Equal(97, segundoDto.IdEntidadAcademica);
                Assert.Equal(2, segundoDto.IdAreaAcademica);
                Assert.Equal("21052-Facultad de Ingeniería Mecánica y Ciencias Navales", segundoDto.Nombre);
                Assert.Contains("Bv. Adolfo Ruíz Cortines 455", segundoDto.Domicilio);
                Assert.Contains("Costa Verde", segundoDto.Domicilio);
                Assert.Contains("94294", segundoDto.Domicilio);
                Assert.Contains("Veracruz", segundoDto.Domicilio);
                Assert.Contains("(229) 775-20-00", NormalizarTexto(segundoDto.Telefono));
                Assert.Contains("(229) 742-17-00", NormalizarTexto(segundoDto.Telefono));
                Assert.Contains("25123", NormalizarTexto(segundoDto.Telefono));
                Assert.Contains("(229) 714-99-90", NormalizarTexto(segundoDto.Telefono));
                Assert.Equal("Técnica", segundoDto.NombreEntidadAcademica);
                Assert.Equal("2-Veracruz", segundoDto.Region);
            });

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarIndice(1), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.ObtenerDiezAsync(1), Times.Once);
    }

    //CP-55
    [Fact]
    public async Task ObtenerListaDeEntidadesAcademicasConFiltro()
    {
        var filtro = new FiltroEntidadAcademicaDTO
        {
            Region = "1-Xalapa",
            IdAreaAcademica = 1,
            Nombre = null
        };

        var entidadesAcademicas = new List<EntidadAcademica>
        {
            new EntidadAcademica
            {
                IdEntidadAcademica = 12,
                IdAreaAcademica = 1,
                Nombre = "11304-Facultad de Estadística e Informática",
                CalleNumero = "Av. Xalapa Esq. Manuel Ávila Camacho S/N",
                Colonia = "Obrero Campesina",
                Cp = "91020",
                Municipio = "Xalapa",
                Telefono = "(228) 815-03-74",
                Conmutador = "(228) 842-17-00",
                Extension = "14155",
                Fax = "(228) 814-99-90",
                Region = "1-Xalapa",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Económico-Administrativa"
                }
            }
        };

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre))
            .ReturnsAsync(entidadesAcademicas);

        var resultado = await _entidadAcademicaService.ObtenerPorFiltroAsync(filtro, 1);

        var dto = Assert.Single(resultado);
        Assert.Equal(12, dto.IdEntidadAcademica);
        Assert.Equal(1, dto.IdAreaAcademica);
        Assert.Equal("11304-Facultad de Estadística e Informática", dto.Nombre);
        Assert.Contains("Av. Xalapa Esq. Manuel Ávila Camacho S/N", dto.Domicilio);
        Assert.Contains("Obrero Campesina", dto.Domicilio);
        Assert.Contains("91020", dto.Domicilio);
        Assert.Contains("Xalapa", dto.Domicilio);
        Assert.Contains("(228) 815-03-74", NormalizarTexto(dto.Telefono));
        Assert.Contains("(228) 842-17-00", NormalizarTexto(dto.Telefono));
        Assert.Contains("14155", NormalizarTexto(dto.Telefono));
        Assert.Contains("(228) 814-99-90", NormalizarTexto(dto.Telefono));
        Assert.Equal("Económico-Administrativa", dto.NombreEntidadAcademica);
        Assert.Equal("1-Xalapa", dto.Region);

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarIndice(1), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.ObtenerPorFiltroAsync(filtro.Region, filtro.IdAreaAcademica, filtro.Nombre), Times.Once);
    }

    //CP-56
    [Fact]
    public async Task ObtenerEntidadAcademica()
    {
        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 12,
            IdAreaAcademica = 1,
            Nombre = "11304-Facultad de Estadística e Informática",
            CalleNumero = "Av. Xalapa Esq. Manuel Ávila Camacho S/N",
            Colonia = "Obrero Campesina",
            Cp = "91020",
            Municipio = "Xalapa",
            Telefono = "(228) 815-03-74",
            Conmutador = "(228) 842-17-00",
            Extension = "14155",
            Fax = "(228) 814-99-90",
            Region = "1-Xalapa",
            IdAreaAcademicaNavigation = new AreaAcademica
            {
                Nombre = "Económico-Administrativa"
            }
        };

        _entidadAcademicaValidatorMock
            .Setup(validator => validator.ValidarIdAsync(12))
            .Returns(Task.CompletedTask);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(12))
            .ReturnsAsync(entidadAcademica);

        var dto = await _entidadAcademicaService.ObtenerPorIdAsync(12);

        Assert.Equal(12, dto.IdEntidadAcademica);
        Assert.Equal(1, dto.IdAreaAcademica);
        Assert.Equal("11304", dto.Clave);
        Assert.Equal("Facultad de Estadística e Informática", dto.Nombre);
        Assert.Equal("Av. Xalapa Esq. Manuel Ávila Camacho S/N", dto.CalleNumero);
        Assert.Equal("Obrero Campesina", dto.Colonia);
        Assert.Equal("91020", dto.Cp);
        Assert.Equal("Xalapa", dto.Municipio);
        Assert.Equal("(228) 815-03-74", dto.Telefono);
        Assert.Equal("(228) 842-17-00", dto.Conmutador);
        Assert.Equal("14155", dto.Extension);
        Assert.Equal("(228) 814-99-90", dto.Fax);
        Assert.Equal("1-Xalapa", dto.Region);
        Assert.Equal("Económico-Administrativa", dto.NombreEntidadAcademica);

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarIdAsync(12), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.ObtenerPorIdAsync(12), Times.Once);
    }

    //CP-58
    [Fact]
    public async Task EditarEntidadAcademica()
    {
        var dto = DatosEntidadAcademicaDTOValido();
        EntidadAcademica? entidadActualizada = null;

        _entidadAcademicaValidatorMock
            .Setup(validator => validator.ValidarEdicionAsync(dto))
            .Returns(Task.CompletedTask);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ActualizarAsync(It.IsAny<EntidadAcademica>()))
            .Callback<EntidadAcademica>(entidad => entidadActualizada = entidad)
            .Returns(Task.CompletedTask);

        await _entidadAcademicaService.EditarAsync(dto);

        Assert.NotNull(entidadActualizada);
        Assert.Equal(dto.IdEntidadAcademica, entidadActualizada!.IdEntidadAcademica);
        Assert.Equal(dto.IdAreaAcademica, entidadActualizada.IdAreaAcademica);
        Assert.Equal($"{dto.Clave}-{dto.Nombre}", entidadActualizada.Nombre);
        Assert.Equal(dto.CalleNumero, entidadActualizada.CalleNumero);
        Assert.Equal(dto.Colonia, entidadActualizada.Colonia);
        Assert.Equal(dto.Cp, entidadActualizada.Cp);
        Assert.Equal(dto.Municipio, entidadActualizada.Municipio);
        Assert.Equal(dto.Telefono, entidadActualizada.Telefono);
        Assert.Equal(dto.Conmutador, entidadActualizada.Conmutador);
        Assert.Equal(dto.Extension, entidadActualizada.Extension);
        Assert.Equal(dto.Fax, entidadActualizada.Fax);
        Assert.Equal(dto.Region, entidadActualizada.Region);

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarEdicionAsync(dto), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.ActualizarAsync(It.IsAny<EntidadAcademica>()), Times.Once);
    }

    //CP-65
    [Fact]
    public async Task EliminarEntidadAcademica()
    {
        var entidadAcademica = new EntidadAcademica
        {
            IdEntidadAcademica = 12,
            IdAreaAcademica = 1,
            Nombre = "11304-Facultad de Estadística e Informática"
        };

        _entidadAcademicaValidatorMock
            .Setup(validator => validator.ValidarIdAsync(12))
            .Returns(Task.CompletedTask);

        _entidadAcademicaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(12))
            .ReturnsAsync(entidadAcademica);

        await _entidadAcademicaService.EliminarAsync(12);

        _entidadAcademicaValidatorMock.Verify(validator => validator.ValidarIdAsync(12), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.ObtenerPorIdAsync(12), Times.Once);
        _entidadAcademicaRepositoryMock.Verify(repository => repository.EliminarAsync(entidadAcademica), Times.Once);
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

    private static string NormalizarTexto(string texto)
    {
        return texto.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}
