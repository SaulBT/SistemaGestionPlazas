using System.Text;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;

namespace SGPlaTests.Services;

public class PlanEstudiosServiceTests
{
    static PlanEstudiosServiceTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    //CP-06-01
    [Fact]
    public void ProcesarArchivoDePlanDeEstudios()
    {
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();
        var service = CrearService(planEstudiosValidatorMock: planEstudiosValidatorMock.Object);

        var rutaTemporal = CrearArchivoExcelPlanEstudios();
        try
        {
            var dto = new ArchivoPlanEstudiosDTO
            {
                Ruta = rutaTemporal,
                NombreArchivo = "PlanLisoft.xls"
            };

            var resultado = service.ProcesarArchivo(dto);

            Assert.Equal(3, resultado.Count);

            Assert.Collection(resultado,
                primera =>
                {
                    Assert.Equal("LISC 80012", primera.Codigo);
                    Assert.Equal("Pruebas de Penetración", primera.Nombre);
                    Assert.Equal("Licenciado en Informática o carrera a fin...", primera.PerfilDocente);
                },
                segunda =>
                {
                    Assert.Equal("LISC 80007", segunda.Codigo);
                    Assert.Equal("Programación I", segunda.Nombre);
                    Assert.Equal("Licenciado en Informática o carrera a fin...", segunda.PerfilDocente);
                },
                tercera =>
                {
                    Assert.Equal("LISC 80020", tercera.Codigo);
                    Assert.Equal("Tecnologías para la Construcción de Software", tercera.Nombre);
                    Assert.Equal("Licenciado en Informática o carrera a fin...", tercera.PerfilDocente);
                });

            planEstudiosValidatorMock.Verify(v => v.ValidarArchivo(dto), Times.Once);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
                File.Delete(rutaTemporal);
        }
    }

    //CP-06-04
    [Fact]
    public async Task AgregarPlanDeEstudios()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        var experienciaEducativaRepositoryMock = new Mock<IExperienciaEducativaRepository>();
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();
        var archivoRepositoryMock = new Mock<IArchivoRepository>();
        var archivoServiceMock = new Mock<IArchivoService>();

        var service = CrearService(
            planEstudiosRepositoryMock.Object,
            experienciaEducativaRepositoryMock.Object,
            planEstudiosValidatorMock.Object,
            archivoRepositoryMock.Object,
            archivoServiceMock.Object);

        var dto = CrearPlanEstudiosDtoValido();
        PlanEstudios? planEnviado = null;
        List<ExperienciaEducativa>? experienciasEnviadas = null;
        Archivo? archivoEnviado = null;

        planEstudiosValidatorMock
            .Setup(v => v.ValidarCreacionAsync(dto))
            .Returns(Task.CompletedTask);

        archivoServiceMock
            .Setup(s => s.GuardarAsync(
                dto.Archivo.Ruta,
                dto.Archivo.NombreArchivo,
                "planes-estudios"))
            .ReturnsAsync(new DatosArchivoGuardadoDTO
            {
                NombreOriginal = "PlanLisoft.xls",
                Ruta = "planes-estudios/archivo-prueba.xls",
                Tipo = "application/vnd.ms-excel",
                Tamanio = 1000
            });

        archivoRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<Archivo>()))
            .Callback<Archivo>(archivo => archivoEnviado = archivo)
            .ReturnsAsync(new Archivo
            {
                IdArchivo = 15,
                Nombre = "PlanLisoft.xls",
                Ruta = "planes-estudios/archivo-prueba.xls",
                Tipo = "application/vnd.ms-excel",
                Tamanio = 1000
            });

        planEstudiosRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<PlanEstudios>()))
            .Callback<PlanEstudios>(plan => planEnviado = plan)
            .ReturnsAsync(new PlanEstudios
            {
                IdPlanEstudios = 1
            });

        experienciaEducativaRepositoryMock
            .Setup(r => r.CrearExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()))
            .Callback<List<ExperienciaEducativa>>(experiencias => experienciasEnviadas = experiencias)
            .Returns(Task.CompletedTask);

        var idResultado = await service.AgregarAsync(dto);

        Assert.Equal(1, idResultado);

        Assert.NotNull(archivoEnviado);
        Assert.Equal("PlanLisoft.xls", archivoEnviado!.Nombre);
        Assert.Equal("planes-estudios/archivo-prueba.xls", archivoEnviado.Ruta);
        Assert.Equal("application/vnd.ms-excel", archivoEnviado.Tipo);
        Assert.Equal(1000, archivoEnviado.Tamanio);

        Assert.NotNull(planEnviado);
        Assert.Equal(dto.IdProgramaEducativo, planEnviado!.IdProgramaEducativo);
        Assert.Equal(dto.Nombre, planEnviado.Nombre);
        Assert.Equal(dto.Sistema, planEnviado.Modalidad);
        Assert.Equal(15, planEnviado.IdArchivoPlan);

        Assert.NotNull(experienciasEnviadas);
        Assert.Equal(3, experienciasEnviadas!.Count);
        Assert.All(experienciasEnviadas, experiencia => Assert.Equal(1, experiencia.IdPlanEstudios));

        Assert.Collection(experienciasEnviadas,
            primera =>
            {
                Assert.Equal("FBGR 80012", primera.Codigo);
                Assert.Equal("Pruebas de Penetración", primera.Nombre);
                Assert.Equal("Licenciado en Informática o carrera a fin...", primera.PerfilDocente);
            },
            segunda =>
            {
                Assert.Equal("FBGR 80007", segunda.Codigo);
                Assert.Equal("Programación I", segunda.Nombre);
                Assert.Equal("Licenciado en Informática o carrera a fin...", segunda.PerfilDocente);
            },
            tercera =>
            {
                Assert.Equal("FBGR 80020", tercera.Codigo);
                Assert.Equal("Tecnologías para la Construcción de Software", tercera.Nombre);
                Assert.Equal("Licenciado en Informática o carrera a fin...", tercera.PerfilDocente);
            });

        planEstudiosValidatorMock.Verify(v => v.ValidarCreacionAsync(dto), Times.Once);
        archivoServiceMock.Verify(s => s.GuardarAsync(dto.Archivo.Ruta, dto.Archivo.NombreArchivo, "planes-estudios"), Times.Once);
        archivoRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<Archivo>()), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<PlanEstudios>()), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.CrearExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()), Times.Once);
    }

    //CP-06-12
    [Fact]
    public async Task ObtenerTodosLosPlanesDeEstudio()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();

        var service = CrearService(planEstudiosRepositoryMock: planEstudiosRepositoryMock.Object);

        var planes = new List<PlanEstudios>
        {
            CrearPlanEstudiosLista(67, "Licenciatura en Ingeniería en Software", "Plan 2024", "Escolarizado", "Económico-Administrativa"),
            CrearPlanEstudiosLista(23, "Licenciatura en Redes y Servicios", "Plan 2010", "Escolarizado", "Técnica")
        };

        planEstudiosRepositoryMock
            .Setup(r => r.ObtenerTodosAsync())
            .ReturnsAsync(planes);

        var resultado = await service.ObtenerTodosAsync();

        Assert.Equal(2, resultado.Count);

        Assert.Collection(resultado,
            primero =>
            {
                Assert.Equal(67, primero.IdPlanEstudios);
                Assert.Equal("Licenciatura en Ingeniería en Software", primero.NombreProgramaEducativo);
                Assert.Equal("Escolarizado", primero.Modalidad);
                Assert.Equal("Plan 2024", primero.Nombre);
                Assert.Equal("Económico-Administrativa", primero.Area);
            },
            segundo =>
            {
                Assert.Equal(23, segundo.IdPlanEstudios);
                Assert.Equal("Licenciatura en Redes y Servicios", segundo.NombreProgramaEducativo);
                Assert.Equal("Escolarizado", segundo.Modalidad);
                Assert.Equal("Plan 2010", segundo.Nombre);
                Assert.Equal("Técnica", segundo.Area);
            });

        planEstudiosRepositoryMock.Verify(r => r.ObtenerTodosAsync(), Times.Once);
    }

    //CP-06-13
    [Fact]
    public async Task ObtenerPlanesDeEstudioPorFiltro()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();

        var service = CrearService(
            planEstudiosRepositoryMock: planEstudiosRepositoryMock.Object,
            planEstudiosValidatorMock: planEstudiosValidatorMock.Object);

        var filtro = new FiltroPlanEstudiosDTO
        {
            IdEntidadAcademica = 31,
            IdProgramaEducativo = 12,
            Nombre = null
        };

        var planes = new List<PlanEstudios>
        {
            CrearPlanEstudiosLista(67, "Licenciatura en Ingeniería en Software", "Plan 2024", "Escolarizado", "Económico-Administrativa")
        };

        planEstudiosRepositoryMock
            .Setup(r => r.ObtenerPorFiltroAsync(filtro.IdEntidadAcademica, filtro.IdProgramaEducativo, filtro.Nombre, 1, 10))
            .ReturnsAsync(planes);

        planEstudiosRepositoryMock
            .Setup(r => r.ContarPorFiltroAsync(filtro.IdEntidadAcademica, filtro.IdProgramaEducativo, filtro.Nombre))
            .ReturnsAsync(1);

        var (resultado, total) = await service.ObtenerPorFiltroAsync(filtro, 1, 10);

        Assert.Equal(1, total);
        var dto = Assert.Single(resultado);
        Assert.Equal(67, dto.IdPlanEstudios);
        Assert.Equal("Licenciatura en Ingeniería en Software", dto.NombreProgramaEducativo);
        Assert.Equal("Escolarizado", dto.Modalidad);
        Assert.Equal("Plan 2024", dto.Nombre);
        Assert.Equal("Económico-Administrativa", dto.Area);

        planEstudiosValidatorMock.Verify(v => v.ValidarIndice(10), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.ObtenerPorFiltroAsync(filtro.IdEntidadAcademica, filtro.IdProgramaEducativo, filtro.Nombre, 1, 10), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.ContarPorFiltroAsync(filtro.IdEntidadAcademica, filtro.IdProgramaEducativo, filtro.Nombre), Times.Once);
    }

    //CP-06-14
    [Fact]
    public async Task ObtenerPlanDeEstudiosPorId()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        var experienciaEducativaRepositoryMock = new Mock<IExperienciaEducativaRepository>();
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();

        var service = CrearService(
            planEstudiosRepositoryMock: planEstudiosRepositoryMock.Object,
            experienciaEducativaRepositoryMock: experienciaEducativaRepositoryMock.Object,
            planEstudiosValidatorMock: planEstudiosValidatorMock.Object);

        var plan = CrearPlanEstudiosLista(67, "Licenciatura en Ingeniería en Software", "Plan 2024", "Escolarizado", "Económico-Administrativo");
        var experiencias = new List<ExperienciaEducativa>
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
        };

        planEstudiosValidatorMock
            .Setup(v => v.ValidarIdAsync(67))
            .Returns(Task.CompletedTask);

        planEstudiosRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(67))
            .ReturnsAsync(plan);

        experienciaEducativaRepositoryMock
            .Setup(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67))
            .ReturnsAsync(experiencias);

        var resultado = await service.ObtenerPorIdAsync(67);

        Assert.Equal(67, resultado.IdPlanEstudios);
        Assert.Equal("Licenciatura en Ingeniería en Software", resultado.NombreProgramaEducativo);
        Assert.Equal("Escolarizado", resultado.Modalidad);
        Assert.Equal("Plan 2024", resultado.Nombre);
        Assert.Equal("Económico-Administrativo", resultado.NombreAreaAcademica);
        Assert.Equal(2, resultado.ExperienciasEducativas.Count);

        Assert.Collection(resultado.ExperienciasEducativas,
            primera =>
            {
                Assert.Equal(93, primera.IdExperienciaEducativa);
                Assert.Equal("FBGR 80001", primera.Codigo);
                Assert.Equal("Computación Básica", primera.Nombre);
                Assert.Equal("Licenciado en Informática o carrera a fin...", primera.PerfilDocente);
            },
            segunda =>
            {
                Assert.Equal(187, segunda.IdExperienciaEducativa);
                Assert.Equal("FBGR 80014", segunda.Codigo);
                Assert.Equal("Procesos de Software", segunda.Nombre);
                Assert.Equal("Licenciado en Informática o carrera a fin...", segunda.PerfilDocente);
            });

        planEstudiosValidatorMock.Verify(v => v.ValidarIdAsync(67), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.ObtenerPorIdAsync(67), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67), Times.Once);
    }

    //CP-06-17
    [Fact]
    public async Task EditarPlanDeEstudios()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        var experienciaEducativaRepositoryMock = new Mock<IExperienciaEducativaRepository>();
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();

        var service = CrearService(
            planEstudiosRepositoryMock: planEstudiosRepositoryMock.Object,
            experienciaEducativaRepositoryMock: experienciaEducativaRepositoryMock.Object,
            planEstudiosValidatorMock: planEstudiosValidatorMock.Object);

        var dto = new EditarPlanEstudiosDTO
        {
            IdPlanEstudios = 67,
            NuevaLista = false,
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

        List<int>? idsEliminados = null;
        List<ExperienciaEducativa>? experienciasEditadas = null;
        List<ExperienciaEducativa>? experienciasNuevas = null;
        var sequence = new MockSequence();

        planEstudiosValidatorMock
            .Setup(v => v.ValidarEdicionAsync(dto))
            .Returns(Task.CompletedTask);

        planEstudiosRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(67))
            .ReturnsAsync(new PlanEstudios
            {
                IdPlanEstudios = 67,
                IdProgramaEducativo = 12,
                Nombre = "Plan 2024",
                Modalidad = "Escolarizado",
                IdArchivoPlan = 0
            });

        experienciaEducativaRepositoryMock
            .InSequence(sequence)
            .Setup(r => r.EliminarExperienciasEducativasPorIdsAsync(It.IsAny<List<int>>()))
            .Callback<List<int>>(ids => idsEliminados = ids)
            .Returns(Task.CompletedTask);

        experienciaEducativaRepositoryMock
            .InSequence(sequence)
            .Setup(r => r.ActualizarExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()))
            .Callback<List<ExperienciaEducativa>>(experiencias => experienciasEditadas = experiencias)
            .Returns(Task.CompletedTask);

        experienciaEducativaRepositoryMock
            .InSequence(sequence)
            .Setup(r => r.CrearExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()))
            .Callback<List<ExperienciaEducativa>>(experiencias => experienciasNuevas = experiencias)
            .Returns(Task.CompletedTask);

        await service.EditarAsync(dto);

        Assert.NotNull(idsEliminados);
        Assert.Single(idsEliminados!);
        Assert.Equal(187, idsEliminados![0]);

        Assert.NotNull(experienciasEditadas);
        var experienciaEditada = Assert.Single(experienciasEditadas!);
        Assert.Equal(93, experienciaEditada.IdExperienciaEducativa);
        Assert.Equal(67, experienciaEditada.IdPlanEstudios);
        Assert.Equal("FBGR 80002", experienciaEditada.Codigo);
        Assert.Equal("Computación Básica I", experienciaEditada.Nombre);
        Assert.Equal("Licenciado en Redes o carrera a fin...", experienciaEditada.PerfilDocente);

        Assert.NotNull(experienciasNuevas);
        var experienciaNueva = Assert.Single(experienciasNuevas!);
        Assert.Equal(67, experienciaNueva.IdPlanEstudios);
        Assert.Equal("FBGR 80067", experienciaNueva.Codigo);
        Assert.Equal("Estructuras de Datos", experienciaNueva.Nombre);
        Assert.Equal("Licenciado en informática o carrera a fin...", experienciaNueva.PerfilDocente);

        planEstudiosValidatorMock.Verify(v => v.ValidarEdicionAsync(dto), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.ObtenerPorIdAsync(67), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.EliminarExperienciasEducativasPorIdsAsync(It.IsAny<List<int>>()), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.ActualizarExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.CrearExperienciasEducativasAsync(It.IsAny<List<ExperienciaEducativa>>()), Times.Once);
    }

    //CP-06-31
    [Fact]
    public async Task EliminarPlanDeEstudios()
    {
        var planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
        var experienciaEducativaRepositoryMock = new Mock<IExperienciaEducativaRepository>();
        var planEstudiosValidatorMock = new Mock<IPlanEstudiosValidator>();
        var archivoRepositoryMock = new Mock<IArchivoRepository>();
        var archivoServiceMock = new Mock<IArchivoService>();

        var service = CrearService(
            planEstudiosRepositoryMock.Object,
            experienciaEducativaRepositoryMock.Object,
            planEstudiosValidatorMock.Object,
            archivoRepositoryMock.Object,
            archivoServiceMock.Object);

        var plan = new PlanEstudios
        {
            IdPlanEstudios = 67,
            IdProgramaEducativo = 12,
            Nombre = "Plan 2024",
            Modalidad = "Escolarizado",
            IdArchivoPlan = 15
        };

        var archivo = new Archivo
        {
            IdArchivo = 15,
            Nombre = "Plan 2024.xls",
            Ruta = "planes-estudios/plan-2024.xls"
        };

        var experiencias = new List<ExperienciaEducativa>
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
        };

        planEstudiosValidatorMock
            .Setup(v => v.ValidarIdAsync(67))
            .Returns(Task.CompletedTask);

        planEstudiosRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(67))
            .ReturnsAsync(plan);

        experienciaEducativaRepositoryMock
            .Setup(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67))
            .ReturnsAsync(experiencias);

        archivoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(15))
            .ReturnsAsync(archivo);

        experienciaEducativaRepositoryMock
            .Setup(r => r.EliminarExperienciasEducativasPorIdsAsync(It.IsAny<List<int>>()))
            .Returns(Task.CompletedTask);

        planEstudiosRepositoryMock
            .Setup(r => r.EliminarAsync(plan))
            .Returns(Task.CompletedTask);

        archivoRepositoryMock
            .Setup(r => r.EliminarAsync(archivo))
            .Returns(Task.CompletedTask);

        archivoServiceMock
            .Setup(s => s.EliminarAsync(archivo.Ruta))
            .Returns(Task.CompletedTask);

        await service.EliminarAsync(67);

        planEstudiosValidatorMock.Verify(v => v.ValidarIdAsync(67), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.ObtenerPorIdAsync(67), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.ObtenerExperienciasEducativasPorIdPlanEstudiosAsync(67), Times.Once);
        experienciaEducativaRepositoryMock.Verify(r => r.EliminarExperienciasEducativasPorIdsAsync(It.Is<List<int>>(ids => ids.Count == 2 && ids.Contains(93) && ids.Contains(187))), Times.Once);
        planEstudiosRepositoryMock.Verify(r => r.EliminarAsync(plan), Times.Once);
        archivoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(15), Times.Once);
        archivoRepositoryMock.Verify(r => r.EliminarAsync(archivo), Times.Once);
        archivoServiceMock.Verify(s => s.EliminarAsync(archivo.Ruta), Times.Once);
    }

    private static PlanEstudiosService CrearService(
        IPlanEstudiosRepository? planEstudiosRepositoryMock = null,
        IExperienciaEducativaRepository? experienciaEducativaRepositoryMock = null,
        IPlanEstudiosValidator? planEstudiosValidatorMock = null,
        IArchivoRepository? archivoRepositoryMock = null,
        IArchivoService? archivoServiceMock = null)
    {
        return new PlanEstudiosService(
            planEstudiosRepositoryMock ?? Mock.Of<IPlanEstudiosRepository>(),
            experienciaEducativaRepositoryMock ?? Mock.Of<IExperienciaEducativaRepository>(),
            planEstudiosValidatorMock ?? Mock.Of<IPlanEstudiosValidator>(),
            archivoRepositoryMock ?? Mock.Of<IArchivoRepository>(),
            archivoServiceMock ?? Mock.Of<IArchivoService>(),
            Mock.Of<ILogger<PlanEstudiosService>>());
    }

    private static CrearPlanEstudiosDTO CrearPlanEstudiosDtoValido()
    {
        return new CrearPlanEstudiosDTO
        {
            IdProgramaEducativo = 12,
            Nombre = "Plan 2014",
            Sistema = "Escolarizado",
            Archivo = new ArchivoPlanEstudiosDTO
            {
                Ruta = "planes-estudios/plan-2014.xls",
                NombreArchivo = "PlanLisoft.xls"
            },
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

    private static PlanEstudios CrearPlanEstudiosLista(
        int idPlanEstudios,
        string nombreProgramaEducativo,
        string nombrePlan,
        string modalidad,
        string nombreAreaAcademica)
    {
        return new PlanEstudios
        {
            IdPlanEstudios = idPlanEstudios,
            Nombre = nombrePlan,
            Modalidad = modalidad,
            IdProgramaEducativoNavigation = new ProgramaEducativo
            {
                Nombre = nombreProgramaEducativo,
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    IdAreaAcademicaNavigation = new AreaAcademica
                    {
                        Nombre = nombreAreaAcademica
                    }
                }
            }
        };
    }

    private static string CrearArchivoExcelPlanEstudios()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Plan");

        worksheet.Cell(1, 6).Value = "MATERIA_EE";
        worksheet.Cell(1, 7).Value = "CURSO_EE";
        worksheet.Cell(1, 8).Value = "DESC_EE";
        worksheet.Cell(1, 9).Value = "HT_EE";
        worksheet.Cell(1, 10).Value = "HP_EE";
        worksheet.Cell(1, 11).Value = "CREDITOS_EE";
        worksheet.Cell(1, 14).Value = "PERFIL_DOC";

        worksheet.Cell(2, 6).Value = "LISC";
        worksheet.Cell(2, 7).Value = "80012";
        worksheet.Cell(2, 8).Value = "Pruebas de Penetración";
        worksheet.Cell(2, 9).Value = "3";
        worksheet.Cell(2, 10).Value = "2";
        worksheet.Cell(2, 11).Value = "8";
        worksheet.Cell(2, 14).Value = "Licenciado en Informática o carrera a fin...";

        worksheet.Cell(3, 6).Value = "LISC";
        worksheet.Cell(3, 7).Value = "80007";
        worksheet.Cell(3, 8).Value = "Programación I";
        worksheet.Cell(3, 9).Value = "2";
        worksheet.Cell(3, 10).Value = "3";
        worksheet.Cell(3, 11).Value = "6";
        worksheet.Cell(3, 14).Value = "Licenciado en Informática o carrera a fin...";

        worksheet.Cell(4, 6).Value = "LISC";
        worksheet.Cell(4, 7).Value = "80020";
        worksheet.Cell(4, 8).Value = "Tecnologías para la Construcción de Software";
        worksheet.Cell(4, 9).Value = "3";
        worksheet.Cell(4, 10).Value = "0";
        worksheet.Cell(4, 11).Value = "6";
        worksheet.Cell(4, 14).Value = "Licenciado en Informática o carrera a fin...";

        workbook.SaveAs(path);
        return path;
    }
}
