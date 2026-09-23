using Moq;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class EntidadAcademicaMvcServiceTests
{
    private readonly Mock<IEntidadAcademicaMvcRepository> _repository = new();
    private readonly FixedTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 23, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Crear_rechaza_catalogos_inactivos_o_ajenos()
    {
        _repository.Setup(x => x.ReferenciasActivasAsync(It.IsAny<EntidadAcademicaMvcInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => CreateService().CrearAsync(Input()));

        Assert.Contains("activos", exception.Message);
        _repository.Verify(x => x.CrearAsync(It.IsAny<EntidadAcademicaMvcInputDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_no_permite_cambiar_clave_ni_campus()
    {
        _repository.Setup(x => x.ObtenerPorIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Entity(id: 9, campusId: 2, areaId: 3));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().ActualizarAsync(9, WithValues(Input(), campusId: 8)));

        Assert.Contains("inmutables", exception.Message);
        _repository.Verify(x => x.ActualizarAsync(It.IsAny<int>(), It.IsAny<EntidadAcademicaMvcInputDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_bloquea_cambio_de_area_si_hay_programas()
    {
        _repository.Setup(x => x.ObtenerPorIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Entity(id: 9, campusId: 2, areaId: 3));
        _repository.Setup(x => x.TieneProgramasAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().ActualizarAsync(9, WithValues(Input(), areaId: 7)));

        Assert.Contains("programas educativos", exception.Message);
        _repository.Verify(x => x.ActualizarAsync(It.IsAny<int>(), It.IsAny<EntidadAcademicaMvcInputDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_propaga_el_instante_utc_del_reloj_inyectado()
    {
        await CreateService().EliminarAsync(9);

        _repository.Verify(x => x.EliminarEnCascadaAsync(
            9, _timeProvider.GetUtcNow().UtcDateTime, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Buscar_rechaza_paginacion_fuera_de_rango()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().BuscarAsync(
            new FiltroEntidadAcademicaMvcDto(null, null, null, 1, 101)));
        _repository.Verify(x => x.BuscarAsync(It.IsAny<FiltroEntidadAcademicaMvcDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private EntidadAcademicaMvcService CreateService() => new(_repository.Object, _timeProvider);

    private static EntidadAcademicaMvcInputDto Input() => new()
    {
        Clave = "12345",
        Nombre = "Facultad de prueba",
        Calle = "Av. Universidad",
        NumeroExterior = "10A",
        Colonia = "Centro",
        CodigoPostal = "91000",
        Telefono = "2281234567",
        Extension = "123",
        CampusId = 2,
        AreaAcademicaId = 3,
        MunicipioId = 4
    };

    private static EntidadAcademicaMvcInputDto WithValues(EntidadAcademicaMvcInputDto value, int? campusId = null, int? areaId = null) => new()
    {
        Clave = value.Clave,
        Nombre = value.Nombre,
        Calle = value.Calle,
        NumeroExterior = value.NumeroExterior,
        Colonia = value.Colonia,
        CodigoPostal = value.CodigoPostal,
        Telefono = value.Telefono,
        Extension = value.Extension,
        CampusId = campusId ?? value.CampusId,
        AreaAcademicaId = areaId ?? value.AreaAcademicaId,
        MunicipioId = value.MunicipioId
    };

    private static EntidadAcademicaMvcDto Entity(int id, int campusId, int areaId) => new(
        id, "12345", "Facultad de prueba", "Av. Universidad", "10A", "Centro", "91000", "2281234567", "123",
        campusId, "Xalapa", 1, "Xalapa", areaId, "Humanidades", 1, "Xalapa");

    private sealed class FixedTimeProvider(DateTimeOffset current) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => current;
    }
}
