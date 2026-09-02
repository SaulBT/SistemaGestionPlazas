//using Moq;
//using SGPla.Models;
//using SGPla.Models.DTOs.Archivo;
//using SGPla.Models.DTOs.Docentes;
//using SGPla.Models.DTOs.Grados;
//using SGPla.Repositories.Interfaces;
//using SGPla.Services.Implementations;
//using SGPla.Services.Interfaces;
//using SGPla.Validations.Interfaces;

//namespace SGPla.Tests.Services
//{
//    public class AspiranteServiceTest
//    {
//        private readonly AspiranteService _aspiranteService;
//        private readonly Mock<IAspiranteRepository> _aspiranteRepositoryMock;
//        private readonly Mock<IGradoRepository> _gradoRepositoryMock;
//        private readonly Mock<IArchivoRepository> _archivoRepositoryMock;
//        private readonly Mock<IArchivoService> _archivoServiceMock;
//        private readonly Mock<IDocenteValidator> _docenteValidatorMock;

//        public AspiranteServiceTest()
//        {
//            _aspiranteRepositoryMock = new Mock<IAspiranteRepository>();
//            _gradoRepositoryMock = new Mock<IGradoRepository>();
//            _archivoRepositoryMock = new Mock<IArchivoRepository>();
//            _archivoServiceMock = new Mock<IArchivoService>();
//            _docenteValidatorMock = new Mock<IDocenteValidator>();
//            _aspiranteService = new AspiranteService(
//                _aspiranteRepositoryMock.Object,
//                _gradoRepositoryMock.Object,
//                _archivoRepositoryMock.Object,
//                _archivoServiceMock.Object,
//                _docenteValidatorMock.Object);
//        }

//        //CP-26-01
//        [Fact]
//        public async Task RegistrarAspirante()
//        {
//            var dto = new RegistrarDocenteDTO
//            {
//                Nombre = "Rafael Quintana López",
//                DescripcionPerfil = "Licenciado en Ingeniería en software con experiencia de 3 años de docencia en educación media-superior.",
//                ArchivosGenerales = new CargarArchivoDTO
//                {
//                    NombreArchivo = "doc_generales.docx",
//                    RutaArchivo = "/Archivos/temp-data/doc_generales.docx"
//                },
//                Grados = new List<AgregarGradoDTO>
//                {
//                    new AgregarGradoDTO { Grado = "Lic.", Titulo = "Ingeniería en Software", Ultimo = true }
//                }
//            };

//            _docenteValidatorMock
//                .Setup(v => v.ValidarRegistroAsync(dto))
//                .Returns(Task.CompletedTask);

//            _archivoServiceMock
//                .Setup(s => s.GuardarAsync(dto.ArchivosGenerales.RutaArchivo, dto.ArchivosGenerales.NombreArchivo, "archivos-docente"))
//                .ReturnsAsync(new DatosArchivoGuardadoDTO { NombreOriginal = "doc_generales.docx", Ruta = "/ruta/doc_generales.docx", Tipo = "docx", Tamanio = 1024 });

//            _archivoRepositoryMock
//                .Setup(r => r.CrearAsync(It.IsAny<Archivo>()))
//                .ReturnsAsync(new Archivo { IdArchivo = 1 });

//            _aspiranteRepositoryMock
//                .Setup(r => r.RegistrarAsync(It.IsAny<Docente>()))
//                .ReturnsAsync(new Docente { IdDocente = 12 });

//            _gradoRepositoryMock
//                .Setup(r => r.AgregarAsync(It.IsAny<Grado>()))
//                .Returns(Task.CompletedTask);

//            await _aspiranteService.RegistrarAspiranteAsync(dto);

//            _aspiranteRepositoryMock.Verify(r => r.RegistrarAsync(It.Is<Docente>(d =>
//                d.Nombre == dto.Nombre &&
//                d.DescripcionPerfil == dto.DescripcionPerfil)), Times.Once);

