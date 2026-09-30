using System.Text.Json;
using System.Text.Json.Serialization;
using SGPla.Models.DTOs.Planea;
using SGPla.Parsers;

namespace SGPla.Tests.Parsers;

public class PlaneaNormalizadorTests
{
    private static PlaneaRespuesta Cargar()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "planea_muestra.json"));
        return JsonSerializer.Deserialize<PlaneaRespuesta>(json, new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString })!;
    }

    [Fact]
    public void Normalizar_FixtureConservaNrcUnicosYExpandeHorarios()
    {
        var datos = PlaneaNormalizador.Normalizar(Cargar());
        Assert.Equal(9, datos.NrcRecibidos);
        Assert.Equal(1, datos.NrcSinPlan);
        Assert.Single(datos.Copias, c => c.Nrc == "10709");
        Assert.Contains(datos.Copias, c => c.Nrc == "10676" && c.CodigoExperiencia == "CVCB 18003" && c.CodigoPlan == "CIVI-20-E-CR");
        Assert.Contains(datos.Copias, c => c.Nrc == "59262" && c.CodigoExperiencia == "MVZ 80002");
        var horarios = datos.Horarios.Where(h => h.Nrc == "10676").ToList();
        Assert.Equal(5, horarios.Count);
        Assert.All(horarios, h => { Assert.Equal(new TimeOnly(14, 0), h.HoraInicio); Assert.Equal(new TimeOnly(14, 59), h.HoraFin); Assert.Equal("I-INEB", h.Edificio); Assert.Equal("I-02", h.Aula); });
        Assert.Equal(new[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" }, horarios.Select(h => h.Dia));
        Assert.DoesNotContain(datos.Horarios, h => h.Nrc is "10812" or "25108");
        Assert.Contains(datos.Advertencias, a => a.Contains("Horarios de NRC que no vienen en resultado", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("", null)] [InlineData("  ", null)] [InlineData("-", null)] [InlineData("---", null)] [InlineData(" X ", "X")]
    public void Limpiar_Normaliza(string valor, string? esperado) => Assert.Equal(esperado, PlaneaNormalizador.Limpiar(valor));

    [Theory]
    [InlineData("0700", true, 7, 0)] [InlineData("2460", false, 0, 0)] [InlineData("123", false, 0, 0)] [InlineData("ab12", false, 0, 0)] [InlineData(null, false, 0, 0)]
    public void TryParsearHora_ValidaFormato(string? valor, bool valido, int h, int m)
    {
        var ok = PlaneaNormalizador.TryParsearHora(valor, out var hora); Assert.Equal(valido, ok); if (ok) Assert.Equal(new TimeOnly(h, m), hora);
    }

    [Fact]
    public void ParsearFechaYTruncar()
    {
        Assert.Equal(new DateOnly(2026, 8, 17), PlaneaNormalizador.ParsearFecha("2026-08-17"));
        Assert.Null(PlaneaNormalizador.ParsearFecha("x"));
        Assert.Equal("abc", PlaneaNormalizador.Truncar("abcdef", 3));
    }

    [Fact]
    public void Normalizar_DescartaSesionesDuplicadas()
    {
        var respuesta = new PlaneaRespuesta { Resultado = [new() { Nrc="1", Materia="ABCD", Curso="12345", CodigoPlan="PLAN", Titulo="T" }], Horarios = [
            new() { Nrc="1", IdHorario="1", LunIni="0900", LunFin="1000" }, new() { Nrc="1", IdHorario="2", LunIni="0900", LunFin="1000" }] };
        Assert.Single(PlaneaNormalizador.Normalizar(respuesta).Horarios);
    }
}
