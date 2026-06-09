using Moq;
using SGPla.Commons;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

namespace SGPla.Tests.Validators
{
    public class IntegranteCtValidatorTest
    {
        private readonly Mock<IIntegranteCtRepository> _integranteCtRepositoryMock;
        private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
        private readonly IntegranteCtValidator _integranteCtValidator;

        public IntegranteCtValidatorTest()
        {
            _integranteCtRepositoryMock = new Mock<IIntegranteCtRepository>();
            _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
            _integranteCtValidator = new IntegranteCtValidator(
                _integranteCtRepositoryMock.Object,
                _entidadAcademicaRepositoryMock.Object);
        }

        //CP-27-02
        [Fact]
        public async Task RegistrarIntegranteConCamposNulos()
        {
            var dto = new RegistrarIntegranteCtDto
            {
                Cargo = "",
                Nombre = "",
                Grado = "",
                IdEntidadAcademica = 1
            };

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Cargo es obligatorio.", ex.Message);
        }

        //CP-27-03
        [Fact]
        public async Task RegistrarIntegranteConIdEntidadAcademicaInvalida()
        {
            var dto = new RegistrarIntegranteCtDto
            {
                Cargo = "Secretario de Finanzas",
                Nombre = "Juan Davidson Carmona",
                Grado = "Dr.",
                IdEntidadAcademica = -1
            };

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La IdEntidadAcademica es inválida.", ex.Message);
        }

        //CP-27-04
        [Fact]
        public async Task RegistrarIntegranteConEntidadAcademicaInexistente()
        {
            var dto = new RegistrarIntegranteCtDto
            {
                Cargo = "Secretario de Finanzas",
                Nombre = "Juan Davidson Carmona",
                Grado = "Dr.",
                IdEntidadAcademica = 25
            };

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe esa Entidad Academica.", ex.Message);
        }

        //CP-27-05
        [Fact]
        public async Task RegistrarIntegranteConNombreRepetido()
        {
            var dto = new RegistrarIntegranteCtDto
            {
                Cargo = "Tesorera",
                Nombre = "Adriana Cruz",
                Grado = "Mtra.",
                IdEntidadAcademica = 2
            };

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorNombre(dto.Nombre, dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarRegistroAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("Ya existe ese Integrante", ex.Message);
        }

        //CP-27-07
        [Fact]
        public async Task EditarIntegranteConCamposNulos()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 10,
                Cargo = "",
                Nombre = "",
                Grado = "",
                IdEntidadAcademica = 31
            };

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdIntegranteCt))
                .ReturnsAsync(true);

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El Cargo es obligatorio.", ex.Message);
        }

        //CP-27-08
        [Fact]
        public async Task EditarIntegranteConIdEntidadAcademicaInvalida()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 10,
                Cargo = "Super intendente",
                Nombre = "Tadeo Elidorio Cháriz",
                Grado = "Lic.",
                IdEntidadAcademica = 0
            };

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdIntegranteCt))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La IdEntidadAcademica es inválida.", ex.Message);
        }

        //CP-27-09
        [Fact]
        public async Task EditarIntegranteConEntidadAcademicaInexistente()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 10,
                Cargo = "Super intendente",
                Nombre = "Tadeo Elidorio Cháriz",
                Grado = "Lic.",
                IdEntidadAcademica = 12
            };

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdIntegranteCt))
                .ReturnsAsync(true);

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe esa Entidad Academica.", ex.Message);
        }

        //CP-27-10
        [Fact]
        public async Task EditarIntegranteConNombreRepetido()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 14,
                Cargo = "Secretaria General",
                Nombre = "Justina Abasolo",
                Grado = "Dra.",
                IdEntidadAcademica = 55
            };

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdIntegranteCt))
                .ReturnsAsync(true);

            _entidadAcademicaRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorNombre(dto.Nombre, dto.IdEntidadAcademica))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("Ya existe ese Integrante", ex.Message);
        }

        //CP-27-11
        [Fact]
        public async Task EditarIntegranteConIdIntegranteCtInvalida()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = -4,
                Cargo = "Super intendente",
                Nombre = "Tadeo Elidorio Cháriz",
                Grado = "Lic.",
                IdEntidadAcademica = 31
            };

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La IdIntegranteCt es inválida.", ex.Message);
        }

        //CP-27-12
        [Fact]
        public async Task EditarIntegranteInexistente()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 77,
                Cargo = "Super intendente",
                Nombre = "Tadeo Elidorio Cháriz",
                Grado = "Lic.",
                IdEntidadAcademica = 31
            };

            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(dto.IdIntegranteCt))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Integrante.", ex.Message);
        }

        //CP-27-15
        [Fact]
        public async Task ObtenerIntegranteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarIdIntegranteAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La IdIntegranteCt es inválida.", ex.Message);
        }

        //CP-27-16
        [Fact]
        public async Task ObtenerIntegranteInexistente()
        {
            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(500))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarIdIntegranteAsync(500));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Integrante.", ex.Message);
        }

        //CP-27-18
        [Fact]
        public async Task EliminarIntegranteConIdInvalida()
        {
            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarIdIntegranteAsync(-10));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("La IdIntegranteCt es inválida.", ex.Message);
        }

        //CP-27-19
        [Fact]
        public async Task EliminarIntegranteInexistente()
        {
            _integranteCtRepositoryMock
                .Setup(r => r.ExistePorIdAsync(24))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _integranteCtValidator.ValidarIdIntegranteAsync(24));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("No existe ese Integrante.", ex.Message);
        }
    }
}
