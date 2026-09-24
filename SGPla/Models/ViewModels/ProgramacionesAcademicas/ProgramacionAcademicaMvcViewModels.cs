using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.Catalogos;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public sealed class ProgramacionAcademicaMvcIndexViewModel
{
    public ProgramacionAcademicaMvcFiltro Filtro { get; init; } = new();
    public IReadOnlyList<ProgramacionAcademicaMvcFila> Items { get; init; } = [];
    public int Total { get; init; }
    public IReadOnlyList<CatalogoOpcion> Regiones { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Entidades { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Programas { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Periodos { get; init; } = [];
}

public sealed class ProgramacionAcademicaImportarViewModel
{
    [Range(1, int.MaxValue)] public int PeriodoEscolarId { get; set; }
    [Range(1, int.MaxValue)] public int EntidadAcademicaId { get; set; }
    [Required] public IFormFile? ArchivoVacantes { get; set; }
    [Required] public IFormFile? ArchivoDescargas { get; set; }
    public IReadOnlyList<CatalogoOpcion> Regiones { get; set; } = [];
    public IReadOnlyList<CatalogoOpcion> Entidades { get; set; } = [];
    public IReadOnlyList<CatalogoOpcion> Periodos { get; set; } = [];
}
