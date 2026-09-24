using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class PlaneaSnapshotValidatorTests
{
    private readonly PlaneaSnapshotValidator _validator = new();
    private static readonly IReadOnlyDictionary<string, int> Programaciones = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["A12B3"] = 42
    };

    [Fact]
    public void Validar_expande_dias_deduplica_y_ignora_filas_sin_sesion()
    {
        var registros = new[]
        {
            Registro("A12B3", "P123", "Docente Uno", "0800", "0859", "0900", "0959"),
            Registro("A12B3", "P123", "Docente Uno", "0800", "0859", null, null),
            Registro("A12B3", null, null, null, null, null, null)
        };

        var snapshot = _validator.Validar("202601", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Programaciones, registros);

        Assert.Equal(2, snapshot.Sesiones.Count);
        Assert.Equal(1, snapshot.DuplicadosDescartados);
        Assert.Equal(1, snapshot.RegistrosIgnorados);
        var docente = Assert.Single(snapshot.Docentes);
        Assert.Equal("P123", docente.NumeroPersonal);
        Assert.Equal(new DateOnly(2026, 1, 1), docente.FechaInicio);
        Assert.Equal(new DateOnly(2026, 6, 30), docente.FechaFin);
    }

    [Fact]
    public void Validar_cuenta_sesiones_fuera_del_periodo_y_traslapes_sin_descartar_sesiones()
    {
        var registros = new[]
        {
            Registro("A12B3", null, null, "0800", "0900", null, null, fechaInicio: "2025-12-01", edificio: "Edificio A"),
            Registro("A12B3", null, null, "0830", "0930", null, null, fechaInicio: "2025-12-01", edificio: "Edificio B")
        };

        var snapshot = _validator.Validar("202601", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Programaciones, registros);

        Assert.Equal(2, snapshot.Sesiones.Count);
        Assert.Equal(3, snapshot.Advertencias); // 2 sesiones fuera de rango y una pareja traslapada.
    }

    [Fact]
    public void Validar_rechaza_periodo_nrc_docente_y_horarios_inconsistentes()
    {
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "0800", "0859", null, null, periodo: "202602")]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("NO-EXISTE", null, null, "0800", "0859", null, null)]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", "P123", null, "0800", "0859", null, null)]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "0800", null, null, null)]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "2200", "0100", null, null)]));
    }

    [Fact]
    public void Validar_rechaza_dos_docentes_para_un_mismo_nrc()
    {
        var registros = new[]
        {
            Registro("A12B3", "P123", "Docente Uno", "0800", "0859", null, null),
            Registro("A12B3", "P456", "Docente Dos", null, null, "0900", "0959")
        };
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30), Programaciones, registros));
    }

    private static PlaneaRegistro Registro(string nrc, string? numeroPersonal, string? nombre,
        string? lunesInicio, string? lunesFin, string? martesInicio, string? martesFin,
        string periodo = "202601", string fechaInicio = "2026-01-01", string fechaFin = "2026-06-30",
        string? edificio = null) =>
        new(periodo, nrc, numeroPersonal, nombre, edificio, null, fechaInicio, fechaFin,
            [new PlaneaHorarioDia(1, lunesInicio, lunesFin), new PlaneaHorarioDia(2, martesInicio, martesFin),
             new PlaneaHorarioDia(3, null, null), new PlaneaHorarioDia(4, null, null),
             new PlaneaHorarioDia(5, null, null), new PlaneaHorarioDia(6, null, null)]);
}
