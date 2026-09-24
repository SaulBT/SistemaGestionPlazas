namespace SGPla.Models.DTOs.Catalogos;

public sealed record RegionAdministracionFila(int Id, int Clave, string Nombre);
public sealed record CampusAdministracionFila(int Id, string Clave, string Nombre, int RegionId, string Region);

public sealed class CrearRegionMvcDto
{
    public int Clave { get; init; }
    public string Nombre { get; init; } = string.Empty;
}

public sealed class CrearCampusMvcDto
{
    public string Clave { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public int RegionId { get; init; }
}
