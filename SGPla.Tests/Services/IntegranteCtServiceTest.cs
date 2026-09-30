using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.IntegranteCt;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;

namespace SGPla.Tests.Services
{
    public class IntegranteCtServiceTest
    {
        private readonly IntegranteCtService _integranteCtService;
        private readonly Mock<IIntegranteCtRepository> _integranteCtRepositoryMock;
        private readonly Mock<IIntegranteCtValidator> _integranteCtValidatorMock;

        public IntegranteCtServiceTest()
        {
            _integranteCtRepositoryMock = new Mock<IIntegranteCtRepository>();
            _integranteCtValidatorMock = new Mock<IIntegranteCtValidator>();
            _integranteCtService = new IntegranteCtService(
                _integranteCtRepositoryMock.Object,
                _integranteCtValidatorMock.Object);
        }

        //CP-27-01
        [Fact]
        public async Task RegistrarIntegrante()
        {
            var dto = new RegistrarIntegranteCtDto
            {
                Cargo = "Secretario de Finanzas",
                Nombre = "Juan Davidson Carmona",
                Grado = "Dr.",
                IdEntidadAcademica = 1
            };

            _integranteCtValidatorMock
                .Setup(v => v.ValidarRegistroAsync(dto))
                .Returns(Task.CompletedTask);

            _integranteCtRepositoryMock
                .Setup(r => r.RegistrarAsync(It.IsAny<IntegranteCt>()))
                .Returns(Task.CompletedTask);

            await _integranteCtService.RegistrarIntegranteAsync(dto);

            _integranteCtRepositoryMock.Verify(r => r.RegistrarAsync(It.Is<IntegranteCt>(i =>
                i.Cargo == dto.Cargo &&
                i.Nombre == $"{dto.Grado} {dto.Nombre}" &&
                i.IdEntidadAcademica == dto.IdEntidadAcademica)), Times.Once);
        }

        //CP-27-06
        [Fact]
        public async Task EditarIntegrante()
        {
            var dto = new DatosIntegranteCtDto
            {
                IdIntegranteCt = 10,
                Cargo = "Super intendente",
                Nombre = "Tadeo Elidorio Cháriz",
                Grado = "Lic.",
                IdEntidadAcademica = 31
            };

            _integranteCtValidatorMock
                .Setup(v => v.ValidarEdicionAsync(dto))
                .Returns(Task.CompletedTask);

            _integranteCtRepositoryMock
                .Setup(r => r.EditarAsync(It.IsAny<IntegranteCt>()))
                .Returns(Task.CompletedTask);

            await _integranteCtService.EditarIntegranteAsync(dto);

            _integranteCtRepositoryMock.Verify(r => r.EditarAsync(It.Is<IntegranteCt>(i =>
                i.Cargo == dto.Cargo &&
                i.Nombre == $"{dto.Grado} {dto.Nombre}")), Times.Once);
        }

        //CP-27-13
        [Fact]
        public async Task ObtenerTodosLosIntegrantes()
        {
            int idEntidadAcademica = 109;

            var integrantes = new List<IntegranteCt>
            {
                new IntegranteCt { IdIntegranteCt = 1, Cargo = "Ejecutivo General",   Nombre = "Mtro. Guillermo Oropesa Mercado",    IdEntidadAcademica = 109 },
                new IntegranteCt { IdIntegranteCt = 2, Cargo = "Directora de Facultad", Nombre = "Dra. Mariana Sofía Domínguez Ibarra", IdEntidadAcademica = 109 },
                new IntegranteCt { IdIntegranteCt = 3, Cargo = "Contralor",           Nombre = "Lic. Juan Manuel Alcalde",           IdEntidadAcademica = 109 }
            };

            _integranteCtValidatorMock
                .Setup(v => v.ValidarIdEntidadAsync(idEntidadAcademica))
                .Returns(Task.CompletedTask);

            _integranteCtRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(idEntidadAcademica))
                .ReturnsAsync(integrantes);

            var resultado = await _integranteCtService.ObtenerTodosIntegrantesAsync(idEntidadAcademica);

            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Count);

            Assert.Equal(1, resultado[0].IdIntegranteCt);
            Assert.Equal("Ejecutivo General", resultado[0].Cargo);
            Assert.Equal("Mtro.", resultado[0].Grado);
            Assert.Equal(" Guillermo Oropesa Mercado", resultado[0].Nombre);

            Assert.Equal(2, resultado[1].IdIntegranteCt);
            Assert.Equal("Directora de Facultad", resultado[1].Cargo);
            Assert.Equal("Dra.", resultado[1].Grado);
            Assert.Equal(" Mariana Sofía Domínguez Ibarra", resultado[1].Nombre);

            Assert.Equal(3, resultado[2].IdIntegranteCt);
            Assert.Equal("Contralor", resultado[2].Cargo);
            Assert.Equal("Lic.", resultado[2].Grado);
            Assert.Equal(" Juan Manuel Alcalde", resultado[2].Nombre);
        }

        //CP-27-14
        [Fact]
        public async Task ObtenerIntegrantePorId()
        {
            int id = 19;
            var integrante = new IntegranteCt
            {
                IdIntegranteCt = 19,
                Cargo = "Supervisor",
                Nombre = "Mtro. Enrique Juárez Omar",
                IdEntidadAcademica = 8
            };

            _integranteCtValidatorMock
                .Setup(v => v.ValidarIdIntegranteAsync(id))
                .Returns(Task.CompletedTask);

            _integranteCtRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(id))
                .ReturnsAsync(integrante);

            var resultado = await _integranteCtService.ObtenerIntegrantePorIdAsync(id);

            Assert.NotNull(resultado);
            Assert.Equal(19, resultado.IdIntegranteCt);
            Assert.Equal("Supervisor", resultado.Cargo);
            Assert.Equal("Mtro.", resultado.Grado);
            Assert.Equal(8, resultado.IdEntidadAcademica);
        }

        //CP-27-17
        [Fact]
        public async Task EliminarIntegrantePorId()
        {
            int id = 19;
            var integrante = new IntegranteCt { IdIntegranteCt = 19 };

            _integranteCtValidatorMock
                .Setup(v => v.ValidarIdIntegranteAsync(id))
                .Returns(Task.CompletedTask);

            _integranteCtRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(id))
                .ReturnsAsync(integrante);

            _integranteCtRepositoryMock
                .Setup(r => r.EliminarAsync(integrante))
                .Returns(Task.CompletedTask);

            await _integranteCtService.EliminarIntegranteAsync(id);

            _integranteCtRepositoryMock.Verify(r => r.EliminarAsync(integrante), Times.Once);
        }
    }
}