//            _gradoRepositoryMock.Verify(r => r.AgregarAsync(It.Is<Grado>(g =>
//                g.Grado1 == "Lic." &&
//                g.Titulo == "Ingeniería en Software" &&
//                g.Ultimo == true)), Times.Once);
//        }

//        //CP-26-05
//        [Fact]
//        public async Task ObtenerAspirante()
//        {
//            int id = 12;

//            var aspirante = new Docente
//            {
//                IdDocente = 12,
//                Nombre = "Sofía Lizeth Mercado Velasquez",
//                DescripcionPerfil = "Licenciada en Redes y Servicios, con maestría en Ciencia de Datos y Doctorado en Inteligencia Artificial.",
//                IdArchivosGenerales = 56
//            };

//            var grados = new List<Grado>
//            {
//                new Grado { IdGrado = 25, IdDocente = 12, Grado1 = "Licenciatura", Titulo = "Ingeniería en Redes y Servicios", Ultimo = false },
//                new Grado { IdGrado = 26, IdDocente = 12, Grado1 = "Maestría",     Titulo = "Ciencia de Datos",                Ultimo = false },
//                new Grado { IdGrado = 27, IdDocente = 12, Grado1 = "Doctorado",    Titulo = "Inteligencia Artificial",         Ultimo = true  }
//            };

//            _docenteValidatorMock
//                .Setup(v => v.ValidarIdAsync(id))
//                .Returns(Task.CompletedTask);

//            _aspiranteRepositoryMock
//                .Setup(r => r.ObtenerPorIdAsync(id))
//                .ReturnsAsync(aspirante);

//            _gradoRepositoryMock
//                .Setup(r => r.ObtenerTodosAsync(id))
//                .ReturnsAsync(grados);

//            var resultado = await _aspiranteService.ObtenerAspiranteAsync(id);

//            Assert.NotNull(resultado);
//            Assert.Equal(12, resultado.IdDocente);
//            Assert.Equal("Sofía Lizeth Mercado Velasquez", resultado.Nombre);
//            Assert.Equal("Licenciada en Redes y Servicios, con maestría en Ciencia de Datos y Doctorado en Inteligencia Artificial.", resultado.DescripcionPerfil);
//            Assert.Equal(56, resultado.IdArchivosGenerales);
//            Assert.Equal(3, resultado.Grados.Count);

//            Assert.Equal(25, resultado.Grados[0].IdGrado);
//            Assert.Equal("Licenciatura", resultado.Grados[0].Grado);
//            Assert.Equal("Ingeniería en Redes y Servicios", resultado.Grados[0].Titulo);
//            Assert.False(resultado.Grados[0].Ultimo);

//            Assert.Equal(26, resultado.Grados[1].IdGrado);
//            Assert.Equal("Maestría", resultado.Grados[1].Grado);
//            Assert.Equal("Ciencia de Datos", resultado.Grados[1].Titulo);
//            Assert.False(resultado.Grados[1].Ultimo);

//            Assert.Equal(27, resultado.Grados[2].IdGrado);
//            Assert.Equal("Doctorado", resultado.Grados[2].Grado);
//            Assert.Equal("Inteligencia Artificial", resultado.Grados[2].Titulo);
//            Assert.True(resultado.Grados[2].Ultimo);
//        }

//        //CP-26-08
//        [Fact]
//        public async Task EditarAspirante()
//        {
//            var dto = new EditarDocenteDTO
//            {
//                IdDocente = 35,
//                Nombre = "Aureliano Gabriel Buendia Márquez",
//                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
//                NuevoArchivo = true,
//                ArchivosGenerales = new CargarArchivoDTO
//                {
//                    NombreArchivo = "misDocsActualizados.docx",
//                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
//                }
//            };
//            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
//            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

//            var aspirante = new Docente { IdDocente = 35, IdArchivosGenerales = 931 };
//            var archivoAnterior = new Archivo { IdArchivo = 931, Ruta = "/Archivos/antiguo.docx" };
//            var archivoNuevo = new DatosArchivoGuardadoDTO
//            {
//                NombreOriginal = "misDocsActualizados.docx",
//                Ruta = "/Archivos/misDocsActualizados.docx",
//                Tipo = "docx",
//                Tamanio = 2048
//            };

