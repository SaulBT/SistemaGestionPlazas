using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Models.ViewModels.PlanesEstudios;

public sealed class PlanEstudiosMvcIndexViewModel
{
    public PlanEstudiosMvcFiltro Filtro { get; init; } = new();
    public IReadOnlyList<PlanEstudiosMvcFila> Items { get; init; } = [];
    public int Total { get; init; }
    public IReadOnlyList<CatalogoOpcion> Regiones { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Areas { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Entidades { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Programas { get; init; } = [];
}

public sealed class PlanEstudiosMvcCrearViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProgramaEducativoId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression("^[A-Za-z0-9._-]+$")]
    public string Codigo { get; set; } = string.Empty;

    public IReadOnlyList<CatalogoOpcion> Programas { get; set; } = [];
}

public sealed class PlanEstudiosMvcDetalleViewModel
{
    public PlanEstudiosMvcDetalle Plan { get; init; } = null!;
    public IReadOnlyList<ExperienciaEducativaMvcDto> Experiencias { get; init; } = [];
    public int TotalExperiencias { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public string? Busqueda { get; init; }
    public IReadOnlyList<CatalogoOpcion> AreasFormacion { get; init; } = [];
}

public sealed class PlanEstudiosMvcExperienciaViewModel
{
    public int Id { get; set; }
    [Required]
    [Range(1, int.MaxValue)]
    public int PlanEstudiosId { get; set; }
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression("^[A-Za-z0-9._-]+$")]
    public string MateriaEe { get; set; } = string.Empty;
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression("^[A-Za-z0-9._-]+$")]
    public string CursoEe { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int HorasTeoricas { get; set; }
    [Range(0, int.MaxValue)]
    public int HorasPracticas { get; set; }
    [Range(1, int.MaxValue)]
    public int Creditos { get; set; }
    public string? PerfilDocente { get; set; }
    [Range(1, int.MaxValue)]
    public int AreaFormacionId { get; set; }
    public bool DatosAcademicosBloqueados { get; set; }
    public IReadOnlyList<CatalogoOpcion> AreasFormacion { get; set; } = [];
}
