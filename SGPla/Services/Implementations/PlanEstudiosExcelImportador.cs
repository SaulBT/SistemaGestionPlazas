using System.Globalization;
using System.Text;
using ExcelDataReader;
using SGPla.Commons;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlanEstudiosExcelImportador : IPlanEstudiosExcelImportador
{
    private static readonly HashSet<string> MateriasOmitidas =
        ["ENSO", "BGRC", "BGRE", "BGRT", "FBGR", "FBGT", "EXAV"];

    private readonly ILogger<PlanEstudiosExcelImportador> _logger;

    public PlanEstudiosExcelImportador(ILogger<PlanEstudiosExcelImportador> logger) => _logger = logger;

    public PlanEstudiosExcelImportacionMvc Leer(Stream archivo, string codigoPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archivo);
        var experiencias = new List<GuardarExperienciaEducativaMvcDto>();
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(archivo);
            if (!reader.Read()) return Error("El archivo no contiene encabezados.");
            var columnas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var nombre = Texto(reader, i).ToUpperInvariant();
                if (nombre.Length > 0) columnas[nombre] = i;
            }

            var faltante = Constantes.COLUMNAS_REQUERIDAS.FirstOrDefault(x => !columnas.ContainsKey(x));
            if (faltante is not null) return Error($"El archivo no contiene la columna requerida {faltante}.");

            var codigoEsperado = codigoPlan.Trim().ToUpperInvariant();
            var identificadores = new HashSet<string>(StringComparer.Ordinal);
            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var materia = Celda(reader, columnas, Constantes.MATERIA_EE).ToUpperInvariant();
                var curso = Celda(reader, columnas, Constantes.CURSO_EE).ToUpperInvariant();
                var nombre = Celda(reader, columnas, Constantes.DESC_EE);
                var horasTeoricasTexto = Celda(reader, columnas, Constantes.HT_EE);
                var horasPracticasTexto = Celda(reader, columnas, Constantes.HP_EE);
                var creditosTexto = Celda(reader, columnas, Constantes.CREDITOS_EE);
                var perfil = Celda(reader, columnas, Constantes.PERFIL_DOC);
                var codigoFila = Celda(reader, columnas, Constantes.CODIGO_PLAN).ToUpperInvariant();

                if (new[] { materia, curso, nombre, horasTeoricasTexto, horasPracticasTexto, creditosTexto, perfil, codigoFila }
                    .All(string.IsNullOrWhiteSpace)) continue;
                if (MateriasOmitidas.Contains(materia)) continue;
                if (codigoFila != codigoEsperado)
                    return Error("El archivo contiene filas de otro código de Plan.");
                if (!TryEntero(horasTeoricasTexto, permitirVacio: true, out var horasTeoricas) || horasTeoricas < 0 ||
                    !TryEntero(horasPracticasTexto, permitirVacio: true, out var horasPracticas) || horasPracticas < 0 ||
                    !TryEntero(creditosTexto, permitirVacio: false, out var creditos) || creditos <= 0)
                    return Error("Las horas deben ser enteros no negativos y los créditos un entero positivo.");

                var clave = $"{materia}\u001f{curso}";
                if (!identificadores.Add(clave)) return Error("El archivo contiene materia y curso duplicados.");
                experiencias.Add(new GuardarExperienciaEducativaMvcDto
                {
                    Nombre = nombre,
                    MateriaEe = materia,
                    CursoEe = curso,
                    HorasTeoricas = horasTeoricas,
                    HorasPracticas = horasPracticas,
                    Creditos = creditos,
                    PerfilDocente = perfil
                });
                if (experiencias.Count > 10000) return Error("El archivo excede el límite de 10,000 Experiencias Educativas.");
            }

            return experiencias.Count == 0
                ? Error("El archivo no contiene Experiencias Educativas importables.")
                : new PlanEstudiosExcelImportacionMvc(experiencias, null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el Excel temporal de Planes de Estudio.");
            return Error("No fue posible leer el archivo. Comprueba que sea un Excel válido.");
        }
    }

    private static string Celda(IExcelDataReader reader, IReadOnlyDictionary<string, int> columnas, string nombre) =>
        Texto(reader, columnas[nombre]);

    private static string Texto(IExcelDataReader reader, int indice) =>
        indice >= reader.FieldCount || reader.IsDBNull(indice)
            ? string.Empty
            : Convert.ToString(reader.GetValue(indice), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

    private static bool TryEntero(string valor, bool permitirVacio, out int resultado)
    {
        if (permitirVacio && string.IsNullOrWhiteSpace(valor))
        {
            resultado = 0;
            return true;
        }
        if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero) &&
            numero == decimal.Truncate(numero) && numero is >= int.MinValue and <= int.MaxValue)
        {
            resultado = (int)numero;
            return true;
        }
        resultado = 0;
        return false;
    }

    private static PlanEstudiosExcelImportacionMvc Error(string mensaje) => new([], mensaje);
}
