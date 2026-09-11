namespace SGPla.Modules.ProgramasEducativos.Application.Models;

public sealed record ArchivoPlanContenido(
    string NombreOriginal,
    string Tipo,
    long Tamanio,
    byte[] Contenido);

public sealed record ArchivoPlanParaGuardar(
    byte[] Contenido,
    string NombreOriginal,
    string Tipo);

public sealed record ArchivoPlanGuardado(
    string NombreOriginal,
    string Ruta,
    string Tipo,
    long Tamanio);

public sealed record ExperienciaEducativaParaCrear(
    string Codigo,
    string Nombre,
    string PerfilDocente,
    string Horas,
    string Creditos);

public sealed record PlanEstudioParaGuardar(
    int? IdPlanEstudios,
    string? Nombre,
    string? Modalidad,
    ArchivoPlanContenido? Archivo);

public sealed record PlanEstudioParaPersistir(
    int? IdPlanEstudios,
    string Nombre,
    string? Modalidad,
    ArchivoPlanGuardado? ArchivoNuevo,
    IReadOnlyList<ExperienciaEducativaParaCrear> ExperienciasEducativas);

public sealed record PlanEstudioPreparado(
    int? IdPlanEstudios,
    string Nombre,
    string? Modalidad,
    ArchivoPlanContenido? Archivo,
    IReadOnlyList<ExperienciaEducativaParaCrear> ExperienciasEducativas);

public sealed record PreparacionPlanesEstudio(
    IReadOnlyList<PlanEstudioPreparado> PlanesEstudio,
    string? Campo,
    string? Mensaje)
{
    public bool EsValida => string.IsNullOrEmpty(Mensaje);
}

public sealed record PlanEstudioRegistro(
    int IdPlanEstudios,
    string Nombre,
    string? Modalidad,
    int? IdArchivo,
    string? NombreArchivo,
    string? RutaArchivo,
    string? TipoArchivo,
    double? TamanioArchivo,
    int CantidadExperienciasEducativas);

public sealed record PlanEstudiosImportacionResultado(
    IReadOnlyList<ExperienciaEducativaParaCrear> ExperienciasEducativas,
    string? Error)
{
    public bool EsValido => string.IsNullOrEmpty(Error);
}
