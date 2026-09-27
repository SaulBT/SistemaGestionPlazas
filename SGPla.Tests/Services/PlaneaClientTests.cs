using System.Net;
using System.IO.Compression;
using System.Text;
using System.Net.Sockets;
using Xunit.Abstractions;
using Microsoft.Extensions.Options;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class PlaneaClientTests
{
    private readonly ITestOutputHelper _output;

    public PlaneaClientTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ObtenerProgramaciones_lee_arreglo_json_y_ignora_campos_desconocidos()
    {
        const string body = """
            [{"PERIODO":202601,"NRC":"A12B3","ID_DOCENTE":123,"NOMBRE":"Docente Uno","FECHA_INICIO":"2026-01-01","FECHA_FIN":"2026-06-30","LUN_INI":"0800","LUN_FIN":"0859","HRS_SEMANA":99}]
            """;
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, "application/json", body));

        var registro = Assert.Single(await client.ObtenerProgramacionesAsync("202601"));

        Assert.Equal("202601", registro.Periodo);
        Assert.Equal("123", registro.NumeroPersonal);
        Assert.Equal("0800", registro.Horarios[0].Inicio);
        Assert.Null(registro.Horarios[1].Inicio);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_respuesta_http_fallida()
    {
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.ServiceUnavailable, "application/json", "[]"));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.ObtenerProgramacionesAsync("202601"));
    }

    [Theory]
    [InlineData("text/html", "<html>maintenance</html>")]
    [InlineData("application/json", "{}")]
    [InlineData("application/json", "[]")]
    public async Task ObtenerProgramaciones_rechaza_respuestas_sin_snapshot_json_valido(string mediaType, string body)
    {
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, mediaType, body));
        await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202601"));
    }

    [Fact]
    public async Task ObtenerProgramaciones_envia_la_clave_en_el_header_configurado()
    {
        var handler = new CapturaHandler(HttpStatusCode.OK, "[]");
        var options = Opciones();
        options.ModoAutenticacion = "Header";
        options.NombreParametro = "X-API-KEY";
        var client = CrearCliente(handler, options);

        await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("test-key", handler.Request!.Headers.GetValues("X-API-KEY").Single());
        Assert.Equal("https://planea.example/api/periodo/202701", handler.Request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task ObtenerProgramaciones_envia_la_clave_en_el_query_configurado()
    {
        var handler = new CapturaHandler(HttpStatusCode.OK, "[]");
        var options = Opciones();
        options.ModoAutenticacion = "Query";
        options.NombreParametro = "token";
        var client = CrearCliente(handler, options);

        await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Contains("token=test-key", handler.Request!.RequestUri!.Query);
        Assert.False(handler.Request.Headers.Contains("X-API-KEY"));
    }

    [Fact]
    public async Task ObtenerProgramaciones_sin_clave_no_hace_peticion()
    {
        var handler = new CapturaHandler(HttpStatusCode.OK, "[]");
        var options = Opciones();
        options.ApiKey = "";
        var client = CrearCliente(handler, options);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Contains("Planea:ApiKey", error.Message);
        Assert.Null(handler.Request);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task ObtenerProgramaciones_credencial_rechazada_no_expone_la_clave(HttpStatusCode status)
    {
        var client = CrearCliente(new RespuestaHandler(status, "application/json", ""));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("PLANEA rechazó la credencial configurada.", error.Message);
        Assert.DoesNotContain("test-key", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ObtenerProgramaciones_lee_stream_sin_content_length_y_respeta_limite_despues_de_descomprimir()
    {
        var bodyStream = new GeneratedRecordsStream(100_000);
        var content = new StreamContent(bodyStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var contentLength = content.Headers.ContentLength;
        var client = CrearCliente(new ContenidoHandler(content));

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var antes = GC.GetTotalMemory(forceFullCollection: true);
        var registros = await client.ObtenerProgramacionesAsync("202701");
        var picoObservado = Math.Max(bodyStream.PicoHeapObservado, GC.GetTotalMemory(forceFullCollection: false));
        var despues = GC.GetTotalMemory(forceFullCollection: true);
        _output.WriteLine($"PlaneaClient 100000: heap antes={antes:N0}, pico observado={picoObservado:N0}, retenido después de GC={despues:N0} bytes; body={bodyStream.BytesRead:N0} bytes.");

        Assert.Equal(100_000, registros.Count);
        Assert.Null(contentLength);
    }

    [Fact]
    public async Task ObtenerProgramaciones_limita_respuesta_stream_sin_content_length()
    {
        var options = Opciones();
        options.TamanoMaximoMb = 1;
        var content = new StreamContent(new GeneratedOversizeStream(2 * 1024 * 1024));
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var client = CrearCliente(new ContenidoHandler(content), options);

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("La respuesta de PLANEA excede el tamaño permitido.", error.Message);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_content_length_superior_al_limite()
    {
        var options = Opciones();
        options.TamanoMaximoMb = 1;
        using var content = new StringContent("[]", null, "application/json");
        content.Headers.ContentLength = 2 * 1024 * 1024;
        var client = CrearCliente(new ContenidoHandler(content), options);

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("La respuesta de PLANEA excede el tamaño permitido.", error.Message);
    }

    [Theory]
    [InlineData("", "PLANEA devolvió JSON malformado.")]
    [InlineData("[{\"PERIODO\":{}}]", "PLANEA devolvió un valor no escalar para PERIODO.")]
    [InlineData("[{", "PLANEA devolvió JSON malformado.")]
    public async Task ObtenerProgramaciones_conserva_errores_de_json(string body, string mensaje)
    {
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, "application/json", body));

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal(mensaje, error.Message);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_raiz_no_arreglo_antes_de_leer_el_cuerpo()
    {
        var stream = new RootThenLargeStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var client = CrearCliente(new ContenidoHandler(content));

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("PLANEA debe devolver un arreglo JSON directo.", error.Message);
        Assert.Equal(1, stream.BytesRead);
    }

    [Fact]
    public async Task ObtenerProgramaciones_acepta_respuesta_gzip_despues_de_descompresion_http()
    {
        var original = Encoding.UTF8.GetBytes("[{\"PERIODO\":202701}]");
        using var comprimido = new MemoryStream();
        await using (var gzip = new GZipStream(comprimido, CompressionLevel.Fastest, leaveOpen: true))
            await gzip.WriteAsync(original);
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var servidor = Task.Run(async () =>
        {
            using var socket = await listener.AcceptTcpClientAsync();
            await using var stream = socket.GetStream();
            var request = new List<byte>();
            var buffer = new byte[1];
            while (request.Count < 4 || !request.TakeLast(4).SequenceEqual("\r\n\r\n"u8.ToArray()))
            {
                if (await stream.ReadAsync(buffer) == 0) break;
                request.Add(buffer[0]);
            }
            var headers = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Encoding: gzip\r\nContent-Length: {comprimido.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(headers);
            await stream.WriteAsync(comprimido.ToArray());
        });
        var options = Opciones();
        options.BaseUrl = $"http://127.0.0.1:{port}/api/periodo";
        var http = new HttpClient(new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.All });
        var client = new PlaneaClient(http, Options.Create(options));
        try
        {
            var registros = await client.ObtenerProgramacionesAsync("202701");
            Assert.Single(registros);
            Assert.Equal("202701", registros[0].Periodo);
            await servidor;
        }
        finally { listener.Stop(); }
    }

    private static PlaneaClient CrearCliente(HttpMessageHandler handler, PlaneaOptions? options = null) =>
        new(new HttpClient(handler), Options.Create(options ?? Opciones()));

    private static PlaneaOptions Opciones() => new()
    {
        BaseUrl = "https://planea.example/api/periodo",
        ApiKey = "test-key",
        TimeoutSegundos = 10,
        ModoAutenticacion = "Header",
        NombreParametro = "X-API-KEY"
    };

    private sealed class CapturaHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, null, "application/json")
            });
        }
    }

    private sealed class RespuestaHandler(HttpStatusCode codigo, string mediaType, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(codigo)
            {
                Content = new StringContent(body, null, new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType))
            });
        }
    }

    private sealed class ContenidoHandler(HttpContent content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }

    private sealed class GeneratedRecordsStream(int count) : Stream
    {
        private static readonly byte[] Record = Encoding.UTF8.GetBytes("{\"PERIODO\":202701}");
        private int _record = -1;
        private int _offset;
        private bool _finished;
        private bool _separadorPendiente;
        public long PicoHeapObservado { get; private set; }
        public long BytesRead { get; private set; }
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
            if (buffer.IsEmpty || _finished) return 0;
            var written = 0;
            while (written < buffer.Length && !_finished)
            {
                if (_record == -1)
                {
                    buffer[written++] = (byte)'[';
                    _record = 0;
                    continue;
                }
                if (_record < count)
                {
                    if (_separadorPendiente)
                    {
                        buffer[written++] = (byte)',';
                        _separadorPendiente = false;
                    }
                    else
                    {
                        var copied = Math.Min(buffer.Length - written, Record.Length - _offset);
                        Record.AsSpan(_offset, copied).CopyTo(buffer[written..]);
                        _offset += copied;
                        written += copied;
                        if (_offset == Record.Length) { _offset = 0; _record++; _separadorPendiente = _record < count; }
                    }
                    continue;
                }
                buffer[written++] = (byte)']';
                _finished = true;
            }
            BytesRead += written;
            PicoHeapObservado = Math.Max(PicoHeapObservado, GC.GetTotalMemory(forceFullCollection: false));
            return written;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Read(buffer.Span));
    }

    private sealed class GeneratedOversizeStream(int bytes) : Stream
    {
        private int _read;
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
            var count = Math.Min(Math.Min(buffer.Length, 8192), bytes - _read);
            if (count <= 0) return 0;
            buffer[..count].Fill((byte)' ');
            if (_read == 0) buffer[0] = (byte)'[';
            _read += count;
            if (_read == bytes) buffer[count - 1] = (byte)']';
            return count;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Read(buffer.Span));
    }

    private sealed class RootThenLargeStream : Stream
    {
        public int BytesRead { get; private set; }
        private bool _first = true;
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
            if (!_first || buffer.IsEmpty) return 0;
            _first = false;
            BytesRead++;
            buffer[0] = (byte)'{';
            return 1;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Read(buffer.Span));
    }
}
