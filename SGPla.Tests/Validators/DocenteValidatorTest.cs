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
    public class DocenteValidatorTest
    {
        private readonly Mock<IDocenteRepository> _docenteRepositoryMock;
        private readonly Mock<IGradoRepository> _gradoRepositoryMock;
        private readonly DocenteValidator _docenteValidator;

        public DocenteValidatorTest()
        {
            _docenteRepositoryMock = new Mock<IDocenteRepository>();
            _gradoRepositoryMock = new Mock<IGradoRepository>();
            _docenteValidator = new DocenteValidator(
                _docenteRepositoryMock.Object,
                _gradoRepositoryMock.Object);
        }

        //CP-25-02
        [Fact]
        public async Task RegistrarDocenteConCamposVacios()
        {
            var dto = new RegistrarDocenteDTO
            {
                Nombre = "",
                DescripcionPerfil = " ",
                ArchivosGenerales = null,
                Grados = new List<AgregarGradoDTO>(),
                NumeroPersonal = "",
                Puesto = ""
            };

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre es obligatorio.", ex.Message);
        }

        //CP-25-03
        [Fact]
        public async Task RegistrarDocenteSinArchivo()
        {
            var dto = new RegistrarDocenteDTO
            {
                Nombre = "Rafael Quintana López",
                DescripcionPerfil = "Licenciado en Ingeniería en software con experiencia de 3 años de docencia en educación media-superior.",
                ArchivosGenerales = null,
                Grados = new List<AgregarGradoDTO>
                {
                    new AgregarGradoDTO { Grado = "Lic.", Titulo = "Ingeniería en Software", Ultimo = true }
                },
                NumeroPersonal = "27",
                Puesto = "Docente"
            };

            _docenteRepositoryMock
                .Setup(r => r.ExistePorNumeroAsync(dto.NumeroPersonal))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No se cargó ningún archivo.", ex.Message);
        }

        //CP-25-04
        [Fact]
        public async Task RegistrarDocenteSinGrados()
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
                Grados = new List<AgregarGradoDTO>(),
                NumeroPersonal = "27",
                Puesto = "Docente"
            };

            _docenteRepositoryMock
                .Setup(r => r.ExistePorNumeroAsync(dto.NumeroPersonal))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("Debe haber al menos un Grado.", ex.Message);
        }

        //CP-25-05
        [Fact]
        public async Task RegistrarDocenteConPuestoInvalido()
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
                Grados = new List<AgregarGradoDTO>
                {
                    new AgregarGradoDTO { Grado = "Lic.", Titulo = "Ingeniería en Software", Ultimo = true }
                },
                NumeroPersonal = "27",
                Puesto = "Encargado de Biblioteca"
            };

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Puesto no es válido.", ex.Message);
        }

        //CP-25-07
        [Fact]
        public async Task ObtenerDocenteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarIdAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente es inválida.", ex.Message);
        }

        //CP-25-08
        [Fact]
        public async Task ObtenerDocenteInexistente()
        {
            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(67))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarIdAsync(67));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Docente.", ex.Message);
        }

        //CP-25-10
        [Fact]
        public async Task EditarDocenteConCamposVacios()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 90,
                Nombre = "",
                DescripcionPerfil = "",
                NumeroPersonal = "",
                Puesto = "",
                NuevoArchivo = false,
                ArchivosGenerales = new CargarArchivoDTO { NombreArchivo = "", RutaArchivo = "" }
            };

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre es obligatorio.", ex.Message);
        }

        //CP-25-11
        [Fact]
        public async Task EditarDocenteConIdInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = -35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente es inválida.", ex.Message);
        }

        //CP-25-12
        [Fact]
        public async Task EditarDocenteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 90,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(90))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Docente.", ex.Message);
        }

        //CP-25-13
        [Fact]
        public async Task EditarDocenteConGradoAgregadoConIdDocenteInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 0, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente es inválida.", ex.Message);
        }

        //CP-25-14
        [Fact]
        public async Task EditarDocenteConGradoAgregadoConIdDocenteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 87, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(87))
                .ReturnsAsync(false);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Docente.", ex.Message);
        }

        //CP-25-15
        [Fact]
        public async Task EditarDocenteConGradoAgregadoConIdDocenteDeOtroDocente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 91, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(91))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente de un Grado agregado no concuerda con la Id del Docente editado.", ex.Message);
        }

        //CP-25-16
        [Fact]
        public async Task EditarDocenteConGradoEditadoConIdGradoInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = -1, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Grado es inválida.", ex.Message);
        }

        //CP-25-17
        [Fact]
        public async Task EditarDocenteConGradoEditadoConIdGradoInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 372, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(372))
                .ReturnsAsync((Grado?)null);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ningún grado con la Id 372.", ex.Message);
        }

        //CP-25-18
        [Fact]
        public async Task EditarDocenteConGradoEditadoConIdDocenteInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 0, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente es inválida.", ex.Message);
        }

        //CP-25-19
        [Fact]
        public async Task EditarDocenteConGradoEditadoConIdDocenteInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 404, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(404))
                .ReturnsAsync(false);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Docente.", ex.Message);
        }

        //CP-25-20
        [Fact]
        public async Task EditarDocenteConGradoEditadoConIdDocenteDeOtroDocente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 22, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(22))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente del Grado editado con Id 40 no concuerda con la Id del Docente editado.", ex.Message);
        }

        //CP-25-21
        [Fact]
        public async Task EditarDocenteConGradoEliminadoConIdGradoInvalida()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
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

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Grado es inválida.", ex.Message);
        }

        //CP-25-22
        [Fact]
        public async Task EditarDocenteConGradoEliminadoConIdGradoInexistente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
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

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(10))
                .ReturnsAsync((Grado?)null);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ningún grado con la Id 10.", ex.Message);
        }

        //CP-25-23
        [Fact]
        public async Task EditarDocenteConGradoEliminadoConIdGradoDeOtroDocente()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
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

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = true } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(46))
                .ReturnsAsync(new Grado { IdGrado = 46, IdDocente = 38 });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente del Grado eliminado con Id 46 no concuerda con la Id del Docente editado.", ex.Message);
        }

        //CP-25-24
        [Fact]
        public async Task EditarDocenteConPuestoInvalido()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Personal Administrativo",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO
                {
                    NombreArchivo = "misDocsActualizados.docx",
                    RutaArchivo = "/Archivos/temp-data/misDocsActualizados.docx"
                }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Puesto no es válido.", ex.Message);
        }

        //CP-25-25
        [Fact]
        public async Task EditarDocenteConNuevoArchivoYCamposVacios()
        {
            var dto = new EditarDocenteDTO
            {
                IdDocente = 35,
                Nombre = "Aureliano Gabriel Buendia Márquez",
                DescripcionPerfil = "Cursé la carrera de Licenciatura en Arqueología en la ENAH de 1999 a 2003, he trabajado como docente en escuelas rurales y en la propia ENAH desde 2015.",
                NumeroPersonal = "88",
                Puesto = "Docente",
                NuevoArchivo = true,
                ArchivosGenerales = new CargarArchivoDTO { NombreArchivo = "", RutaArchivo = "" }
            };
            dto.GradosAgregados.Add(new AgregarGradoDTO { IdDocente = 35, Grado = "Doctorado", Titulo = "Culturas del Preclásico", Ultimo = true });
            dto.GradosEditados.Add(new DatosGradoDTO { IdGrado = 40, IdDocente = 35, Grado = "Licenciatura", Titulo = "Arqueología.", Ultimo = false });

            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(35))
                .ReturnsAsync(true);

            _gradoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(35))
                .ReturnsAsync(new List<Grado> { new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura", Titulo = "Antropología", Ultimo = false } });

            _gradoRepositoryMock
                .Setup(r => r.ObtenerAsync(40))
                .ReturnsAsync(new Grado { IdGrado = 40, IdDocente = 35, Grado1 = "Licenciatura" });

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre del Archivo es obligatorio.", ex.Message);
        }

        //CP-25-27
        [Fact]
        public async Task EliminarDocenteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarIdAsync(-124));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Docente es inválida.", ex.Message);
        }

        //CP-25-28
        [Fact]
        public async Task EliminarDocenteInexistente()
        {
            _docenteRepositoryMock
                .Setup(r => r.ExistePorIdAsync(90))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _docenteValidator.ValidarIdAsync(90));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Docente.", ex.Message);
        }
    }
}
