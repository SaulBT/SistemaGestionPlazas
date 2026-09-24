using System.Net.Http.Headers;
using System.Text.Json;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlaneaClient : IPlaneaClient
{
    private const int TamanoMaximoRespuesta = 50 * 1024 * 1024;
    private static readonly Uri EndpointBase = new("https://planea.uv.mx/planea/index.php/apiroladoovr/periodo/");
    private static readonly (byte Dia, string Prefijo)[] Dias =
    [
        (1, "LUN"), (2, "MAR"), (3, "MIE"), (4, "JUE"), (5, "VIE"), (6, "SAB")
    ];

    private readonly HttpClient _httpClient;

    public PlaneaClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<PlaneaRegistro>> ObtenerProgramacionesAsync(
        string clavePeriodo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clavePeriodo)) throw new ArgumentException("La clave del periodo es obligatoria.", nameof(clavePeriodo));
        var uri = new Uri(EndpointBase, Uri.EscapeDataString(clavePeriodo));
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
