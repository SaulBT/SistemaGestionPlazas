using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using SGPla.Parsers;

namespace SGPla.Services.Implementations;

public sealed class ProgramacionAcademicaMvcService : IProgramacionAcademicaMvcService
{
    private const long TamanoMaximoArchivo = 10 * 1024 * 1024;
    private const int MaximoFilas = 20000;
    private static readonly Regex NrcRegex = new("^[A-Z0-9._-]{1,20}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly IProgramacionAcademicaMvcRepository _repository;

    public ProgramacionAcademicaMvcService(IProgramacionAcademicaMvcRepository repository) => _repository = repository;

    public Task<PaginaProgramacionAcademicaMvc> BuscarAsync(
        ProgramacionAcademicaMvcFiltro filtro, int? entidadAcademicaAutorizadaId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (filtro.Pagina < 1 || filtro.TamanoPagina is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(filtro));
        return _repository.BuscarAsync(filtro, entidadAcademicaAutorizadaId, cancellationToken);
    }

    public async Task<ResultadoImportacionProgramacionMvc> ImportarVacantesYDescargasAsync(
        int periodoEscolarId, int entidadAcademicaId, IFormFile archivoVacantes, IFormFile archivoDescargas,
        CancellationToken cancellationToken = default)
    {
        if (periodoEscolarId < 1 || entidadAcademicaId < 1) throw new ArgumentException("Selecciona un periodo y una Entidad Académica vigentes.");
        ValidarArchivo(archivoVacantes, "Vacantes");
        ValidarArchivo(archivoDescargas, "Descargas");
        var filas = new List<ProgramacionAcademicaImportacionFila>();
        filas.AddRange(await LeerArchivoAsync(archivoVacantes, cancellationToken));
        filas.AddRange(await LeerArchivoAsync(archivoDescargas, cancellationToken));
        if (filas.Count == 0) throw new ArgumentException("Los archivos no contienen filas con NRC para importar.");
        if (filas.Count > MaximoFilas) throw new ArgumentException($"La importación excede el límite de {MaximoFilas} filas.");

        var normalizadas = new Dictionary<string, ProgramacionAcademicaImportacionFila>(StringComparer.Ordinal);
        foreach (var fila in filas)
        {
            var programa = fila.Programa.Trim();
            var experiencia = fila.ExperienciaEducativa.Trim();
            var nrc = fila.Nrc.Trim().ToUpperInvariant();
            if (programa.Length is < 1 or > 200 || experiencia.Length is < 1 or > 200 || !NrcRegex.IsMatch(nrc))
                throw new ArgumentException("Cada fila debe tener Programa, Experiencia Educativa y NRC válidos.");
            var candidata = new ProgramacionAcademicaImportacionFila(programa, experiencia, nrc);
            if (normalizadas.TryGetValue(nrc, out var anterior))
            {
                if (NormalizarTexto(anterior.Programa) != NormalizarTexto(programa) ||
                    NormalizarTexto(anterior.ExperienciaEducativa) != NormalizarTexto(experiencia))
                    throw new ArgumentException($"El NRC {nrc} aparece asociado a programas o experiencias distintos en los archivos.");
                continue;
            }
            normalizadas.Add(nrc, candidata);
        }

        return await _repository.ImportarAsync(periodoEscolarId, entidadAcademicaId, normalizadas.Values.ToArray(), cancellationToken);
    }

    public Task<IReadOnlyList<SGPla.Models.ViewModels.Catalogos.CatalogoOpcion>> ObtenerPeriodosAsync(CancellationToken cancellationToken = default) =>
        _repository.ObtenerPeriodosAsync(cancellationToken);

    private static void ValidarArchivo(IFormFile? archivo, string nombre)
    {
        if (archivo is null || archivo.Length is <= 0 or > TamanoMaximoArchivo)
            throw new ArgumentException($"El archivo de {nombre} es obligatorio y debe medir hasta 10 MB.");
        var extension = Path.GetExtension(archivo.FileName);
        if (!(extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
              extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
              extension.Equals(".htm", StringComparison.OrdinalIgnoreCase) ||
              extension.Equals(".html", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"El archivo de {nombre} debe ser Excel o HTML exportado por Excel.");
    }

    private static async Task<IReadOnlyList<ProgramacionAcademicaImportacionFila>> LeerArchivoAsync(
        IFormFile archivo, CancellationToken cancellationToken)
    {
        await using var stream = archivo.OpenReadStream();
        using var memoria = new MemoryStream((int)Math.Min(archivo.Length, TamanoMaximoArchivo));
        await stream.CopyToAsync(memoria, cancellationToken);
        memoria.Position = 0;
        List<SGPla.Models.DTOs.Oferta.OfertaDTO> filas;
        try
        {
            filas = DescargasParser.Parse(memoria, archivo.FileName, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or ArgumentException or FormatException)
        {
            throw new ArgumentException($"El archivo {Path.GetFileName(archivo.FileName)} no tiene un formato de programación válido.", ex);
        }
        return filas.Where(x => !string.IsNullOrWhiteSpace(x.NRC))
            .Select(x => new ProgramacionAcademicaImportacionFila(x.Programa, x.ExperienciaEducativa, x.NRC))
            .ToArray();
    }

    private static string NormalizarTexto(string valor) =>
        new string(valor.Trim().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Normalize(NormalizationForm.FormC).ToUpperInvariant();
}
