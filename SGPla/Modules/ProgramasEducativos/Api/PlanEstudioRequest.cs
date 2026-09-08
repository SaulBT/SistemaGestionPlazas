using Microsoft.AspNetCore.Http;

namespace SGPla.Modules.ProgramasEducativos.Api;

public sealed class PlanEstudioRequest
{
    public int? IdPlanEstudios { get; set; }

    public string? Nombre { get; set; }

    public string? Modalidad { get; set; }

    public IFormFile? Archivo { get; set; }
}
