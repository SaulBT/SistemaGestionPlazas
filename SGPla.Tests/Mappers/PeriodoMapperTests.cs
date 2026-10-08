using SGPla.Mappers;

namespace SGPla.Tests.Mappers;

public class PeriodoMapperTests
{
    [Theory]
    [InlineData(2026, 8, 1, "202701")]
    [InlineData(2026, 10, 8, "202701")]
    [InlineData(2026, 12, 31, "202701")]
    [InlineData(2027, 1, 31, "202701")]
    [InlineData(2027, 2, 1, "202751")]
    [InlineData(2027, 7, 31, "202751")]
    public void ConstruirCodigoPeriodo_InfiereElPeriodoSemestralDeLaFecha(int anio, int mes, int dia, string esperado)
    {
        var codigo = PeriodoEscolarMapper.PeriodoMapper.ConstruirCodigoPeriodo(new DateOnly(anio, mes, dia));

        Assert.Equal(esperado, codigo);
    }
}
