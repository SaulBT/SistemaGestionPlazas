using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Interfaces;
using Xunit;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.DTOs.Oferta;

namespace SGPla.Tests.Services
{
    public class AvisoServiceTests
    {
        private readonly Mock<IAvisoRepository> _avisoRepositoryMock;
        private readonly Mock<IProgramacionAcademicaRepository> _programacionAcademicaRepositoryMock;
        private readonly Mock<IOfertaRepository> _ofertaRepositoryMock;
        private readonly Mock<IArchivoRepository> _archivoRepositoryMock;
        private readonly Mock<IArchivoService> _archivoServiceMock;
        private readonly Mock<IHorarioRepository> _horarioRepositoryMock;
        private readonly Mock<IAvisoValidator> _avisoValidatorMock;
        private readonly Mock<IPeriodoEscolarRepository> _periodoEscolarRepositoryMock;
        private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
        private readonly Mock<IArticuloRepository> _articuloRepositoryMock;
        private readonly Mock<IPlantillaService> _plantillaServiceMock;

        private readonly AvisoService _avisoService;

        public AvisoServiceTests()
        {
            _avisoRepositoryMock = new Mock<IAvisoRepository>();
            _programacionAcademicaRepositoryMock = new Mock<IProgramacionAcademicaRepository>();
            _ofertaRepositoryMock = new Mock<IOfertaRepository>();
            _archivoRepositoryMock = new Mock<IArchivoRepository>();
            _archivoServiceMock = new Mock<IArchivoService>();
            _horarioRepositoryMock = new Mock<IHorarioRepository>();
            _avisoValidatorMock = new Mock<IAvisoValidator>();
            _periodoEscolarRepositoryMock = new Mock<IPeriodoEscolarRepository>();
            _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
            _articuloRepositoryMock = new Mock<IArticuloRepository>();
            _plantillaServiceMock = new Mock<IPlantillaService>();

            _avisoService = new AvisoService(
                _avisoRepositoryMock.Object,
                _programacionAcademicaRepositoryMock.Object,
                _ofertaRepositoryMock.Object,
                _archivoRepositoryMock.Object,
                _archivoServiceMock.Object,
                _horarioRepositoryMock.Object,
                _avisoValidatorMock.Object,
                _periodoEscolarRepositoryMock.Object,
                _entidadAcademicaRepositoryMock.Object,
                _articuloRepositoryMock.Object,
                _plantillaServiceMock.Object
            );
        }

        // CP-01
        [Fact]
        public async Task CrearAviso_DatosCompletos_RegistraAvisoOfertasYHorarios()
        {
            var dto = new CrearAvisoDTO
            {
                IdEntidadAcademica = 1,
                IdPeriodo = 1,
                IdArticulo = 1,
                Folio = "AV-2026-001",
                FechaCreacion = new DateOnly(2026, 9, 3),
                FechaCT = new DateOnly(2026, 9, 10),
                FechaVacantes = new DateOnly(2026, 9, 15),
                Requisitos = "Título profesional y cédula.",
                Lugar = "Facultad de Estadística e Informática",
                Correo = "coordinacion@uv.mx",
                Modalidad = "Presencial",
                OfertasId = new List<int> { 10, 20 },
                Horarios = new List<CrearHorarioAvisoDTO>
                {
                    new CrearHorarioAvisoDTO
                    {
                        Fecha = "2026-09-10",
                        HoraInicio = "09:00",
                        HoraTermino = "14:00"
                    }
                }
            };

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(new EntidadAcademica
                {
                    IdEntidadAcademica = 1,
                    Nombre = "Facultad de Estadística e Informática",
                    Region = "Región Xalapa",
                    IdAreaAcademicaNavigation = new AreaAcademica { Nombre = "Económico-Administrativa" }
                });

            _articuloRepositoryMock
                .Setup(r => r.ObtenerArticuloPorIdAsync(dto.IdArticulo))
                .ReturnsAsync(new Articulo { IdArticulo = 1, Numero = "70", Descripcion = "Docente de Asignatura" });

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(dto.IdPeriodo))
                .ReturnsAsync(new Periodo
                {
                    IdPeriodo = 1,
                    Codigo = "202601"
                });

            _ofertaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Oferta
                {
                    IdOferta = id,
                    Nrc = "12345",
                    IdProgramaEducativoNavigation = new ProgramaEducativo { Nombre = "Ingeniería de Software" },
                    IdExperienciaEducativaNavigation = new ExperienciaEducativa
                    {
                        Nombre = "Principios de Diseño de Software",
                        Horas = "4",
                        PerfilDocente = "Ingeniero de Software"
                    }
                });

            _horarioRepositoryMock
                .Setup(r => r.ObtenerPorIdOferta(It.IsAny<int>()))
                .ReturnsAsync(new List<Horario>());

