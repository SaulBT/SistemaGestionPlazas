using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.Ofertas;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas;

public sealed class CrearOfertaViewModel
{
    [Range(1, int.MaxValue)] public int ProgramacionAcademicaId { get; set; }
    [Required, StringLength(100)] public string ClavePlaza { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int TipoPlazaId { get; set; }
    [Range(1, int.MaxValue)] public int TipoContratacionId { get; set; }
    [Required] public string PerfilSolicitado { get; set; } = string.Empty;
    [StringLength(1000)] public string? Justificacion { get; set; }
    public ProgramacionOfertaMvc? Programacion { get; set; }
    public IReadOnlyList<OfertaMvcOpcion> TiposPlaza { get; set; } = [];
    public IReadOnlyList<OfertaMvcOpcion> TiposContratacion { get; set; } = [];
}
