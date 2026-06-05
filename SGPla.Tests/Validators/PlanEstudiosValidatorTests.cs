using Microsoft.Extensions.Logging;
using Moq;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

namespace SGPlaTests.Validations;

public class PlanEstudiosValidatorTests
{
    private readonly Mock<IPlanEstudiosRepository> _planEstudiosRepositoryMock;
    private readonly Mock<IExperienciaEducativaRepository> _experienciaEducativaRepositoryMock;
    private readonly PlanEstudiosValidator _validator;

    public PlanEstudiosValidatorTests()
    {
        _planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        _experienciaEducativaRepositoryMock = new Mock<IExperienciaEducativaRepository>();
        _validator = new PlanEstudiosValidator(
            _planEstudiosRepositoryMock.Object,
            _experienciaEducativaRepositoryMock.Object,
            Mock.Of<ILogger<PlanEstudiosValidator>>());
    }

    // CP-83
    [Fact]
    public void ProcesarArchivoConDatosNulos()
    {
        var dto = new ArchivoPlanEstudiosDTO
        {
            Ruta = null!,
            NombreArchivo = null!
        };

        var exception = Assert.Throws<ValidacionExcepction>(() => _validator.ValidarArchivo(dto));

        Assert.Equal("El Archivo es obligatorio.", exception.Message);
    }

    // CP-84
    [Fact]
    public void ProcesarArchivoConFormatoInvalido()
    {
        var dto = new ArchivoPlanEstudiosDTO
        {
            Ruta = "planes-estudios/plan-lisoft.pdf",
            NombreArchivo = "PlanLisoft.pdf"
        };

        var exception = Assert.Throws<ValidacionExcepction>(() => _validator.ValidarArchivo(dto));

        Assert.Equal("El formato del archivo no es soportado.", exception.Message);
    }

