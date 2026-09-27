using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlaneaClient : IPlaneaClient
{
    private static readonly (byte Dia, string Prefijo)[] Dias =
    [
        (1, "LUN"), (2, "MAR"), (3, "MIE"), (4, "JUE"), (5, "VIE"), (6, "SAB")
    ];
    private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 32 };

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

            var limiteBytes = checked((long)_options.TamanoMaximoMb * 1024 * 1024);
            if (response.Content.Headers.ContentLength > limiteBytes)
                throw new InvalidDataException("La respuesta de PLANEA excede el tamaño permitido.");

            await using var origen = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var limitado = new LimiteBytesStream(origen, limiteBytes);
            var primerByte = await LeerPrimerNoEspacioAsync(limitado, cancellationToken);
            if (primerByte == 0)
                throw new InvalidDataException("PLANEA devolvió JSON malformado.");
            if (primerByte != (byte)'[')
                throw new InvalidDataException("PLANEA debe devolver un arreglo JSON directo.");

            await using var conPrefijo = new StreamConPrefijo(limitado, primerByte);
            var registros = new List<PlaneaRegistro>();
            try
            {
                await foreach (var elemento in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(
                                   conPrefijo, JsonOptions, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (elemento.ValueKind != JsonValueKind.Object)
                        throw new InvalidDataException("La respuesta de PLANEA contiene un elemento que no es objeto.");
                    registros.Add(LeerRegistro(elemento));
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("PLANEA devolvió JSON malformado.", ex);
            }

            if (registros.Count == 0)
                throw new InvalidDataException("PLANEA devolvió un arreglo vacío; el snapshot vigente se conserva.");
            return registros;
        }
    }

    private static async Task<byte> LeerPrimerNoEspacioAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        while (await stream.ReadAsync(buffer, cancellationToken) != 0)
        {
            if (buffer[0] is not ((byte)' ') and not ((byte)'\t') and not ((byte)'\r') and not ((byte)'\n'))
                return buffer[0];
        }
        return 0;
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

    private sealed class LimiteBytesStream(Stream inner, long limit) : Stream
    {
        private long _read;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => Cuenta(inner.Read(buffer, offset, count));
        public override int Read(Span<byte> buffer) => Cuenta(inner.Read(buffer));
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            Cuenta(await inner.ReadAsync(buffer, cancellationToken));
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            Cuenta(await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));
        protected override void Dispose(bool disposing) => base.Dispose(disposing);
        public override ValueTask DisposeAsync() { GC.SuppressFinalize(this); return ValueTask.CompletedTask; }

        private int Cuenta(int bytes)
        {
            _read += bytes;
            if (_read > limit) throw new InvalidDataException("La respuesta de PLANEA excede el tamaño permitido.");
            return bytes;
        }
    }

    private sealed class StreamConPrefijo(Stream inner, byte prefix) : Stream
    {
        private bool _pendiente = true;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty) return 0;
            if (_pendiente) { buffer[0] = prefix; _pendiente = false; return 1; }
            return inner.Read(buffer);
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty) return 0;
            if (_pendiente) { buffer.Span[0] = prefix; _pendiente = false; return 1; }
            return await inner.ReadAsync(buffer, cancellationToken);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        protected override void Dispose(bool disposing) => base.Dispose(disposing);
        public override ValueTask DisposeAsync() { GC.SuppressFinalize(this); return ValueTask.CompletedTask; }
    }
}
