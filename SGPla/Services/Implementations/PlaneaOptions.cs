using Microsoft.Extensions.Options;

namespace SGPla.Services.Implementations;

public sealed class PlaneaOptions : IValidateOptions<PlaneaOptions>
{
    public const string Seccion = "Planea";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 120;
    public string ModoAutenticacion { get; set; } = "Header";
    public string NombreParametro { get; set; } = "X-API-KEY";

    public ValidateOptionsResult Validate(string? name, PlaneaOptions options)
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
            return ValidateOptionsResult.Fail("Planea:BaseUrl debe ser una URL absoluta HTTPS.");
        if (options.TimeoutSegundos is < 1 or > 600)
            return ValidateOptionsResult.Fail("Planea:TimeoutSegundos debe estar entre 1 y 600.");
        if (!string.Equals(options.ModoAutenticacion, "Header", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.ModoAutenticacion, "Query", StringComparison.OrdinalIgnoreCase))
            return ValidateOptionsResult.Fail("Planea:ModoAutenticacion debe ser Header o Query.");
        if (string.IsNullOrWhiteSpace(options.NombreParametro))
            return ValidateOptionsResult.Fail("Planea:NombreParametro es obligatorio.");
        return ValidateOptionsResult.Success;
    }
}
