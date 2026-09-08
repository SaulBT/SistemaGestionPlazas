using ExcelDataReader;
using SGPla.Commons;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;

namespace SGPla.Modules.ProgramasEducativos.Infra;

public sealed class PlanEstudiosImportador : IPlanEstudiosImportador
{
    private static readonly HashSet<string> ExperienciasIgnoradas =
        ["ENSO", "BGRC", "BGRE", "BGRT", "FBGR", "FBGT", "EXAV"];

    public PlanEstudiosImportacionResultado Importar(ArchivoPlanContenido archivo)
    {
        try
        {
            using var stream = new MemoryStream(archivo.Contenido, writable: false);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            if (!reader.Read())
            {
                return Error("El archivo del plan no contiene información.");
            }

            var columnas = ObtenerColumnas(reader);
            var columnaFaltante = Constantes.COLUMNAS_REQUERIDAS
                .FirstOrDefault(columna => !columnas.ContainsKey(columna));

            if (columnaFaltante is not null)
            {
                return Error($"No se encontró la columna '{columnaFaltante}' en el archivo.");
            }

            var experiencias = new List<ExperienciaEducativaParaCrear>();

            while (reader.Read())
            {
                var materia = ObtenerTexto(reader, columnas[Constantes.MATERIA_EE]);
                var curso = ObtenerTexto(reader, columnas[Constantes.CURSO_EE]);
                var nombre = ObtenerTexto(reader, columnas[Constantes.DESC_EE]);
                var horasTeoricas = ObtenerTexto(reader, columnas[Constantes.HT_EE]);
                var horasPracticas = ObtenerTexto(reader, columnas[Constantes.HP_EE]);
                var creditos = ObtenerTexto(reader, columnas[Constantes.CREDITOS_EE]);
                var perfilDocente = ObtenerTexto(reader, columnas[Constantes.PERFIL_DOC]);

                if (TodosLosCamposVacios(
                        materia,
                        curso,
                        nombre,
                        horasTeoricas,
                        horasPracticas,
                        creditos,
                        perfilDocente)
                    || string.IsNullOrWhiteSpace(materia)
                    || ExperienciasIgnoradas.Contains(materia))
                {
                    continue;
                }

                if (!TryObtenerHoras(horasTeoricas, out var horasTeoricasNumero)
                    || !TryObtenerHoras(horasPracticas, out var horasPracticasNumero))
                {
                    return Error("El archivo contiene horas teóricas o prácticas inválidas.");
                }

                experiencias.Add(new ExperienciaEducativaParaCrear(
                    $"{materia} {curso}".Trim(),
                    nombre,
                    perfilDocente,
                    (horasTeoricasNumero + horasPracticasNumero).ToString(),
                    creditos));
            }

            return new PlanEstudiosImportacionResultado(experiencias, null);
        }
        catch (Exception)
        {
            return Error("No fue posible procesar el archivo del plan de estudios.");
        }
    }

    private static Dictionary<string, int> ObtenerColumnas(IExcelDataReader reader)
    {
        var columnas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var indice = 0; indice < reader.FieldCount; indice++)
        {
            var encabezado = ObtenerTexto(reader, indice);
            if (!string.IsNullOrWhiteSpace(encabezado))
            {
                columnas[encabezado] = indice;
            }
        }

        return columnas;
    }

    private static string ObtenerTexto(IExcelDataReader reader, int indice)
    {
        return indice >= reader.FieldCount || reader.IsDBNull(indice)
            ? string.Empty
            : Convert.ToString(reader.GetValue(indice))?.Trim() ?? string.Empty;
    }

    private static bool TodosLosCamposVacios(params string[] campos) =>
        campos.All(string.IsNullOrWhiteSpace);

    private static bool TryObtenerHoras(string valor, out int horas)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            horas = 0;
            return true;
        }

        return int.TryParse(valor, out horas);
    }

    private static PlanEstudiosImportacionResultado Error(string mensaje) =>
        new([], mensaje);
}
