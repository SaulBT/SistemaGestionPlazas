using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.ProgramaEducativo;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

public class ProgramaEducativoServiceTest
{
    private readonly Mock<IProgramaEducativoRepository> _programaEducativoRepositoryMock;
    private readonly Mock<IProgramaEducativoValidator> _programaEducativoValidatorMock;
    private readonly Mock<IEntidadAcademicaRepository> _entidadAcademicaRepositoryMock;
    private readonly Mock<IAreaAcademicaRepository> _areaAcademicaRepositoryMock;
    private readonly ProgramaEducativoService _programaEducativoService;

    public ProgramaEducativoServiceTest()
    {
        _programaEducativoRepositoryMock = new Mock<IProgramaEducativoRepository>();
        _entidadAcademicaRepositoryMock = new Mock<IEntidadAcademicaRepository>();
        _areaAcademicaRepositoryMock = new Mock<IAreaAcademicaRepository>();
        _programaEducativoValidatorMock = new Mock<IProgramaEducativoValidator>();
        _programaEducativoService = new ProgramaEducativoService(
            _programaEducativoRepositoryMock.Object,
            _programaEducativoValidatorMock.Object,
            _entidadAcademicaRepositoryMock.Object,
            _areaAcademicaRepositoryMock.Object
        );
    }