            _plantillaServiceMock
                .Setup(s => s.GenerarAvisoAsync(It.IsAny<PlantillaAvisoDTO>()))
                .ReturnsAsync(50);

            _avisoRepositoryMock
                .Setup(r => r.CrearAsync(It.IsAny<Aviso>()))
                .ReturnsAsync((Aviso a) =>
                {
                    a.IdAviso = 1;
                    return a;
                });

            _avisoRepositoryMock
                .Setup(r => r.AsociarOfertasPorAviso(dto.OfertasId, 1))
                .Returns(Task.CompletedTask);

            _horarioRepositoryMock
                .Setup(r => r.CrearHorarios(It.IsAny<List<Horario>>()))
                .Returns(Task.CompletedTask);

            var excepcion = await Record.ExceptionAsync(() => _avisoService.CrearAviso(dto));

            Assert.Null(excepcion);

            _plantillaServiceMock.Verify(s => s.GenerarAvisoAsync(It.IsAny<PlantillaAvisoDTO>()), Times.Once);
            _avisoRepositoryMock.Verify(r => r.CrearAsync(It.Is<Aviso>(a =>
                a.Folio == dto.Folio &&
                a.IdEntidadAcademica == dto.IdEntidadAcademica &&
                a.Estado == "Creado" &&
                a.IdArchivoOriginal == 50)), Times.Once);
            _avisoRepositoryMock.Verify(r => r.AsociarOfertasPorAviso(dto.OfertasId, 1), Times.Once);
            _horarioRepositoryMock.Verify(r => r.CrearHorarios(It.Is<List<Horario>>(h => h.Count == 1)), Times.Once);
        }

        // CP-02
        [Fact]
        public async Task ObtenerTodosAvisosAsync_ConFiltros_RetornaListaAvisosYTotal()
        {
            var filtroDTO = new FiltroAvisosDTO
            {
                Busqueda = "AV-2026",
                IdPeriodo = 1,
                IdEntidadAcademica = 1,
                Pagina = 1,
                Cantidad = 10
            };

            var avisosSimulados = new List<Aviso>
            {
                new Aviso
                {
                    IdAviso = 1,
                    IdEntidadAcademica = 1,
                    IdPeriodo = 1,
                    IdArticulo = 1,
                    Folio = "AV-2026-001",
                    FechaCreacion = new DateOnly(2026, 9, 1),
                    Estado = "Creado",
                    Archivado = false,
                    Comentarios = "",
                    IdEntidadAcademicaNavigation = new EntidadAcademica
                    {
                        Nombre = "Facultad de Estadística e Informática"
                    },
                    IdArticuloNavigation = new Articulo
                    {
                        Numero = "70"
                    }
                }
            };

            _avisoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(filtroDTO))
                .ReturnsAsync(avisosSimulados);

            _avisoRepositoryMock
                .Setup(r => r.ContarAsync(filtroDTO))
                .ReturnsAsync(1);

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(1))
                .ReturnsAsync(new Periodo
                {
                    IdPeriodo = 1,
                    Codigo = "202601"
                });

            var (items, total) = await _avisoService.ObtenerTodosAvisosAsync(filtroDTO);

            Assert.NotNull(items);
            Assert.Single(items);
            Assert.Equal(1, total);

            var avisoObtenido = items[0];
            Assert.Equal(1, avisoObtenido.IdAviso);
            Assert.Equal("AV-2026-001", avisoObtenido.Folio);
            Assert.Equal("Creado", avisoObtenido.Estado);
            Assert.False(avisoObtenido.Archivado);

            _avisoRepositoryMock.Verify(r => r.ObtenerTodosAsync(filtroDTO), Times.Once);
            _avisoRepositoryMock.Verify(r => r.ContarAsync(filtroDTO), Times.Once);
            _periodoEscolarRepositoryMock.Verify(r => r.ObtenerPorIdAsync(1), Times.Once);
        }

        // CP-03
        [Fact]
        public async Task ObtenerTodosAvisosAsync_SinCoincidencias_RetornaListaVaciaYTotalCero()
        {
            var filtroDTO = new FiltroAvisosDTO
            {
                Busqueda = "Inexistente",
                Pagina = 1,
                Cantidad = 10
            };

            _avisoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(filtroDTO))
                .ReturnsAsync(new List<Aviso>());

            _avisoRepositoryMock
                .Setup(r => r.ContarAsync(filtroDTO))
                .ReturnsAsync(0);

            var (items, total) = await _avisoService.ObtenerTodosAvisosAsync(filtroDTO);

            Assert.NotNull(items);
            Assert.Empty(items);
            Assert.Equal(0, total);

            _avisoRepositoryMock.Verify(r => r.ObtenerTodosAsync(filtroDTO), Times.Once);
            _avisoRepositoryMock.Verify(r => r.ContarAsync(filtroDTO), Times.Once);
        }

        // CP-04
        [Fact]
        public async Task ObtenerAvisoPorIDAsync_IdExistente_RetornaDatosAvisoDTO()
        {
            const int idAviso = 1;

            var avisoSimulado = new Aviso
            {
                IdAviso = idAviso,
                IdEntidadAcademica = 1,
                IdPeriodo = 1,
                IdArticulo = 1,
                Folio = "AV-2026-001",
                FechaCreacion = new DateOnly(2026, 9, 1),
                FechaVacantes = new DateOnly(2026, 9, 15),
                Modalidad = "Presencial",
                Requisitos = "Título profesional y cédula.",
                Lugar = "Facultad de Estadística e Informática",
                IdEntidadAcademicaNavigation = new EntidadAcademica
                {
                    Region = "Región Xalapa",
                    Nombre = "Facultad de Estadística e Informática"
                },
                IdArticuloNavigation = new Articulo
                {
                    Numero = "70"
                }
            };

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.ObtenerPorIDAsync(idAviso))
                .ReturnsAsync(avisoSimulado);

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(1))
                .ReturnsAsync(new Periodo
                {
                    IdPeriodo = 1,
                    Codigo = "202601"
                });

            _ofertaRepositoryMock
                .Setup(r => r.ObtenerPorAvisoAsync(idAviso))
                .ReturnsAsync(new List<Oferta>());

            _horarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAviso(idAviso))
                .ReturnsAsync(new List<Horario>());

            var resultado = await _avisoService.ObtenerAvisoPorIDAsync(idAviso);

            Assert.NotNull(resultado);
            Assert.Equal(idAviso, resultado.IdAviso);
            Assert.Equal("AV-2026-001", resultado.Folio);
            Assert.Equal("Presencial", resultado.Modalidad);
            Assert.Equal("Título profesional y cédula.", resultado.Requisitos);
            Assert.Equal("Facultad de Estadística e Informática", resultado.Lugar);

            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.ObtenerPorIDAsync(idAviso), Times.Once);
        }

        // CP-07
        [Fact]
        public async Task EnviarARevisionAsync_DatosValidos_EnviaARevisionCorrectamente()
        {
            var revisionDTO = new RevisionDTO
            {
                IdAviso = 1,
                Comentarios = "Se remite para revisión por parte de la DGAA."
            };

            _avisoValidatorMock
                .Setup(v => v.ValidarEnviarARevisionAsync(revisionDTO))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.EnviarARevisionAsync(revisionDTO.IdAviso, revisionDTO.Comentarios))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.EnviarARevisionAsync(revisionDTO));

            Assert.Null(ex);

            _avisoValidatorMock.Verify(v => v.ValidarEnviarARevisionAsync(revisionDTO), Times.Once);
            _avisoRepositoryMock.Verify(r => r.EnviarARevisionAsync(revisionDTO.IdAviso, revisionDTO.Comentarios), Times.Once);
        }

        // CP-12
        [Fact]
        public async Task FirmarAvisoAsync_DatosValidos_GuardaArchivoYFirmaAviso()
        {
            const int idAviso = 1;
            var archivoDTO = new CargarArchivoDTO
            {
                NombreArchivo = "AvisoFirmado.pdf",
                RutaArchivo = "C:/uploads/temp/AvisoFirmado.pdf"
            };

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoValidatorMock
                .Setup(v => v.ValidarArchivoAsync(archivoDTO))
                .Returns(Task.CompletedTask);

            var archivoGuardadoDTO = new DatosArchivoGuardadoDTO
            {
                NombreOriginal = archivoDTO.NombreArchivo,
                Ruta = "archivos/avisos/AvisoFirmado_123.pdf",
                Tipo = "application/pdf",
                Tamanio = 2048
            };

            _archivoServiceMock
                .Setup(s => s.GuardarAsync(archivoDTO.RutaArchivo, archivoDTO.NombreArchivo, "aviso-firmado"))
                .ReturnsAsync(archivoGuardadoDTO);

            _archivoRepositoryMock
                .Setup(r => r.CrearAsync(It.IsAny<Archivo>()))
                .ReturnsAsync((Archivo a) =>
                {
                    a.IdArchivo = 10;
                    return a;
                });

            _avisoRepositoryMock
                .Setup(r => r.FirmarAsync(idAviso, 10))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.FirmarAvisoAsync(idAviso, archivoDTO));

            Assert.Null(ex);

            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoValidatorMock.Verify(v => v.ValidarArchivoAsync(archivoDTO), Times.Once);
            _archivoServiceMock.Verify(s => s.GuardarAsync(archivoDTO.RutaArchivo, archivoDTO.NombreArchivo, "aviso-firmado"), Times.Once);
            _archivoRepositoryMock.Verify(r => r.CrearAsync(It.Is<Archivo>(a => a.Nombre == archivoGuardadoDTO.NombreOriginal)), Times.Once);
            _avisoRepositoryMock.Verify(r => r.FirmarAsync(idAviso, 10), Times.Once);
        }

        // CP-18
        [Fact]
        public async Task PublicarAvisoAsync_DatosValidos_PublicaCorrectamente()
        {
            const int idAviso = 1;
            const string url = "https://www.uv.mx/convocatorias/aviso-1.pdf";

            _avisoValidatorMock
                .Setup(v => v.ValidarPublicacionAsync(idAviso, url))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.PublicarAsync(idAviso, url))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.PublicarAvisoAsync(idAviso, url));

            Assert.Null(ex);

            _avisoValidatorMock.Verify(v => v.ValidarPublicacionAsync(idAviso, url), Times.Once);
            _avisoRepositoryMock.Verify(r => r.PublicarAsync(idAviso, url), Times.Once);
        }

        // CP-22
        [Fact]
        public async Task ArchivarAvisoAsync_IdValida_CambiaEstadoArchivadoATrue()
        {
            const int idAviso = 1;

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.CambiarStatusArchivadoAsync(idAviso, true))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.ArchivarAvisoAsync(idAviso));

            Assert.Null(ex);

            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.CambiarStatusArchivadoAsync(idAviso, true), Times.Once);
        }

        // CP-25
        [Fact]
        public async Task DesarchivarAvisoAsync_IdValida_CambiaEstadoArchivadoAFalse()
        {
            const int idAviso = 1;

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.CambiarStatusArchivadoAsync(idAviso, false))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.DesarchivarAvisoAsync(idAviso));

            Assert.Null(ex);

            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.CambiarStatusArchivadoAsync(idAviso, false), Times.Once);
        }

        // CP-28
        [Fact]
        public async Task VerComentariosAsync_AvisoExistente_RetornaComentarios()
        {
            const int idAviso = 1;
            const string comentariosEsperados = "Corregir número de plazas en oferta";

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.VerComentariosAsync(idAviso))
                .ReturnsAsync(comentariosEsperados);

            var resultado = await _avisoService.VerComentariosAsync(idAviso);

            Assert.Equal(comentariosEsperados, resultado);
            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.VerComentariosAsync(idAviso), Times.Once);
        }

        // CP-30
        [Fact]
        public async Task VerificarEstadoAvisoAsync_EstadoCoincide_RetornaTrue()
        {
            const int idAviso = 1;
            const string estado = "Creado";

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.VerificarEstadoAsync(idAviso, estado))
                .ReturnsAsync(true);

            var resultado = await _avisoService.VerificarEstadoAvisoAsync(idAviso, estado);

            Assert.True(resultado);
            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.VerificarEstadoAsync(idAviso, estado), Times.Once);
        }

        // CP-32
        [Fact]
        public async Task EliminarAvisoPorId_IdExistente_EliminaCorrectamente()
        {
            const int idAviso = 1;

            _avisoValidatorMock
                .Setup(v => v.ValidarIdAsync(idAviso))
                .Returns(Task.CompletedTask);

            _avisoRepositoryMock
                .Setup(r => r.EliminarAsync(idAviso))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _avisoService.EliminarAvisoPorId(idAviso));

            Assert.Null(ex);
            _avisoValidatorMock.Verify(v => v.ValidarIdAsync(idAviso), Times.Once);
            _avisoRepositoryMock.Verify(r => r.EliminarAsync(idAviso), Times.Once);
        }

        // CP-35
        [Fact]
        public async Task ObtenerPlanesConOfertasAviso_ParametrosValidos_RetornaListaPlanes()
        {
            const int idEntidadAcademica = 1;
            const int idPeriodo = 1;
            const int idArticulo = 1;

            var listaEsperada = new List<OfertaPlanEstudiosAvisoDTO>
            {
                new OfertaPlanEstudiosAvisoDTO
                {
                    Nombre = "Licenciatura en Ingeniería de Software",
                    Ofertas = new List<DatosOfertaAvisoDTO>()
                }
            };

            _ofertaRepositoryMock
                .Setup(r => r.ObtenerPlanesEstudioCrearAviso(idEntidadAcademica, idPeriodo, idArticulo))
                .ReturnsAsync(listaEsperada);

            var resultado = await _avisoService.ObtenerPlanesConOfertasAviso(idEntidadAcademica, idPeriodo, idArticulo);

            Assert.NotNull(resultado);
            Assert.Single(resultado);
            Assert.Equal("Licenciatura en Ingeniería de Software", resultado[0].Nombre);
            _ofertaRepositoryMock.Verify(r => r.ObtenerPlanesEstudioCrearAviso(idEntidadAcademica, idPeriodo, idArticulo), Times.Once);
        }
    }
}