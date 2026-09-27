using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlaneaClient : IPlaneaClient
{
    private const int TamanoMaximoRespuesta = 50 * 1024 * 1024;
    private static readonly (byte Dia, string Prefijo)[] Dias =
    [
        (1, "LUN"), (2, "MAR"), (3, "MIE"), (4, "JUE"), (5, "VIE"), (6, "SAB")
    ];

    private readonly HttpClient _httpClient;
    private readonly PlaneaOptions _options;

    public PlaneaClient(HttpClient httpClient, IOptions<PlaneaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSegundos);
    }

    public async Task<IReadOnlyList<PlaneaRegistro>> ObtenerProgramacionesAsync(
        string clavePeriodo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clavePeriodo)) throw new ArgumentException("La clave del periodo es obligatoria.", nameof(clavePeriodo));
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Falta configurar Planea:ApiKey; no se realizó ninguna petición a PLANEA.");
        var baseUrl = _options.BaseUrl.EndsWith('/') ? _options.BaseUrl : _options.BaseUrl + "/";
        var uri = new Uri(new Uri(baseUrl, UriKind.Absolute), Uri.EscapeDataString(clavePeriodo));
        if (string.Equals(_options.ModoAutenticacion, "Query", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri);
            var queryName = Uri.EscapeDataString(_options.NombreParametro);
            var queryValue = Uri.EscapeDataString(_options.ApiKey);
            builder.Query = string.IsNullOrEmpty(builder.Query) ? $"{queryName}={queryValue}" : $"{builder.Query.TrimStart('?')}&{queryName}={queryValue}";
            uri = builder.Uri;
        }
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (string.Equals(_options.ModoAutenticacion, "Header", StringComparison.OrdinalIgnoreCase))
            request.Headers.TryAddWithoutValidation(_options.NombreParametro, _options.ApiKey);
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("No fue posible comunicarse con PLANEA.", null, ex.StatusCode);
        }
        using (response)
        {
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            throw new InvalidOperationException("PLANEA rechazó la credencial configurada.");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"PLANEA respondió HTTP {(int)response.StatusCode}.", null, response.StatusCode);
        if (!EsJson(response.Content.Headers.ContentType?.MediaType))
            throw new InvalidDataException("PLANEA no devolvió contenido JSON.");
        if (response.Content.Headers.ContentLength > TamanoMaximoRespuesta)
            throw new InvalidDataException("La respuesta de PLANEA excede el tamaño permitido.");

        await using var origen = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var memoria = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var leidos = await origen.ReadAsync(buffer, cancellationToken);
            if (leidos == 0) break;
            if (memoria.Length + leidos > TamanoMaximoRespuesta)
                throw new InvalidDataException("La respuesta de PLANEA excede el tamaño permitido.");
            await memoria.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
        }
        memoria.Position = 0;

        try
        {
            using var json = await JsonDocument.ParseAsync(memoria, new JsonDocumentOptions { MaxDepth = 32 }, cancellationToken);
            if (json.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("PLANEA debe devolver un arreglo JSON directo.");
            var registros = new List<PlaneaRegistro>();
            foreach (var elemento in json.RootElement.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (elemento.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("La respuesta de PLANEA contiene un elemento que no es objeto.");
                registros.Add(LeerRegistro(elemento));
            }
            if (registros.Count == 0)
                throw new InvalidDataException("PLANEA devolvió un arreglo vacío; el snapshot vigente se conserva.");
            return registros;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("PLANEA devolvió JSON malformado.", ex);
        }
    }
    }

    private static PlaneaRegistro LeerRegistro(JsonElement objeto)
    {
        var horarios = Dias.Select(dia => new PlaneaHorarioDia(dia.Dia,
            ObtenerEscalar(objeto, $"{dia.Prefijo}_INI"), ObtenerEscalar(objeto, $"{dia.Prefijo}_FIN"))).ToArray();
        return new PlaneaRegistro(ObtenerEscalar(objeto, "PERIODO"), ObtenerEscalar(objeto, "NRC"),
            ObtenerEscalar(objeto, "ID_DOCENTE"), ObtenerEscalar(objeto, "NOMBRE"),
            ObtenerEscalar(objeto, "EDIFICIO"), ObtenerEscalar(objeto, "AULA"),
            ObtenerEscalar(objeto, "FECHA_INICIO"), ObtenerEscalar(objeto, "FECHA_FIN"), horarios);
    }

    private static string? ObtenerEscalar(JsonElement objeto, string propiedad)
    {
        if (!objeto.TryGetProperty(propiedad, out var valor) || valor.ValueKind == JsonValueKind.Null) return null;
        return valor.ValueKind switch
        {
            JsonValueKind.String => valor.GetString(),
            JsonValueKind.Number => valor.GetRawText(),
            _ => throw new InvalidDataException($"PLANEA devolvió un valor no escalar para {propiedad}.")
        };
    }

    private static bool EsJson(string? mediaType) =>
        string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)
        || mediaType?.EndsWith("+json", StringComparison.OrdinalIgnoreCase) == true;
}
