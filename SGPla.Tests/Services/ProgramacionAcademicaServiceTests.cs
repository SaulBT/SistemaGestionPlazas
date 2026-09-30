using Moq;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;

namespace SGPla.Tests.Services;

public class ProgramacionAcademicaServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImportaSinExigirDocenteYDescartaFilasSinNrc(bool registrado)
    {
        var validator = new Mock<IProgramacionAcademicaValidator>();
        var repository = new Mock<IProgramacionAcademicaRepository>();
        var docentes = new Mock<IDocenteRepository>();
        var experiencias = new Mock<IExperienciaEducativaRepository>();
        var programas = new Mock<IProgramaEducativoRepository>();
        docentes.Setup(x => x.ObtenerIdsPorNumeroPersonalAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(registrado ? new Dictionary<string, int> { ["123"] = 7 } : new());
        experiencias.Setup(x => x.ObtenerIdsPorNombreAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, int> { ["CONTABILIDAD"] = 10, ["SOLO CARGA"] = 11 });
        programas.Setup(x => x.ObtenerIdsProgramasAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, int> { ["14140"] = 2 });
        List<Oferta>? guardadas = null;
        List<CargaAcademica>? cargasGuardadas = null;
        repository.Setup(x => x.GuardarOfertasYCargas(It.IsAny<List<Oferta>>(), It.IsAny<List<CargaAcademica>>()))
            .Callback<List<Oferta>, List<CargaAcademica>>((o, c) => { guardadas = o; cargasGuardadas = c; })
            .Returns(Task.CompletedTask);
        var service = new ProgramacionAcademicaService(validator.Object, repository.Object,
            docentes.Object, experiencias.Object, Mock.Of<IEntidadAcademicaRepository>(),
            programas.Object, Mock.Of<IArticuloRepository>());

        await service.GuardarOfertasyCargasAsync(
            [new() { NRC = "12345", Programa = "14140-CONTADURIA", ExperienciaEducativa = "CONTABILIDAD",
                NP = "123", NombreDocente = "DOCENTE DEL ARCHIVO", IdPeriodo = 5, Incluida = true },
             new() { NRC = "  ", Programa = "INVALIDO", ExperienciaEducativa = "IGNORAR" }],
            [new() { Nrc = "23456", Programa = "14140-CONTADURIA", ExperienciaEducativa = "SOLO CARGA",
                NumeroPersonal = "123", NombreDocente = "DOCENTE DEL ARCHIVO" },
             new() { Nrc = null, Programa = "INVALIDO", ExperienciaEducativa = "IGNORAR" }]);

        var oferta = Assert.Single(guardadas!);
        Assert.Equal(registrado ? 7 : (int?)null, oferta.IdDocente);
        Assert.Equal("123", oferta.NumeroPersonalImportado);
        Assert.Equal("DOCENTE DEL ARCHIVO", oferta.NombreDocenteImportado);
        Assert.False(oferta.Incluida);
        var carga = Assert.Single(cargasGuardadas!);
        Assert.Equal(registrado ? 7 : (int?)null, carga.IdDocente);
        Assert.Equal("DOCENTE DEL ARCHIVO", carga.NombreDocenteImportado);
        Assert.Equal(5, carga.IdPeriodo);
        Assert.Equal(11, carga.IdExperienciaEducativa);
        validator.Verify(x => x.ValidarDocentes(It.IsAny<List<OfertaDTO>>()), Times.Never);
        validator.Verify(x => x.ValidarExperiencias(It.Is<List<OfertaDTO>>(o => o.Count == 1)), Times.Once);
        experiencias.Verify(x => x.ObtenerIdsPorNombreAsync(It.Is<List<string>>(n => n.Contains("SOLO CARGA") && !n.Contains("IGNORAR"))), Times.Once);
    }

    [Theory]
    [InlineData(null, "DOCENTE", true)]
    [InlineData("123", null, true)]
    [InlineData(" ", " ", false)]
    public void ClasificaPorDatosDelArchivoSinNecesitarRegistro(string? np, string? nombre, bool convocada)
    {
        var dto = new OfertaDTO { NP = np, NombreDocente = nombre, Incluida = true };
        Assert.Equal(convocada, dto.TieneDocente);
        Assert.Equal(!convocada, OfertaMapper.ToModel(dto).Incluida);
    }
}
