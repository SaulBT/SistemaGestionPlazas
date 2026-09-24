using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.IntegranteCt;

namespace SGPla.Models.ViewModels.IntegranteCt;

public sealed class IntegranteConsejoTecnicoMvcIndexViewModel
{
    public IReadOnlyList<IntegranteConsejoTecnicoMvcItem> Integrantes { get; set; } = [];
    public IReadOnlyList<IntegranteConsejoTecnicoMvcTratamiento> Tratamientos { get; set; } = [];
    public IntegranteConsejoTecnicoMvcCambioViewModel Formulario { get; set; } = new();
    public string? Busqueda { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
    public int Total { get; set; }
}

public sealed class IntegranteConsejoTecnicoMvcCambioViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Cargo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TratamientoAcademicoId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? FechaFin { get; set; }

    public IntegranteConsejoTecnicoMvcCambio ToDto() =>
        new(Nombre, Cargo, TratamientoAcademicoId, FechaInicio, FechaFin);
}
