namespace SGPla.Models.DTOs.EntidadAcademica;

public sealed record FiltroEntidadAcademicaMvcDto(
    int? RegionId,
    int? AreaAcademicaId,
    string? Busqueda,
    int Pagina,
    int Cantidad);

public sealed class EntidadAcademicaMvcInputDto
{
    public string Clave { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Calle { get; init; } = string.Empty;
    public string? NumeroExterior { get; init; }
    public string Colonia { get; init; } = string.Empty;
    public string CodigoPostal { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string? Extension { get; init; }
    public int CampusId { get; init; }
    public int AreaAcademicaId { get; init; }
    public int MunicipioId { get; init; }
}

public sealed record EntidadAcademicaMvcDto(
    int Id,
    string Clave,
    string Nombre,
    string Calle,
    string? NumeroExterior,
    string Colonia,
    string CodigoPostal,
    string Telefono,
    string? Extension,
    int CampusId,
    string CampusNombre,
    int RegionId,
    string RegionNombre,
    int AreaAcademicaId,
    string AreaAcademicaNombre,
    int MunicipioId,
    string MunicipioNombre);
