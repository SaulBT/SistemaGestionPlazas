using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Moq;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class ProgramacionAcademicaMvcServiceTests
{
    private readonly Mock<IProgramacionAcademicaMvcRepository> _repository = new();
    private readonly ProgramacionAcademicaMvcService _service;

    public ProgramacionAcademicaMvcServiceTests() => _service = new ProgramacionAcademicaMvcService(_repository.Object);

    [Fact]
    public async Task Importar_acepta_NRC_alfanumerico_y_deduplica_entre_vacantes_y_descargas()
    {
        using var vacantes = CrearExcel("DERECHO", "A12B3", "Teoría general");
        using var descargas = CrearExcel("DERECHO", "A12B3", "Teoría general");
        var archivoVacantes = ComoArchivo(vacantes, "vacantes.xlsx");
        var archivoDescargas = ComoArchivo(descargas, "descargas.xlsx");
        _repository.Setup(x => x.ImportarAsync(5, 9, It.IsAny<IReadOnlyList<ProgramacionAcademicaImportacionFila>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoImportacionProgramacionMvc(1, 0, 1));

        var resultado = await _service.ImportarVacantesYDescargasAsync(5, 9, archivoVacantes, archivoDescargas);

        Assert.Equal(1, resultado.Creadas);
        _repository.Verify(x => x.ImportarAsync(5, 9, It.Is<IReadOnlyList<ProgramacionAcademicaImportacionFila>>(filas =>
            filas.Count == 1 && filas[0].Nrc == "A12B3" && filas[0].Programa == "DERECHO"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_rechaza_un_NRC_repetido_con_otra_EE_antes_de_persistir()
    {
        using var vacantes = CrearExcel("DERECHO", "A12B3", "Teoría general");
        using var descargas = CrearExcel("DERECHO", "A12B3", "Filosofía");

        await Assert.ThrowsAsync<ArgumentException>(() => _service.ImportarVacantesYDescargasAsync(
            5, 9, ComoArchivo(vacantes, "vacantes.xlsx"), ComoArchivo(descargas, "descargas.xlsx")));

        _repository.Verify(x => x.ImportarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyList<ProgramacionAcademicaImportacionFila>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reporta_archivo_excel_incompleto_como_error_de_validacion()
    {
        using var vacantes = new MemoryStream([0x50, 0x4B]);
        using var descargas = CrearExcel("DERECHO", "A12B3", "Teoría general");

        var error = await Assert.ThrowsAsync<ArgumentException>(() => _service.ImportarVacantesYDescargasAsync(
            5, 9, ComoArchivo(vacantes, "vacantes.xlsx"), ComoArchivo(descargas, "descargas.xlsx")));

        Assert.Contains("formato de programación válido", error.Message, StringComparison.OrdinalIgnoreCase);
        _repository.Verify(x => x.ImportarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyList<ProgramacionAcademicaImportacionFila>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static MemoryStream CrearExcel(string programa, string nrc, string experiencia)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Programación");
        hoja.Cell(1, 1).Value = $"Programa: {programa}";
        hoja.Cell(2, 1).Value = "NRC";
        hoja.Cell(2, 2).Value = "EXPERIENCIA EDUCATIVA";
        hoja.Cell(3, 1).Value = nrc;
        hoja.Cell(3, 2).Value = experiencia;
        var stream = new MemoryStream();
        libro.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static IFormFile ComoArchivo(Stream stream, string nombre)
    {
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, nombre, nombre);
    }
}
