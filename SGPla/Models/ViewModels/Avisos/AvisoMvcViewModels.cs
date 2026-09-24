using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.Avisos;

namespace SGPla.Models.ViewModels.Avisos;

public sealed class AvisosMvcIndexViewModel
{
    public IReadOnlyList<AvisoMvcItem> Avisos { get; init; } = [];
    public IReadOnlyList<AvisoMvcOpcion> Periodos { get; init; } = [];
    public IReadOnlyList<AvisoMvcOpcion> Entidades { get; init; } = [];
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int? PeriodoEscolarId { get; init; }
    public int? EntidadAcademicaId { get; init; }
    public string? Estado { get; init; }
}

public sealed class AvisoMvcDetalleViewModel
{
    public AvisoMvcDetalle Aviso { get; set; } = null!;
    public AvisoMvcNuevoDatos Datos { get; set; } = new([], [], [], [], []);
    public List<int> OfertaIds { get; set; } = [];
    public EnviarAvisoMvcViewModel Envio { get; set; } = new();
}

public sealed class EnviarAvisoMvcViewModel
{
    [Range(1, int.MaxValue)]
    public int ModalidadRecepcionId { get; set; }

    [Required]
    public string Requisitos { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LugarRecepcion { get; set; }

    [Required, EmailAddress, StringLength(254)]
    public string CorreoContacto { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string NombreTitular { get; set; } = string.Empty;

    [Required]
    public DateOnly FechaConsejoTecnico { get; set; }

    [Required]
    public DateOnly FechaVacantes { get; set; }

    [MinLength(1)]
    public List<AvisoHorarioEntradaViewModel> Horarios { get; set; } = [new()];
}

public sealed class AvisoHorarioEntradaViewModel
{
    [Required]
    public DateOnly Fecha { get; set; }

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }
}

public sealed class CrearAvisoMvcViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un periodo vigente.")]
    public int PeriodoEscolarId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un sistema educativo vigente.")]
    public int SistemaEducativoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un artículo vigente.")]
    public int ArticuloId { get; set; }

    [Required]
    public string TipoComunicado { get; set; } = "AVISO";

    [MinLength(1, ErrorMessage = "Selecciona al menos una Oferta disponible.")]
    public List<int> OfertaIds { get; set; } = [];

    public AvisoMvcNuevoDatos Datos { get; set; } = new([], [], [], [], []);
}
