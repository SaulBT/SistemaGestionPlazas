using Moq;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Interfaces;
using SGPla.Validations.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGPla.Tests.Validators
{
    public class ProgramaEducativoValidatorTest
    {

        private readonly Mock<IProgramaEducativoRepository> _programaEducativoRepositoryMock;
        private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
        private readonly Mock<IPlanEstudiosRepository> _planEstudiosRepositoryMock;
        private readonly ProgramaEducativoValidator _programaEducativoValidator;

        public ProgramaEducativoValidatorTest()
        {
            _programaEducativoRepositoryMock = new Mock<IProgramaEducativoRepository>();
            _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
            _planEstudiosRepositoryMock = new Mock<IPlanEstudiosRepository>();
            _programaEducativoValidator = new ProgramaEducativoValidator(
                _programaEducativoRepositoryMock.Object,
                _entidadAcademicaRepositoryMock.Object,
                _planEstudiosRepositoryMock.Object
                );
        }

        //CP-05-02
        [Fact]
        public async Task CrearProgramaEducativoConDatosNulos()
        {
            var dto = new CrearProgramaEducativoDTO
            {
                IdEntidadAcademica = 0,
                Nombre = null!,
            };

            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = 1,
                IdEntidadAcademica = dto.IdEntidadAcademica,
                Nombre = dto.Nombre
            };

            var entidadAcademica = new EntidadAcademica
            {
                IdEntidadAcademica = dto.IdEntidadAcademica,
                Nombre = "Facultad de Informática"
            };

           

            _programaEducativoRepositoryMock
                .Setup(repository => repository.ExisteAsync(programaEducativo))
                .ReturnsAsync(programaEducativo);

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El nombre del programa educativo es obligatorio.", ex.Message);
        }

        //CP-05-03
        [Fact]
        public async Task CrearProgramaEducativoConIdEntidadAcademicaInvalida()
        {
            var dto = new CrearProgramaEducativoDTO
            {
                IdEntidadAcademica = -12,
                Nombre = "Licenciatura en Informática"
            };
            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarCreacionAsync(dto)); 
           

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("La entidad académica es obligatoria.", ex.Message);
        }

        //CP-05-04
        [Fact]
        public async Task CrearProgramaEducativoConAreaAcademicaInexistente()
        {
            var dto = new CrearProgramaEducativoDTO
            {
                IdEntidadAcademica = 1,
                Nombre = "Licenciatura en Informática"
            };

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarCreacionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("La entidad académica no existe.", ex.Message);
        }

     


        //CP-05-08
        [Fact]
        public async Task ObtenerProgramaEducativoConIdInvalido()
        {
            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarObtenerPorIdAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El ID del programa educativo no es válido.", ex.Message);
        }

        //CP-05-10
        [Fact]
        public async Task EditarProgramaEducativoConValoresNulos()
        {
            var dto = new EditarProgramaEducativoDTO
            {
                IdEntidadAcademica = 12,
                Nombre = null!,
            };

            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = 1,
                IdEntidadAcademica = dto.IdEntidadAcademica,
                Nombre = dto.Nombre
            };

            _programaEducativoRepositoryMock
                .Setup(repository => repository.ExisteAsync(programaEducativo))
                .ReturnsAsync(programaEducativo);

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El nombre del programa educativo es obligatorio.", ex.Message);
        }

        //CP-05-11
        [Fact]
        public async Task EditarProgramaEducativoConIdEntidadAcademicaInvalida()
        {
            var dto = new EditarProgramaEducativoDTO
            {
                IdProgramaEducativo = 1,
                Nombre = "Licenciatura en Informática",
                IdEntidadAcademica = -12
            };

            dto.IdEntidadAcademica = 0;

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("La entidad académica es obligatoria.", ex.Message);
        }



        //CP-05-12
        [Fact]
        public async Task EditarProgramaEducativoConNombreInvalido()
        {
            var dto = new EditarProgramaEducativoDTO
            {
                IdProgramaEducativo = 1,
                IdEntidadAcademica = 12,
                Nombre = "Licenciatura-en-informática"
            };

            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = dto.IdProgramaEducativo,
                IdEntidadAcademica = dto.IdEntidadAcademica,
                Nombre = dto.Nombre
            };

            _programaEducativoRepositoryMock
                .Setup(repository => repository.ExisteAsync(programaEducativo))
                .ReturnsAsync(programaEducativo);

           

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El nombre del programa educativo solo puede contener letras, números y espacios.", ex.Message);
        }


        //CP-05-13
        [Fact]
        public async Task EditarProgramaEducativoInexistente()
        {
            var dto = new EditarProgramaEducativoDTO
            {
                IdProgramaEducativo = 1,
                Nombre = "Licenciatura en Informática",
                IdEntidadAcademica = 12
            };  

            var programaEducativo = new ProgramaEducativo
            {
                IdProgramaEducativo = dto.IdProgramaEducativo,
                IdEntidadAcademica = dto.IdEntidadAcademica,
                Nombre = dto.Nombre
            };


            _programaEducativoRepositoryMock
                .Setup(repository => repository.ExisteAsync(programaEducativo))
                .ReturnsAsync((ProgramaEducativo)null);

            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarEdicionAsync(dto));

            Assert.NotNull(ex);
            Assert.IsType<ValidacionExcepction>(ex);
            Assert.Contains("El programa educativo no existe.", ex.Message);
        }



        //CP-05-15
        [Fact]
        public async Task EliminarProgramaEducativoConIdInvalido()
        {
            var ex = await Record.ExceptionAsync(() => _programaEducativoValidator.ValidarEliminarAsync(0));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains("El ID del programa educativo no es válido.", ex.Message);
        }

      

    }
}
