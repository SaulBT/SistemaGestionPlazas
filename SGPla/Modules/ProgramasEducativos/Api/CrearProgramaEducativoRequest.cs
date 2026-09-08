using Microsoft.AspNetCore.Http;

namespace SGPla.Modules.ProgramasEducativos.Api;

public sealed class CrearProgramaEducativoRequest
{
    public string? Nombre { get; set; }

    public string? Campus { get; set; }

    public int? IdEntidadAcademica { get; set; }

    public List<PlanEstudioRequest> PlanesEstudio { get; set; } = [];
}
