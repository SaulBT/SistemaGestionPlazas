using System.Net.Http.Headers;
using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlaneaClient : IPlaneaClient
{
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
            if (primerByte != (byte)'{')
                throw new InvalidDataException("PLANEA debe devolver un objeto JSON con la sección horarios.");

            await using var conPrefijo = new StreamConPrefijo(limitado, primerByte);
            try
            {
                var parser = new PlaneaJsonReader(clavePeriodo);
                var registros = await LeerEnvoltorioAsync(conPrefijo, parser, cancellationToken);
                if (!parser.PeriodoEncontrado)
                    throw new InvalidDataException("PLANEA no devolvió el periodo solicitado.");
                if (!parser.HorariosEncontrados)
                    throw new InvalidDataException("PLANEA no devolvió la sección horarios.");
                if (!parser.HorariosTerminados)
                    throw new InvalidDataException("PLANEA devolvió JSON malformado.");
                if (registros.Count == 0)
                    throw new InvalidDataException("PLANEA devolvió una sección horarios vacía; el snapshot vigente se conserva.");
                return registros;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("PLANEA devolvió JSON malformado.", ex);
            }
        }
    }

    private static async Task<IReadOnlyList<PlaneaRegistro>> LeerEnvoltorioAsync(Stream stream,
        PlaneaJsonReader parser, CancellationToken cancellationToken)
    {
        const int tamanoInicial = 64 * 1024;
        var buffer = ArrayPool<byte>.Shared.Rent(tamanoInicial);
        var longitud = 0;
        try
        {
            while (true)
            {
                if (longitud == buffer.Length)
                {
                    var ampliado = ArrayPool<byte>.Shared.Rent(checked(buffer.Length * 2));
                    buffer.AsSpan(0, longitud).CopyTo(ampliado);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = ampliado;
                }
                var leidos = await stream.ReadAsync(buffer.AsMemory(longitud), cancellationToken);
                var final = leidos == 0;
                longitud += leidos;
                var consumidos = parser.Procesar(buffer.AsSpan(0, longitud), final, out var incompleto);
                if (consumidos > 0)
                {
                    buffer.AsSpan(consumidos, longitud - consumidos).CopyTo(buffer);
                    longitud -= consumidos;
                }
                if (final)
                {
                    if (longitud != 0 || incompleto)
                        throw new JsonException("El documento terminó antes de completar un token JSON.");
                    return parser.Registros;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
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
