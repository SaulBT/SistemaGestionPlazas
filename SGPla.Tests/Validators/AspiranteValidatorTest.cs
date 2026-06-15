using Moq;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.Docentes;
using SGPla.Models.DTOs.Grados;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

namespace SGPla.Tests.Validators
{
    public class AspiranteValidatorTest
    {
        private readonly Mock<IAspiranteRepository> _aspiranteRepositoryMock;
        private readonly Mock<IGradoRepository> _gradoRepositoryMock;
        private readonly AspiranteValidator _aspiranteValidator;

        public AspiranteValidatorTest()
        {
            _aspiranteRepositoryMock = new Mock<IAspiranteRepository>();
            _gradoRepositoryMock = new Mock<IGradoRepository>();
            _aspiranteValidator = new AspiranteValidator(
                _aspiranteRepositoryMock.Object,
                _gradoRepositoryMock.Object);
        }

        //CP-26-02
        [Fact]
        public async Task RegistrarAspiranteConCamposVacios()
        {
            var dto = new RegistrarDocenteDTO
            {
                Nombre = "",
                DescripcionPerfil = " ",
                ArchivosGenerales = null,
                Grados = new List<AgregarGradoDTO>()
            };

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre es obligatorio.", ex.Message);
        }

        //CP-26-03
        [Fact]
        public async Task RegistrarAspiranteSinArchivo()
        {
            var dto = new RegistrarDocenteDTO
            {
                Nombre = "Rafael Quintana López",
                DescripcionPerfil = "Licenciado en Ingeniería en software con experiencia de 3 años de docencia en educación media-superior.",
                ArchivosGenerales = null,
                Grados = new List<AgregarGradoDTO>
                {
                    new AgregarGradoDTO { Grado = "Lic.", Titulo = "Ingeniería en Software", Ultimo = true }
                }
            };

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No se cargó ningún archivo.", ex.Message);
        }

        //CP-26-04
        [Fact]
        public async Task RegistrarAspiranteSinGrados()
        {
            var dto = new RegistrarDocenteDTO
            {
                Nombre = "Rafael Quintana López",
                DescripcionPerfil = "Licenciado en Ingeniería en software con experiencia de 3 años de docencia en educación media-superior.",
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "doc_generales.docx",
                    RutaArchivo = "/Archivos/temp-data/doc_generales.docx"
                },
                Grados = new List<AgregarGradoDTO>()
            };

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("Debe haber al menos un Grado.", ex.Message);
        }

        //CP-26-06
        [Fact]
        public async Task ObtenerAspiranteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarIdAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante es inválida.", ex.Message);
        }

        //CP-26-07
        [Fact]
        public async Task ObtenerAspiranteInexistente()
        {
            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(67))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarIdAsync(67));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aspirante.", ex.Message);
        }

        //CP-26-09
        [Fact]
        public async Task EditarAspiranteConCamposVacios()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 90,
                Nombre = "",
                DescripcionPerfil = "",
                NuevoArchivo = false,
                ArchivosGenerales = new CargarArchivoDTO { NombreArchivo = "", RutaArchivo = "" }
            };

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre es obligatorio.", ex.Message);
        }

        //CP-26-10
        [Fact]
        public async Task EditarAspiranteConIdInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = -35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante es inválida.", ex.Message);
        }

        //CP-26-11
        [Fact]
        public async Task EditarAspiranteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aspirante.", ex.Message);
        }

        //CP-26-12
        [Fact]
        public async Task EditarAspiranteConGradoAgregadoConIdAspiranteInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 0, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante es inválida.", ex.Message);
        }

        //CP-26-13
        [Fact]
        public async Task EditarAspiranteConGradoAgregadoConIdAspiranteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 87, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(87))
                .ReturnsAsync(false);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aspirante.", ex.Message);
        }

        //CP-26-14
        [Fact]
        public async Task EditarAspiranteConGradoAgregadoConIdAspiranteDeOtroAspirante()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 91, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(91))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante de un Grado agregado no concuerda con la Id del Aspirante editado.", ex.Message);
        }

        //CP-26-15
        [Fact]
        public async Task EditarAspiranteConGradoEditadoConIdGradoInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = -1, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Grado es inválida.", ex.Message);
        }

        //CP-26-16
        [Fact]
        public async Task EditarAspiranteConGradoEditadoConIdGradoInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 372, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(372))
                .ReturnsAsync((Grado?)null);

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ningún grado con la Id 372.", ex.Message);
        }

        //CP-26-17
        [Fact]
        public async Task EditarAspiranteConGradoEditadoConIdAspiranteInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 0, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante es inválida.", ex.Message);
        }

        //CP-26-18
        [Fact]
        public async Task EditarAspiranteConGradoEditadoConIdAspiranteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 404, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(404))
                .ReturnsAsync(false);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aspirante.", ex.Message);
        }

        //CP-26-19
        [Fact]
        public async Task EditarAspiranteConGradoEditadoConIdAspiranteDeOtroAspirante()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 22, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(22))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante del Grado editado con Id 40 no concuerda con la Id del Aspirante editado.", ex.Message);
        }

        //CP-26-20
        [Fact]
        public async Task EditarAspiranteConGradoEliminadoConIdGradoInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });
            dto.IdsGradosEliminados.Add(0);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Grado es inválida.", ex.Message);
        }

        //CP-26-21
        [Fact]
        public async Task EditarAspiranteConGradoEliminadoConIdGradoInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });
            dto.IdsGradosEliminados.Add(10);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(10))
                .ReturnsAsync((Grado?)null);

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ningún grado con la Id 10.", ex.Message);
        }

        //CP-26-22
        [Fact]
        public async Task EditarAspiranteConGradoEliminadoConIdGradoDeOtroAspirante()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });
            dto.IdsGradosEliminados.Add(46);

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(46))
                .ReturnsAsync(new Grado { IdGrado = 46, IdDocente = 38 });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante del Grado eliminado con Id 46 no concuerda con la Id del Aspirante editado.", ex.Message);
        }

        //CP-26-23
        [Fact]
        public async Task EditarAspiranteConNuevoArchivoYCamposVacios()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO { NombreArchivo = "", RutaArchivo = "" }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = false } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre del Archivo es obligatorio.", ex.Message);
        }

        //CP-26-25
        [Fact]
        public async Task EliminarAspiranteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarIdAsync(-124));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aspirante es inválida.", ex.Message);
        }

        //CP-26-26
        [Fact]
        public async Task EliminarAspiranteInexistente()
        {
            _aspiranteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(90))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _aspiranteValidator.ValidarIdAsync(90));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aspirante.", ex.Message);
        }
    }
}