//            _docenteValidatorMock
//                .Setup(v => v.ValidarEdicionAsync(dto))
//                .Returns(Task.CompletedTask);

//            _aspiranteRepositoryMock
//                .Setup(r => r.ObtenerPorIdAsync(35))
//                .ReturnsAsync(aspirante);

//            _archivoServiceMock
//                .Setup(s => s.GuardarAsync(dto.ArchivosGenerales.RutaArchivo, dto.ArchivosGenerales.NombreArchivo, "archivos-docente"))
//                .ReturnsAsync(archivoNuevo);

//            _archivoRepositoryMock
//                .Setup(r => r.ObtenerPorIdAsync(931))
//                .ReturnsAsync(archivoAnterior);

//            _archivoRepositoryMock
//                .Setup(r => r.EliminarAsync(It.IsAny<Archivo>()))
//                .Returns(Task.CompletedTask);

//            _gradoRepositoryMock
//                .Setup(r => r.AgregarAsync(It.IsAny<Grado>()))
//                .Returns(Task.CompletedTask);

//            _gradoRepositoryMock
//                .Setup(r => r.EditarAsync(It.IsAny<Grado>()))
//                .Returns(Task.CompletedTask);

//            _aspiranteRepositoryMock
//                .Setup(r => r.EditarAsync(It.IsAny<Docente>()))
//                .Returns(Task.CompletedTask);

//            _archivoServiceMock
//                .Setup(s => s.EliminarAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            await _aspiranteService.EditarAspiranteAsync(dto);

//            _aspiranteRepositoryMock.Verify(r => r.EditarAsync(It.Is<Docente>(d =>
//                d.Nombre == dto.Nombre &&
//                d.DescripcionPerfil == dto.DescripcionPerfil)), Times.Once);

//            _gradoRepositoryMock.Verify(r => r.AgregarAsync(It.Is<Grado>(g =>
//                g.Grado1 == "Doctorado" && g.Titulo == "Culturas del Preclásico" && g.Ultimo == true)), Times.Once);

//            _gradoRepositoryMock.Verify(r => r.EditarAsync(It.Is<Grado>(g =>
//                g.IdGrado == 40 && g.Grado1 == "Licenciatura" && g.Ultimo == false)), Times.Once);
//        }

//        //CP-26-24
//        [Fact]
//        public async Task EliminarAspirante()
//        {
//            int id = 39;

//            var aspirante = new Docente { IdDocente = 39, IdArchivosGenerales = 5 };
//            var archivo = new Archivo { IdArchivo = 5, Ruta = "/Archivos/doc39.docx" };

//            _docenteValidatorMock
//                .Setup(v => v.ValidarIdAsync(id))
//                .Returns(Task.CompletedTask);

//            _aspiranteRepositoryMock
//                .Setup(r => r.ObtenerPorIdAsync(id))
//                .ReturnsAsync(aspirante);

//            _gradoRepositoryMock
//                .Setup(r => r.ObtenerTodosAsync(id))
//                .ReturnsAsync(new List<Grado>());

//            _archivoRepositoryMock
//                .Setup(r => r.ObtenerPorIdAsync(5))
//                .ReturnsAsync(archivo);

//            _archivoRepositoryMock
//                .Setup(r => r.EliminarAsync(archivo))
//                .Returns(Task.CompletedTask);

//            _archivoServiceMock
//                .Setup(s => s.EliminarAsync(archivo.Ruta))
//                .Returns(Task.CompletedTask);

//            _aspiranteRepositoryMock
//                .Setup(r => r.EliminarAsync(aspirante))
//                .Returns(Task.CompletedTask);

//            await _aspiranteService.EliminarAspiranteAsync(id);

//            _aspiranteRepositoryMock.Verify(r => r.EliminarAsync(aspirante), Times.Once);
//            _archivoRepositoryMock.Verify(r => r.EliminarAsync(archivo), Times.Once);
//        }
//    }
//}