    // CP-86
    [Fact]
    public async Task AgregarPlanDeEstudiosConIdProgramaEducativoInvalido()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.IdProgramaEducativo = -78;

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("El IdProgramaEducativo es inválido.", exception.Message);
    }

    // CP-87
    [Fact]
    public async Task AgregarPlanDeEstudiosConProgramaEducativoInexistente()
    {
        var dto = CrearPlanEstudiosDtoValido();

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("No existe ese Programa Educativo.", exception.Message);
    }

    // CP-88
    [Fact]
    public async Task AgregarPlanDeEstudiosSinExperienciasEducativas()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.ExperienciasEducativas = new List<AgregarExperienciaEducativaDTO>();

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("No se puede crear un Plan de Estudios sin Experiencias Educativas.", exception.Message);
    }

    // CP-89
    [Fact]
    public async Task AgregarPlanDeEstudiosConExperienciaEducativaRepetidaEnElPlan()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.ExperienciasEducativas[2].Codigo = "FBGR 80012";

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("Hay Experiencias Educativas con código repetido.", exception.Message);
    }

    // CP-90
    [Fact]
    public async Task AgregarPlanDeEstudiosConExperienciaEducativaRepetidaEnElSistema()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.ExperienciasEducativas[0].Codigo = "FBGR 80001";

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteCodigoExperienciaEducativaEnSistemaAsync("FBGR 80001"))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("Ya hay una Experiencia Educativa con el Código FBGR 80001 en el sistema.", exception.Message);
    }

    // CP-91
    [Fact]
    public async Task AgregarPlanDeEstudiosConValoresDeExperienciaEducativaNulos()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.ExperienciasEducativas = new List<AgregarExperienciaEducativaDTO>
        {
            new AgregarExperienciaEducativaDTO
            {
                Codigo = null!,
                Nombre = null!,
                PerfilDocente = null!
            }
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("El Código es obligatorio en todas las Experiencias Educativas nuevas.", exception.Message);
    }

    // CP-92
    [Fact]
    public async Task AgregarPlanDeEstudiosConCodigoDeUnaExperienciaEducativaInvalido()
    {
        var dto = CrearPlanEstudiosDtoValido();
        dto.ExperienciasEducativas = new List<AgregarExperienciaEducativaDTO>
        {
            new AgregarExperienciaEducativaDTO
            {
                Codigo = "CODE-01",
                Nombre = "Pruebas de Penetración",
                PerfilDocente = "Licenciado en Informática o carrera a fin...",
                Horas = "5",
                Creditos = "8"
            }
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExisteProgramaEducativoPorIdAsync(dto.IdProgramaEducativo))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _validator.ValidarCreacionAsync(dto));

        Assert.Equal("El Código de una Experiencia Educativa es inválido.", exception.Message);
    }

    // CP-96
    [Fact]
    public async Task ObtenerPlanDeEstudiosConIdInvalida()
    {
        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarIdAsync(-100));

        Assert.Equal("La IdPlanEstudios es inválida.", exception.Message);
    }

    // CP-97
    [Fact]
    public async Task ObtenerPlanDeEstudiosInexistente()
    {
        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(94))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarIdAsync(94));

        Assert.Equal("No existe ese Plan de Estudios.", exception.Message);
    }

    // CP-99
    [Fact]
    public async Task EditarPlanDeEstudiosConIdPlanEstudiosInvalida()
    {
        var dto = EditarPlanEstudiosDtoValido();
        dto.IdPlanEstudios = -10;

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("La IdPlanEstudios es inválida.", exception.Message);
    }

    // CP-100
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEliminarConIdInvalida()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int> { -12 }
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("La Id de una Experiencia Educativa para eliminar es inválida.", exception.Message);
    }

    // CP-101
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEliminarInexistente()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int> { 187 }
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteExperienciaEducativaPorIdAsync(187))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("No existe la Experiencia Educativa con Id 187 para eliminar.", exception.Message);
    }

    // CP-102
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarConCodigoDuplicado()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = "FBGR 80014",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteExperienciaEducativaPorIdAsync(93))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExperienciaEducativaPerteneceAPlanAsync(93, 67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(67, "FBGR 80014"))
            .ReturnsAsync(false);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67))
            .ReturnsAsync(new List<ExperienciaEducativa>
            {
                new ExperienciaEducativa
                {
                    IdExperienciaEducativa = 93,
                    IdPlanEstudios = 67,
                    Codigo = "FBGR 80001",
                    Nombre = "Computación Básica",
                    PerfilDocente = "Licenciado en Informática o carrera a fin..."
                },
                new ExperienciaEducativa
                {
                    IdExperienciaEducativa = 187,
                    IdPlanEstudios = 67,
                    Codigo = "FBGR 80014",
                    Nombre = "Procesos de Software",
                    PerfilDocente = "Licenciado en Informática o carrera a fin..."
                }
            });

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("Ya hay una Experiencia Educativa con el Código FBGR 80014 en el Plan de Estudios.", exception.Message);
    }

    // CP-103
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarConCodigoDuplicadoEnOtroPlanDeEstudios()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = "FBGR 80014",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteExperienciaEducativaPorIdAsync(93))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExperienciaEducativaPerteneceAPlanAsync(93, 67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(67, "FBGR 80014"))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("Ya hay una Experiencia Educativa con el Código FBGR 80014 en el sistema.", exception.Message);
    }

    // CP-104
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarConIdInvalida()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 0,
                    Codigo = "FBGR 80002",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin..."
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("La Id de una Experiencia Educativa a editar es inválida.", exception.Message);
    }

    // CP-105
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarInexistente()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = "FBGR 80002",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteExperienciaEducativaPorIdAsync(93))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("No existe ninguna Experiencia Educativa a editar con Id 93.", exception.Message);
    }

    // CP-106
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarConValoresNulos()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = null!,
                    Nombre = null!,
                    PerfilDocente = null!
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("El Código es obligatorio en todas las Experiencias Educativas a editar.", exception.Message);
    }

    // CP-107
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaAEditarConCodigoInvalido()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>(),
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = "192.128",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("El Código de una Experiencia Educativa a editar es inválido.", exception.Message);
    }

    // CP-108
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaNuevaConCodigoDuplicado()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80001",
                    Nombre = "Estructuras de Datos",
                    PerfilDocente = "Licenciado en informática o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(67, "FBGR 80001"))
            .ReturnsAsync(false);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67))
            .ReturnsAsync(new List<ExperienciaEducativa>
            {
                new ExperienciaEducativa
                {
                    IdExperienciaEducativa = 93,
                    IdPlanEstudios = 67,
                    Codigo = "FBGR 80001",
                    Nombre = "Computación Básica",
                    PerfilDocente = "Licenciado en Informática o carrera a fin..."
                }
            });

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("Ya hay una Experiencia Educativa con el Código FBGR 80001 en el Plan de Estudios.", exception.Message);
    }

    // CP-109
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaNuevaConCodigoDuplicadoEnOtroPlanDeEstudios()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80014",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        _experienciaEducativaRepositoryMock
            .Setup(r => r.ExisteCodigoExperienciaEducativaEnOtroPlanAsync(67, "FBGR 80014"))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("Ya hay una Experiencia Educativa con el Código FBGR 80014 en el sistema.", exception.Message);
    }

    // CP-110
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaNuevaConValoresNulos()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = null!,
                    Nombre = null!,
                    PerfilDocente = null!
                }
            },
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("El Código es obligatorio en todas las Experiencias Educativas nuevas.", exception.Message);
    }

    // CP-111
    [Fact]
    public async Task EditarPlanDeEstudiosConExperienciaEducativaNuevaConCodigoInvalido()
    {
        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "10",
                    Nombre = "Estructuras de Datos",
                    PerfilDocente = "Licenciado en informática o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>(),
            IdsExperienciasEliminadas = new List<int>()
        };

        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(67))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _validator.ValidarEdicionAsync(dto));

        Assert.Equal("El Código de una Experiencia Educativa nueva es inválido.", exception.Message);
    }

    // CP-113
    [Fact]
    public async Task EliminarPlanDeEstudiosConIdInvalida()
    {
        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarIdAsync(-1));

        Assert.Equal("La IdPlanEstudios es inválida.", exception.Message);
    }

    // CP-114
    [Fact]
    public async Task EliminarPlanDeEstudiosInexistente()
    {
        _planEstudiosRepositoryMock
            .Setup(r => r.ExistePorIdAsync(17))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ValidacionExcepction>(() => _validator.ValidarIdAsync(17));

        Assert.Equal("No existe ese Plan de Estudios.", exception.Message);
    }

    private static CrearPlanEstudiosDTO CrearPlanEstudiosDtoValido()
    {
        return new CrearPlanEstudiosDTO
        {
            IdProgramaEducativo = 12,
            Nombre = "Plan 2014",
            Sistema = "Escolarizado",
            ExperienciasEducativas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80012",
                    Nombre = "Pruebas de Penetración",
                    PerfilDocente = "Licenciado en Informática o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                },
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80007",
                    Nombre = "Programación I",
                    PerfilDocente = "Licenciado en Informática o carrera a fin...",
                    Horas = "5",
                    Creditos = "6"
                },
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80020",
                    Nombre = "Tecnologías para la Construcción de Software",
                    PerfilDocente = "Licenciado en Informática o carrera a fin...",
                    Horas = "3",
                    Creditos = "6"
                }
            }
        };
    }

    private static EditarPlanEstudiosDTO EditarPlanEstudiosDtoValido()
    {
        return new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            ExperienciasNuevas = new List<AgregarExperienciaEducativaDTO>
            {
                new AgregarExperienciaEducativaDTO
                {
                    Codigo = "FBGR 80067",
                    Nombre = "Estructuras de Datos",
                    PerfilDocente = "Licenciado en informática o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            ExperienciasEditadas = new List<DatosExperienciaEducativaDTO>
            {
                new DatosExperienciaEducativaDTO
                {
                    IdExperienciaEducativa = 93,
                    Codigo = "FBGR 80002",
                    Nombre = "Computación Básica I",
                    PerfilDocente = "Licenciado en Redes o carrera a fin...",
                    Horas = "5",
                    Creditos = "8"
                }
            },
            IdsExperienciasEliminadas = new List<int> { 187 }
        };
    }
}