    //CP-05-01
    [Fact]
    public async Task CrearProgramaEducativo()
    {
        var dto = new CrearProgramaEducativoDTO
        {
            Nombre = "Licenciatura en Ingeniería de Software",
            IdEntidadAcademica = 12
        };

        ProgramaEducativo? programaEducativo = null;

        _programaEducativoValidatorMock
            .Setup(v => v.ValidarCreacionAsync(dto))
            .Returns(Task.CompletedTask);

        _programaEducativoRepositoryMock
            .Setup(r => r.CrearAsync(It.IsAny<ProgramaEducativo>()))
            .Callback<ProgramaEducativo>(p => programaEducativo = p)
            .ReturnsAsync(new ProgramaEducativo
            {
                IdProgramaEducativo = 7,
                Nombre = dto.Nombre,
                IdEntidadAcademica = dto.IdEntidadAcademica,

            });

        var programaEducativoCreado = await _programaEducativoService.CrearAsync(dto);

        Assert.Equal(7, programaEducativoCreado.IdProgramaEducativo);
        Assert.Equal(dto.Nombre, programaEducativo!.Nombre);
        Assert.Equal(dto.IdEntidadAcademica, programaEducativo.IdEntidadAcademica);
        _programaEducativoValidatorMock.Verify(v => v.ValidarCreacionAsync(dto), Times.Once);
        _programaEducativoRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<ProgramaEducativo>()), Times.Once);
    }

    //CP-05-05

    [Fact]
    public async Task ObtenerListaDeProgramasEducativos()
    {
        var filtro = new BuscarProgramaEducativoDTO
        {
            Region = null,
            IdAreaAcademica = null,
            Nombre = null,
            IdEntidadAcademica = null
        };

        var programasEducativos = new List<ProgramaEducativo>
    {
        new ProgramaEducativo
        {
            IdProgramaEducativo = 7,
            Nombre = "Licenciatura en Ingeniería de Software",
            IdEntidadAcademica = 1,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                Nombre = "11304-Facultad de Estadística e Informática",
                IdEntidadAcademica = 1,
                Region = "1-Xalapa",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Económico-Administrativa"
                }
            }
        },
        new ProgramaEducativo
        {
            IdProgramaEducativo = 8,
            Nombre = "Ingeniería en Sistemas Computacionales",
            IdEntidadAcademica = 1,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                Nombre = "21052-Facultad de Ingeniería Mecánica y Ciencias Navales",
                IdEntidadAcademica = 1,
                Region = "2-Veracruz",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Técnica"
                }
            }
        }
    };

        _programaEducativoRepositoryMock
            .Setup(r => r.ObtenerPorFiltroAsync(filtro))
            .ReturnsAsync(programasEducativos);

        var resultado = await _programaEducativoService.BuscarPorFiltroAsync(filtro);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Equal("Licenciatura en Ingeniería de Software", resultado[0].Nombre);
        Assert.Equal("Ingeniería en Sistemas Computacionales", resultado[1].Nombre);

        _programaEducativoRepositoryMock.Verify(
            r => r.ObtenerPorFiltroAsync(filtro),
            Times.Once
        );
    }

    //CP-05-06
    [Fact]
    public async Task ObtenerListaDeProgramasEducativosConFiltro()
    {
        var filtro = new BuscarProgramaEducativoDTO
        {
            Region = "1-Xalapa",
            IdAreaAcademica = 1,
            Nombre = null,
            IdEntidadAcademica = null
        };

        var programasEducativos = new List<ProgramaEducativo>
    {
        new ProgramaEducativo
        {
            IdProgramaEducativo = 7,
            Nombre = "Licenciatura en Ingeniería de Software",
            IdEntidadAcademica = 1,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                Nombre = "11304-Facultad de Estadística e Informática",
                IdEntidadAcademica = 1,
                Region = "1-Xalapa",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Económico-Administrativa"
                }
            }
        },
        new ProgramaEducativo
        {
            IdProgramaEducativo = 8,
            Nombre = "Ingeniería en Sistemas Computacionales",
            IdEntidadAcademica =1,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                Nombre = "21052-Facultad de Ingeniería Mecánica y Ciencias Navales",
                IdEntidadAcademica = 1,
                Region =  "1-Xalapa",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Técnica"
                }
            }
        }
    };

        _programaEducativoRepositoryMock
            .Setup(r => r.ObtenerPorFiltroAsync(filtro))
            .ReturnsAsync(programasEducativos);

        var resultado = await _programaEducativoService.BuscarPorFiltroAsync(filtro);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);

        Assert.Equal("Licenciatura en Ingeniería de Software", resultado[0].Nombre);
        Assert.Equal("Ingeniería en Sistemas Computacionales", resultado[1].Nombre);

        _programaEducativoRepositoryMock.Verify(
            r => r.ObtenerPorFiltroAsync(filtro),
            Times.Once
        );
    }

    //CP-05-07
    [Fact]
    public async Task ObtenerProgramaEducativo()
    {
        var programaEducativo = new ProgramaEducativo
        {
            IdProgramaEducativo = 8,
            Nombre = "Ingeniería en Sistemas Computacionales",
            IdEntidadAcademica = 1,
            IdEntidadAcademicaNavigation = new EntidadAcademica
            {
                Nombre = "21052-Facultad de Ingeniería Mecánica y Ciencias Navales",
                IdEntidadAcademica = 1,
                Region = "2-Veracruz",
                IdAreaAcademicaNavigation = new AreaAcademica
                {
                    Nombre = "Técnica"
                }
            }
        };

        _programaEducativoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(8))
            .ReturnsAsync(programaEducativo);

        var dto = await _programaEducativoService.ObtenerPorIdAsync(8);
        Assert.Equal(8, dto.IdProgramaEducativo);
        Assert.Equal(1, dto.IdEntidadAcademica);


        _programaEducativoRepositoryMock.Verify(repository => repository.ObtenerPorIdAsync(8), Times.Once);
    }

    //CP-05-09
    [Fact]
    public async Task EditarProgramaEducativo()
    {
        var dto = new EditarProgramaEducativoDTO
        {
            IdProgramaEducativo = 1,
            Nombre = "Licenciatura en Ingeniería de Software",
            IdEntidadAcademica = 12
        };

        ProgramaEducativo? programaEducativoActualizado = null;

        _programaEducativoValidatorMock
            .Setup(validator => validator.ValidarEdicionAsync(dto))
            .Returns(Task.CompletedTask);

        _programaEducativoRepositoryMock
            .Setup(repository => repository.ActualizarAsync(It.IsAny<ProgramaEducativo>()))
            .Callback<ProgramaEducativo>(programa => programaEducativoActualizado = programa)
             .ReturnsAsync(new ProgramaEducativo
             {
                 IdProgramaEducativo = 7,
                 Nombre = dto.Nombre,
                 IdEntidadAcademica = dto.IdEntidadAcademica,

             });

        await _programaEducativoService.EditarAsync(dto);

        Assert.NotNull(programaEducativoActualizado);
        Assert.Equal(dto.IdEntidadAcademica, programaEducativoActualizado!.IdEntidadAcademica);
        Assert.Equal(dto.Nombre, programaEducativoActualizado.Nombre);



        _programaEducativoValidatorMock.Verify(validator => validator.ValidarEdicionAsync(dto), Times.Once);
        _programaEducativoRepositoryMock.Verify(repository => repository.ActualizarAsync(It.IsAny<ProgramaEducativo>()), Times.Once);
    }

    //CP-05-14
    [Fact]
    public async Task EliminarProgramaEducativo()
    {
        var programaEducativo = new ProgramaEducativo
        {
            IdProgramaEducativo = 12,
            IdEntidadAcademica = 1,
            Nombre = "11304-Facultad de Estadística e Informática"
        };


        _programaEducativoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(12))
            .ReturnsAsync(programaEducativo);

        _programaEducativoRepositoryMock
            .Setup(r => r.EliminarAsync(12))
            .ReturnsAsync(true);
        _programaEducativoValidatorMock
          .Setup(validator => validator.ValidarEliminarAsync(12))
          .Returns(Task.CompletedTask);


        var resultado = await _programaEducativoService.EliminarAsync(12);

        Assert.True(resultado);
        _programaEducativoValidatorMock.Verify(validator => validator.ValidarEliminarAsync(12), Times.Once);
        _programaEducativoRepositoryMock.Verify(repository => repository.EliminarAsync(programaEducativo.IdProgramaEducativo), Times.Once);

    }
}

