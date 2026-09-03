using Moq;
using SGPla.Commons;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;
using Xunit;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Archivo;

namespace SGPla.Tests.Validators
{
    public class AvisoValidatorTest
    {
        private readonly Mock<IAvisoRepository> _avisoRepositoryMock;
        private readonly AvisoValidator _avisoValidator;

        public AvisoValidatorTest()
        {
            _avisoRepositoryMock = new Mock<IAvisoRepository>();
            _avisoValidator = new AvisoValidator(_avisoRepositoryMock.Object);
        }

        // CP-05
        [Fact]
        public async Task ObtenerAvisoConIdInvalida()
        {
            const int idInvalida = -5;
            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-06
        [Fact]
        public async Task ObtenerAvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 99;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInexistente));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-08
        [Fact]
        public async Task ValidarEnviarARevisionAsync_DatosNulos_LanzaExcepcion()
        {
            RevisionDTO? revisionDTO = null;
            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarEnviarARevisionAsync(revisionDTO!));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No se enviaron datos", ex.Message);
        }

        // CP-09
        [Fact]
        public async Task ValidarEnviarARevisionAsync_IdInvalida_LanzaExcepcion()
        {
            var revisionDTO = new RevisionDTO
            {
                IdAviso = 0,
                Comentarios = "Revisión urgente."
            };

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarEnviarARevisionAsync(revisionDTO));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-10
        [Fact]
        public async Task ValidarEnviarARevisionAsync_AvisoInexistente_LanzaExcepcion()
        {
            var revisionDTO = new RevisionDTO
            {
                IdAviso = 88,
                Comentarios = "Comentarios de prueba."
            };

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(revisionDTO.IdAviso))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarEnviarARevisionAsync(revisionDTO));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-11
        [Fact]
        public async Task ValidarEnviarARevisionAsync_SinComentarios_LanzaExcepcion()
        {
            var revisionDTO = new RevisionDTO
            {
                IdAviso = 1,
                Comentarios = null!
            };

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(revisionDTO.IdAviso))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarEnviarARevisionAsync(revisionDTO));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("Los Comentarios son obligatorios.", ex.Message);
        }

        // CP-13
        [Fact]
        public async Task FirmarAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = -1;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-14
        [Fact]
        public async Task FirmarAvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 120;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInexistente));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-15
        [Fact]
        public async Task ValidarArchivoAsync_ArchivoNulo_LanzaExcepcion()
        {
            CargarArchivoDTO? archivoDTO = null;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarArchivoAsync(archivoDTO!));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No se envió el archivo", ex.Message);
        }

        // CP-16
        [Fact]
        public async Task ValidarArchivoAsync_SinNombreArchivo_LanzaExcepcion()
        {
            var archivoDTO = new CargarArchivoDTO
            {
                NombreArchivo = null!,
                RutaArchivo = "C:/uploads/temp/archivo.pdf"
            };

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarArchivoAsync(archivoDTO));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Nombre del Archivo es obligatorio", ex.Message);
        }

        // CP-17
        [Fact]
        public async Task ValidarArchivoAsync_SinRutaArchivo_LanzaExcepcion()
        {
            var archivoDTO = new CargarArchivoDTO
            {
                NombreArchivo = "AvisoFirmado.pdf",
                RutaArchivo = null!
            };

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarArchivoAsync(archivoDTO));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Ruta del Archivo es obligatoria", ex.Message);
        }

        // CP-19
        [Fact]
        public async Task ValidarPublicacionAsync_IdInvalida_LanzaExcepcion()
        {
            const int idInvalida = 0;
            const string url = "https://www.uv.mx/convocatorias/aviso-1.pdf";

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarPublicacionAsync(idInvalida, url));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-20
        [Fact]
        public async Task ValidarPublicacionAsync_AvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 75;
            const string url = "https://www.uv.mx/convocatorias/aviso-75.pdf";

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarPublicacionAsync(idInexistente, url));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-21
        [Fact]
        public async Task ValidarPublicacionAsync_UrlVacia_LanzaExcepcion()
        {
            const int idAviso = 1;
            string? url = null;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idAviso))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarPublicacionAsync(idAviso, url!));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La URL de la publicación es obligatoria", ex.Message);
        }

        // CP-23
        [Fact]
        public async Task ArchivarAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = -10;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-24
        [Fact]
        public async Task ArchivarAvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 65;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInexistente));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-26
        [Fact]
        public async Task DesarchivarAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = 0;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-27
        [Fact]
        public async Task DesarchivarAvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 65;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInexistente));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        // CP-29
        [Fact]
        public async Task VerComentariosAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = -2;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-31
        [Fact]
        public async Task VerificarEstadoAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = 0;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-33
        [Fact]
        public async Task EliminarAvisoConIdInvalida_LanzaExcepcion()
        {
            const int idInvalida = 0;

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInvalida));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La Id del Aviso es inválida.", ex.Message);
        }

        // CP-34
        [Fact]
        public async Task EliminarAvisoInexistente_LanzaExcepcion()
        {
            const int idInexistente = 40;

            _avisoRepositoryMock
                .Setup(r => r.ExistePorId(idInexistente))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _avisoValidator.ValidarIdAsync(idInexistente));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Aviso", ex.Message);
        }

        
    }
}