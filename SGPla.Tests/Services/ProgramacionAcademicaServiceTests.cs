using SGPla.Models.DTOs.Oferta;

namespace SGPla.Tests.Services;

public class ProgramacionAcademicaServiceTests
{
    [Theory]
    [InlineData(null, "DOCENTE", true)]
    [InlineData("123", null, true)]
    [InlineData(" ", " ", false)]
    public void ClasificaComoConvocadaSoloConDatosDelDocente(string? np, string? nombre, bool convocada)
    {
        var dto = new OfertaDTO { NP = np, NombreDocente = nombre };
        Assert.Equal(convocada, dto.TieneDocente);
    }
}
