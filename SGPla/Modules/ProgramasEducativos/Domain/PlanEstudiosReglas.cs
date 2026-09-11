using System.Text.RegularExpressions;
using SGPla.Commons;
using SGPla.Modules.ProgramasEducativos.Application.Models;

namespace SGPla.Modules.ProgramasEducativos.Domain;

public static partial class PlanEstudiosReglas
{
    public const int CantidadMaximaPlanes = 10;
    public const int LongitudMaximaNombre = 100;
    private const int LongitudMaximaCodigoExperiencia = 10;
    private const int LongitudMaximaNombreExperiencia = 150;

    public static ProgramaEducativoValidacion? ValidarPlanes(
        IReadOnlyList<PlanEstudioParaGuardar> planesEstudio,
        bool esCreacion)
    {
        if (planesEstudio.Count > CantidadMaximaPlanes)
        {
            return Error(
                "planesEstudio",
                $"No se pueden registrar más de {CantidadMaximaPlanes} planes de estudio.");
        }

        var planesRegistrados = new HashSet<int>();
        var claves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var indice = 0; indice < planesEstudio.Count; indice++)
        {
            var plan = planesEstudio[indice];
            var prefijoCampo = $"planesEstudio[{indice}]";

            if (esCreacion && plan.IdPlanEstudios.HasValue)
            {
                return Error(
                    $"{prefijoCampo}.idPlanEstudios",
                    "Un plan nuevo no debe incluir un identificador.");
            }

            if (plan.IdPlanEstudios is <= 0)
            {
                return Error(
                    $"{prefijoCampo}.idPlanEstudios",
                    "El identificador del plan de estudios no es válido.");
            }

            if (plan.IdPlanEstudios.HasValue
                && !planesRegistrados.Add(plan.IdPlanEstudios.Value))
            {
                return Error(
                    $"{prefijoCampo}.idPlanEstudios",
                    "El mismo plan de estudios no puede registrarse más de una vez.");
            }

            if (string.IsNullOrWhiteSpace(plan.Nombre))
            {
                return Error($"{prefijoCampo}.nombre", "El nombre del plan es obligatorio.");
            }

            if (plan.Nombre.Trim().Length > LongitudMaximaNombre)
            {
                return Error(
                    $"{prefijoCampo}.nombre",
                    "El nombre del plan no puede exceder los 100 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(plan.Modalidad)
                && !Constantes.MODALIDADES.Contains(plan.Modalidad.Trim()))
            {
                return Error(
                    $"{prefijoCampo}.modalidad",
                    "La modalidad del plan no es válida.");
            }

            var clave = $"{plan.Nombre.Trim()}|{plan.Modalidad?.Trim() ?? string.Empty}";
            if (!claves.Add(clave))
            {
                return Error(
                    $"{prefijoCampo}.nombre",
                    "Ya se agregó un plan con el mismo nombre y modalidad.");
            }

            var validacionArchivo = ValidarArchivo(plan.Archivo, prefijoCampo, plan.IdPlanEstudios.HasValue);
            if (validacionArchivo is not null)
            {
                return validacionArchivo;
            }
        }

        return null;
    }

    public static ProgramaEducativoValidacion? ValidarExperiencias(
        IReadOnlyList<ExperienciaEducativaParaCrear> experiencias,
        string prefijoCampo)
    {
        if (experiencias.Count == 0)
        {
            return Error(
                $"{prefijoCampo}.archivo",
                "El archivo no contiene experiencias educativas válidas.");
        }

        var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var experiencia in experiencias)
        {
            if (string.IsNullOrWhiteSpace(experiencia.Codigo)
                || !CodigoExperienciaRegex().IsMatch(experiencia.Codigo.Trim()))
            {
                return Error(
                    $"{prefijoCampo}.archivo",
                    "El archivo contiene un código de experiencia educativa inválido.");
            }

            if (!codigos.Add(experiencia.Codigo.Trim()))
            {
                return Error(
                    $"{prefijoCampo}.archivo",
                    "El archivo contiene experiencias educativas con código repetido.");
            }

            if (string.IsNullOrWhiteSpace(experiencia.Nombre)
                || experiencia.Nombre.Trim().Length > LongitudMaximaNombreExperiencia
                || string.IsNullOrWhiteSpace(experiencia.PerfilDocente)
                || string.IsNullOrWhiteSpace(experiencia.Horas)
                || string.IsNullOrWhiteSpace(experiencia.Creditos)
                || experiencia.Codigo.Trim().Length > LongitudMaximaCodigoExperiencia)
            {
                return Error(
                    $"{prefijoCampo}.archivo",
                    "El archivo contiene experiencias educativas incompletas o inválidas.");
            }
        }

        return null;
    }

    private static ProgramaEducativoValidacion? ValidarArchivo(
        ArchivoPlanContenido? archivo,
        string prefijoCampo,
        bool esPlanExistente)
    {
        if (archivo is null)
        {
            return null;
        }

        if (archivo.Tamanio <= 0 || archivo.Contenido.Length == 0)
        {
            return Error($"{prefijoCampo}.archivo", "El archivo del plan está vacío.");
        }

        if (archivo.Tamanio > PlanEstudiosConstantes.TamanioMaximoArchivo)
        {
            return Error(
                $"{prefijoCampo}.archivo",
                "El archivo del plan no puede exceder 10 MB.");
        }

        if (string.IsNullOrWhiteSpace(archivo.NombreOriginal)
            || !PlanEstudiosConstantes.ExtensionesPermitidas.Contains(
                Path.GetExtension(archivo.NombreOriginal)))
        {
            return Error(
                $"{prefijoCampo}.archivo",
                "El archivo del plan debe tener extensión XLS o XLSX.");
        }

        return null;
    }

    private static ProgramaEducativoValidacion Error(string campo, string mensaje) =>
        new(campo, mensaje);

    [GeneratedRegex("^[A-Z]{4} [0-9]{5}$", RegexOptions.Compiled)]
    private static partial Regex CodigoExperienciaRegex();
}
