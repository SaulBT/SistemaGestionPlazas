using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.ProgramasEducativos;

public sealed class CrearProgramaEducativoViewModel
{
    public int IdProgramaEducativo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar una entidad académica")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una entidad académica válida")]
    public int? IdEntidadAcademica { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un sistema educativo")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un sistema educativo válido")]
    public int? IdSistemaEducativo { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un nivel de formación")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un nivel de formación válido")]
    public int? IdNivelFormacion { get; set; }

    // Sólo sirven para filtrar y cargar el combo de entidad; el servidor deriva el ámbito real desde la entidad seleccionada.
    public int? RegionId { get; set; }
    public int? IdAreaAcademica { get; set; }

    [ValidateNever] public List<OptionModel> Regiones { get; set; } = [];
    [ValidateNever] public List<OptionModel> Areas { get; set; } = [];
    [ValidateNever] public List<OptionModel> Entidades { get; set; } = [];
    [ValidateNever] public List<OptionModel> SistemasEducativos { get; set; } = [];
    [ValidateNever] public List<OptionModel> NivelesFormacion { get; set; } = [];
}
