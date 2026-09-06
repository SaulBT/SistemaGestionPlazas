using Microsoft.AspNetCore.Http;

namespace SGPla.Modules.SolicitudesApertura.Api;

public sealed class CrearSolicitudAperturaRequest
{
    public int IdExperienciaEducativa { get; set; }

    public string? Seccion { get; set; }

    public int IdModalidad { get; set; }

    public int CantidadSolicitantes { get; set; }

    public string? Justificacion { get; set; }

    public IFormFile? ArchivoOficio { get; set; }
}
