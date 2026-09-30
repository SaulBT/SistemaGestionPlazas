using SGPla.Commons;

namespace SGPla.Tests.Services;

public class PlaneaPeriodosTests
{
    [Theory]
    [InlineData("202701", "2026-08-01", "2027-01-31")]
    [InlineData("202751", "2027-02-01", "2027-07-31")]
    [InlineData(" 202651 ", "2026-02-01", "2026-07-31")]
    public void FechasPorCodigo_DerivaElRangoInstitucional(string codigo, string inicio, string fin)
    {
        var fechas = PlaneaPeriodos.FechasPorCodigo(codigo);

        Assert.NotNull(fechas);
        Assert.Equal(DateOnly.Parse(inicio), fechas.Value.Inicio);
        Assert.Equal(DateOnly.Parse(fin), fechas.Value.Fin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2027")]
    [InlineData("202702")]
    [InlineData("ABCD01")]
    public void FechasPorCodigo_CodigoNoReconocido_DevuelveNull(string? codigo)
    {
        Assert.Null(PlaneaPeriodos.FechasPorCodigo(codigo));
    }
}
