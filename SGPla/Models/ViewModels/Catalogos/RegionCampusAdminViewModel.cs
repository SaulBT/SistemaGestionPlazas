using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.Catalogos;

namespace SGPla.Models.ViewModels.Catalogos;

public sealed class RegionCampusAdminViewModel
{
    public IReadOnlyList<RegionAdministracionFila> Regiones { get; init; } = [];
    public IReadOnlyList<CampusAdministracionFila> Campus { get; init; } = [];
}

public sealed class CrearRegionViewModel
{
    [Range(1, int.MaxValue)] public int Clave { get; set; }
    [Required, StringLength(200)] public string Nombre { get; set; } = string.Empty;
}

public sealed class CrearCampusViewModel
{
    [Required, StringLength(50), RegularExpression("^[A-Za-z0-9]+$")] public string Clave { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Nombre { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int RegionId { get; set; }
}

public sealed class EditarNombreCatalogoViewModel
{
    [Range(1, int.MaxValue)] public int Id { get; set; }
    [Required, StringLength(200)] public string Nombre { get; set; } = string.Empty;
}
