using System.ComponentModel.DataAnnotations;
using SGPla.Models.DTOs.Solicitudes;

namespace SGPla.Models.ViewModels.Solicitudes;

public sealed class SolicitudMvcRegistroViewModel
{
    public int AvisoId { get; set; }
    public int AvisoOfertaId { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? PuestoActual { get; set; }

    [Required, StringLength(10000)]
    public string DescripcionPerfil { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Observaciones { get; set; }

    public List<FormacionSolicitudMvcViewModel> Formaciones { get; set; } = [];
    public IReadOnlyList<GradoAcademicoSolicitudMvcOpcion> GradosAcademicos { get; set; } = [];
    public string Vacante { get; set; } = string.Empty;
}

public sealed class FormacionSolicitudMvcViewModel
{
    [Range(1, int.MaxValue)]
    public int GradoAcademicoId { get; set; }

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;
}

public sealed class SolicitudMvcListadoViewModel
{
    public int AvisoId { get; set; }
    public string EstadoAviso { get; set; } = string.Empty;
    public IReadOnlyList<SolicitudMvcItem> Solicitudes { get; set; } = [];
    public IReadOnlyList<GradoAcademicoSolicitudMvcOpcion> GradosAcademicos { get; set; } = [];
    public IReadOnlyList<VacanteSolicitudMvcOpcion> VacantesDisponibles { get; set; } = [];
    public IReadOnlyList<TipoDocumentoAspiranteSolicitudMvcOpcion> TiposDocumento { get; set; } = [];
}

public sealed class SolicitudMvcDetalleViewModel
{
    public int AvisoId { get; set; }
    public int Id { get; set; }
    public int AvisoOfertaId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? PuestoActual { get; set; }
    public string DescripcionPerfil { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime RegistradaEn { get; set; }
    public DateTime? AdmitidaEn { get; set; }
    public DateTime? NoAdmitidaEn { get; set; }
    public string? MotivoNoAdmision { get; set; }
    public List<FormacionSolicitudMvcViewModel> Formaciones { get; set; } = [];
    public IReadOnlyList<FormacionSolicitudMvcItem> FormacionesCapturadas { get; set; } = [];
    public IReadOnlyList<DocumentoSolicitudMvcItem> Documentos { get; set; } = [];
    public IReadOnlyList<GradoAcademicoSolicitudMvcOpcion> GradosAcademicos { get; set; } = [];
    public IReadOnlyList<TipoDocumentoAspiranteSolicitudMvcOpcion> TiposDocumento { get; set; } = [];

    [StringLength(1000)]
    public string? Motivo { get; set; }
}
