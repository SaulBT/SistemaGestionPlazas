namespace SGPla.Modules.ProgramasEducativos.Domain;

public static class PlanEstudiosConstantes
{
    public const int TamanioMaximoArchivo = 10 * 1024 * 1024;
    public const int TamanioMaximoSolicitudHttp = 110 * 1024 * 1024;
    public const string CarpetaArchivos = "planes-estudios";
    public static readonly IReadOnlySet<string> ExtensionesPermitidas =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xls", ".xlsx" };
}
