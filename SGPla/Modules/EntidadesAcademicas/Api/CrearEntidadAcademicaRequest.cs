namespace SGPla.Modules.EntidadesAcademicas.Api;

public sealed class CrearEntidadAcademicaRequest
{
    public string? Clave { get; set; }

    public string? Nombre { get; set; }

    public string? CalleNumero { get; set; }

    public string? Colonia { get; set; }

    public string? Cp { get; set; }

    public string? Municipio { get; set; }

    public string? Telefono { get; set; }

    public string? Extension { get; set; }

    public int? IdAreaAcademica { get; set; }

    public string? Region { get; set; }
}
