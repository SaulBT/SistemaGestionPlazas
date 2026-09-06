namespace SGPla.Modules.ProgramasEducativos.Api;

public sealed class ActualizarProgramaEducativoRequest
{
    public string? Nombre { get; set; }

    public string? Campus { get; set; }

    public int? IdEntidadAcademica { get; set; }
}
