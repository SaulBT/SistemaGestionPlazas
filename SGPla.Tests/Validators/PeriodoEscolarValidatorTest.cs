using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;

namespace SGPla.Tests.Validators
{
    public class PeriodoEscolarValidatorTest
    {
        private readonly Mock<IPeriodoEscolarRepository> _periodoEscolarRepositoryMock;
        private readonly PeriodoEscolarValidator _periodoEscolarValidator;

        public PeriodoEscolarValidatorTest()
        {
            _periodoEscolarRepositoryMock = new Mock<IPeriodoEscolarRepository>();
            _periodoEscolarValidator = new PeriodoEscolarValidator(_periodoEscolarRepositoryMock.Object);
        }

        //CP-07-02
        [Fact]
        public async Task CrearPeriodoEscolarConAnioInvalido()
        {
            var dto = new CrearPeriodoEscolarDTO { Anio = "0", Periodo = "Febrero-Julio" };

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El año debe ser un número positivo.", ex.Message);
        }

        //CP-07-03
        [Fact]
        public async Task CrearPeriodoEscolarConPeriodoVacio()
        {
            var dto = new CrearPeriodoEscolarDTO { Anio = "2024", Periodo = "" };

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo no puede estar vacío.", ex.Message);
        }

        //CP-07-04
        [Fact]
        public async Task CrearPeriodoEscolarConPeriodoInvalido()
        {
            var dto = new CrearPeriodoEscolarDTO { Anio = "2024", Periodo = "Enero-Junio" };

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo no es un periodo válido.", ex.Message);
        }

        //CP-07-05
        [Fact]
        public async Task CrearPeriodoEscolarDuplicado()
        {
            var dto = new CrearPeriodoEscolarDTO { Anio = "2024", Periodo = "Febrero-Julio" };

            _periodoEscolarRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<Periodo>()))
                .ReturnsAsync(new Periodo { IdPeriodo = 1 });

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("Ya existe un periodo escolar con los mismos datos.", ex.Message);
        }

        //CP-07-07
        [Fact]
        public async Task EditarPeriodoEscolarConAnioInvalido()
        {
            var dto = new EditarPeriodoEscolarDTO { IdPeriodoEscolar = 1, Anio = "-1", Periodo = "Agosto-Enero" };

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El año debe ser un número positivo.", ex.Message);
        }

        //CP-07-07
        [Fact]
        public async Task EditarPeriodoEscolarConPeriodoNulo()
        {
            var dto = new EditarPeriodoEscolarDTO { IdPeriodoEscolar = 1, Anio = "2024", Periodo = null! };

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo no puede estar vacío.", ex.Message);
        }

        //CP-07-08
        [Fact]
        public async Task EditarPeriodoEscolarInexistente()
        {
            var dto = new EditarPeriodoEscolarDTO { IdPeriodoEscolar = 99, Anio = "2024", Periodo = "Febrero-Julio" };

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(dto.IdPeriodoEscolar))
                .ReturnsAsync((Periodo)null!);

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo escolar no existe.", ex.Message);
        }

        //CP-07-09
        [Fact]
        public async Task EditarPeriodoEscolarConDatosDuplicados()
        {
            var dto = new EditarPeriodoEscolarDTO { IdPeriodoEscolar = 1, Anio = "2024", Periodo = "Agosto-Enero" };

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(dto.IdPeriodoEscolar))
                .ReturnsAsync(new Periodo { IdPeriodo = 1 });

            _periodoEscolarRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<Periodo>()))
                .ReturnsAsync(new Periodo { IdPeriodo = 2 });

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("Ya existe un periodo escolar con los mismos datos.", ex.Message);
        }

        //CP-07-11
        [Fact]
        public async Task ObtenerPeriodoEscolarConIdInvalido()
        {
            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarObtenerPorIdAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El ID del periodo escolar no es válido.", ex.Message);
        }


        [Fact]
        public async Task EliminarPeriodoEscolarConIdInvalido()
        {
            _periodoEscolarRepositoryMock
                .Setup(r => r.TieneRelacionesAsync(0))
                .ReturnsAsync(false);

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEliminarAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El ID del periodo escolar no es válido.", ex.Message);
        }

        //CP-07-16
        [Fact]
        public async Task EliminarPeriodoEscolarInexistente()
        {
            _periodoEscolarRepositoryMock
                .Setup(r => r.TieneRelacionesAsync(99))
                .ReturnsAsync(false);

            _periodoEscolarRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(99))
                .ReturnsAsync((Periodo)null!);

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEliminarAsync(99));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo escolar no existe.", ex.Message);
        }

        //CP-07-17
        [Fact]
        public async Task EliminarPeriodoEscolarConRelaciones()
        {
            _periodoEscolarRepositoryMock
                .Setup(r => r.TieneRelacionesAsync(1))
                .ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _periodoEscolarValidator.ValidarEliminarAsync(1));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El periodo escolar ya está relacionado a un aviso u oferta.", ex.Message);
        }
    }
}