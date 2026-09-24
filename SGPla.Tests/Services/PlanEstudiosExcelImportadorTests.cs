using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class PlanEstudiosExcelImportadorTests
{
    private readonly PlanEstudiosExcelImportador _importador = new(NullLogger<PlanEstudiosExcelImportador>.Instance);

    [Fact]
    public void Leer_separa_materia_curso_y_horas_sin_conservar_el_archivo()
    {
        using var stream = CrearArchivo([("MAT-101", "02", "Matemáticas", 3, 2, 5, "Perfil")], "PLAN-01");

        var resultado = _importador.Leer(stream, "PLAN-01");

        var experiencia = Assert.Single(resultado.Experiencias);
        Assert.Null(resultado.Error);
        Assert.Equal("MAT-101", experiencia.MateriaEe);
        Assert.Equal("02", experiencia.CursoEe);
        Assert.Equal(3, experiencia.HorasTeoricas);
        Assert.Equal(2, experiencia.HorasPracticas);
        Assert.Equal(5, experiencia.Creditos);
    }

    [Fact]
    public void Leer_rechaza_filas_de_otro_codigo_de_plan()
    {
        using var stream = CrearArchivo([("MAT-101", "02", "Matemáticas", 3, 2, 5, "Perfil")], "PLAN-OTRO");

        var resultado = _importador.Leer(stream, "PLAN-01");

        Assert.Empty(resultado.Experiencias);
        Assert.Contains("otro código", resultado.Error);
    }

    [Fact]
    public void Leer_rechaza_materia_y_curso_duplicados_en_el_archivo()
    {
        using var stream = CrearArchivo([
            ("MAT-101", "02", "Matemáticas", 3, 2, 5, "Perfil"),
            ("MAT-101", "02", "Matemáticas II", 4, 1, 5, "Perfil")
        ], "PLAN-01");

        var resultado = _importador.Leer(stream, "PLAN-01");

        Assert.Empty(resultado.Experiencias);
        Assert.Contains("duplicados", resultado.Error);
    }

    private static MemoryStream CrearArchivo(
        IReadOnlyList<(string Materia, string Curso, string Nombre, int HorasTeoricas, int HorasPracticas, int Creditos, string Perfil)> filas,
        string codigoPlan)
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Plan");
        var headers = new[] { "CODIGO_PLAN", "MATERIA_EE", "CURSO_EE", "DESC_EE", "PERFIL_DOC", "HT_EE", "HP_EE", "CREDITOS_EE" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        for (var i = 0; i < filas.Count; i++)
        {
            var row = i + 2;
            var value = filas[i];
            sheet.Cell(row, 1).Value = codigoPlan;
            sheet.Cell(row, 2).Value = value.Materia;
            sheet.Cell(row, 3).Value = value.Curso;
            sheet.Cell(row, 4).Value = value.Nombre;
            sheet.Cell(row, 5).Value = value.Perfil;
            sheet.Cell(row, 6).Value = value.HorasTeoricas;
            sheet.Cell(row, 7).Value = value.HorasPracticas;
            sheet.Cell(row, 8).Value = value.Creditos;
        }
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        workbook.Dispose();
        stream.Position = 0;
        return stream;
    }
}
