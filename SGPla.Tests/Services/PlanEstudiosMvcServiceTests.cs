using Moq;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class PlanEstudiosMvcServiceTests
{
    private readonly Mock<IPlanEstudiosMvcRepository> _repository = new();
    private readonly PlanEstudiosMvcService _service;

    public PlanEstudiosMvcServiceTests() => _service = new PlanEstudiosMvcService(_repository.Object, TimeProvider.System);

    [Fact]
    public async Task Crear_plan_rechaza_programa_inactivo()
    {
        _repository.Setup(x => x.ProgramaActivoAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CrearAsync(new GuardarPlanEstudiosMvcDto
        {
            ProgramaEducativoId = 99, Codigo = "PLAN-01"
        }));

        _repository.Verify(x => x.CrearAsync(It.IsAny<GuardarPlanEstudiosMvcDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Crear_experiencia_rechaza_area_de_formacion_inactiva()
    {
        _repository.Setup(x => x.ObtenerAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(Plan());
        _repository.Setup(x => x.AreaFormacionActivaAsync(80, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CrearExperienciaAsync(Experiencia(planId: 4, areaId: 80)));

        _repository.Verify(x => x.CrearExperienciaAsync(It.IsAny<GuardarExperienciaEducativaMvcDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reemplaza_ids_del_formulario_y_persiste_en_un_lote()
    {
        _repository.Setup(x => x.ObtenerAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(Plan());
        _repository.Setup(x => x.AreaFormacionActivaAsync(80, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(x => x.CrearExperienciasAsync(It.IsAny<IReadOnlyList<GuardarExperienciaEducativaMvcDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var resultado = await _service.ImportarExperienciasAsync(4, 80,
        [
            Experiencia(planId: 999, areaId: 999),
            Experiencia(planId: 888, areaId: 888, materia: "MAT-102", curso: "02")
        ]);

        Assert.Equal(2, resultado);
        _repository.Verify(x => x.CrearExperienciasAsync(It.Is<IReadOnlyList<GuardarExperienciaEducativaMvcDto>>(filas =>
            filas.Count == 2 && filas.All(f => f.PlanEstudiosId == 4 && f.AreaFormacionId == 80)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_rechaza_claves_duplicadas_en_el_mismo_archivo()
    {
        _repository.Setup(x => x.ObtenerAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(Plan());
        _repository.Setup(x => x.AreaFormacionActivaAsync(80, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => _service.ImportarExperienciasAsync(4, 80,
        [Experiencia(4, 80), Experiencia(4, 80)]));

        Assert.Contains("duplicados", error.Message);
        _repository.Verify(x => x.CrearExperienciasAsync(It.IsAny<IReadOnlyList<GuardarExperienciaEducativaMvcDto>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PlanEstudiosMvcDetalle Plan() => new(4, "PLAN-01", 5, "Programa", 6, "Entidad", "Área", "Región", "Sistema", "Nivel");

    private static GuardarExperienciaEducativaMvcDto Experiencia(int planId, int areaId, string materia = "MAT-101", string curso = "01") => new()
    {
        PlanEstudiosId = planId,
        AreaFormacionId = areaId,
        Nombre = "Matemáticas",
        MateriaEe = materia,
        CursoEe = curso,
        HorasTeoricas = 3,
        HorasPracticas = 2,
        Creditos = 5
    };
}
