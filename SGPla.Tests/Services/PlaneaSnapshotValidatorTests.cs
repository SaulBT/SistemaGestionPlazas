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
    public void Validar_ignora_fila_sin_sesion_aunque_no_tenga_fechas()
    {
        var fila = Registro("A12B3", "P123", "Docente Uno", null, null, null, null,
            fechaInicio: "", fechaFin: "");
        var snapshot = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30), Programaciones, [fila]);
        Assert.Empty(snapshot.Sesiones);
        Assert.Equal(1, snapshot.RegistrosIgnorados);
        Assert.Empty(snapshot.Docentes);
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
    public void Validar_rechaza_periodo_y_horarios_inconsistentes_y_omite_nrc_ajeno()
    {
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "0800", "0859", null, null, periodo: "202602")]));
        var ajeno = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("NO-EXISTE", null, null, "0800", "0859", null, null)]);
        Assert.Equal(1, ajeno.NrcSinProgramacion);
        Assert.Equal(1, ajeno.RegistrosIgnorados);
        var identidadParcial = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", "P123", null, "0800", "0859", null, null)]);
        Assert.Empty(identidadParcial.Docentes);
        Assert.Single(identidadParcial.Sesiones);
        var nombreSinId = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, "Docente sin ID", "0800", "0859", null, null)]);
        Assert.Empty(nombreSinId.Docentes);
        Assert.Single(nombreSinId.Sesiones);
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("NRC CON ESPACIO", null, null, "0800", "0859", null, null)]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "0800", null, null, null)]));
        Assert.Throws<InvalidDataException>(() => _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30),
            Programaciones, [Registro("A12B3", null, null, "2200", "0100", null, null)]));
    }

    [Fact]
    public void Validar_elige_docente_principal_y_cuenta_advertencia_por_codocencia()
    {
        var registros = new[]
        {
            Registro("A12B3", "P123", "Docente Uno", "0800", "0859", null, null) with { IndDocente = "01", IndPrincipal = "NO" },
            Registro("A12B3", "P456", "Docente Dos", null, null, "0900", "0959") with { IndDocente = "02", IndPrincipal = "SI" }
        };
        var resultado = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30), Programaciones, registros);
        Assert.Equal("P456", Assert.Single(resultado.Docentes).NumeroPersonal);
        Assert.Equal(1, resultado.Advertencias);
        Assert.Equal(2, resultado.Sesiones.Count);
    }

    [Fact]
    public void Validar_elige_nombre_mas_frecuente_tras_normalizar_acentos_y_mayusculas()
    {
        var filas = new[]
        {
            Registro("A12B3", "P123", "Jose Perez", "0800", "0859", null, null),
            Registro("A12B3", "P123", "José Pérez", null, null, "0900", "0959"),
            Registro("A12B3", "P123", "JOSÉ PÉREZ", null, null, null, null)
        };
        var resultado = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30), Programaciones, filas);
        Assert.Equal("Jose Perez", Assert.Single(resultado.Docentes).Nombre);
        Assert.Equal(0, resultado.Advertencias);
        Assert.Equal(1, resultado.RegistrosIgnorados);
    }

    [Fact]
    public void Validar_cuenta_nombres_inconsistentes_y_elige_el_mas_frecuente()
    {
        var filas = new[]
        {
            Registro("A12B3", "P123", "Nombre Alfa", "0800", "0859", null, null),
            Registro("A12B3", "P123", "Nombre Alfa", null, null, "0900", "0959"),
            Registro("A12B3", "P123", "Nombre Beta", null, null, null, null)
        };
        var resultado = _validator.Validar("202601", new(2026, 1, 1), new(2026, 6, 30), Programaciones, filas);
        Assert.Equal("Nombre Alfa", Assert.Single(resultado.Docentes).Nombre);
        Assert.Equal(1, resultado.Advertencias);
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
