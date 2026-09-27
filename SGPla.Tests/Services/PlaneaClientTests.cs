using System.Net;
using System.IO.Compression;
using System.Text;
using System.Net.Sockets;
using Xunit.Abstractions;
using Microsoft.Extensions.Options;
using SGPla.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel;
using SGPla.Models.DTOs.Integracion;

namespace SGPla.Tests.Services;

public sealed class PlaneaClientTests
{
    private readonly ITestOutputHelper _output;

    public PlaneaClientTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ObtenerProgramaciones_lee_fixture_anonimizada_con_forma_real()
    {
        var body = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "202701_muestra.json"));
        var cliente = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, "application/json", body));
        var registros = await cliente.ObtenerProgramacionesAsync("202701");
        Assert.Equal(3, registros.Count);
        Assert.Equal("02", registros[1].IndDocente);
        Assert.Equal("SI", registros[1].IndPrincipal);
    }

    [Fact]
    public async Task ObtenerProgramaciones_falla_si_falta_horarios_o_no_es_arreglo_o_periodo_no_coincide()
    {
        var casos = new (string Body, string Mensaje)[]
        {
            ("{\"periodo\":\"202701\"}", "PLANEA no devolvió la sección horarios."),
            ("{\"periodo\":\"202701\",\"horarios\":{}}", "PLANEA no devolvió la sección horarios."),
            ("{\"periodo\":\"202601\",\"horarios\":[]}", "PLANEA devolvió un periodo distinto al solicitado.")
        };
        foreach (var (body, mensaje) in casos)
        {
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => CrearCliente(
                new RespuestaHandler(HttpStatusCode.OK, "application/json", body)).ObtenerProgramacionesAsync("202701"));
            Assert.Equal(mensaje, error.Message);
        }
    }

    [Fact]
    public async Task ObtenerProgramaciones_lee_seccion_horarios_del_objeto_y_ignora_campos_desconocidos()
    {
        const string body = """
            {"periodo":"202601","total":1,"resultado":[{"radoc_id":"anon"}],"horarios":[{"PERIODO":"202601","NRC":"A12B3","ID_DOCENTE":123,"NOMBRE":"Docente Uno","FECHA_INICIO":"2026-01-01","FECHA_FIN":"2026-06-30","LUN_INI":"0800","LUN_FIN":"0859","IND_DOCENTE":"01","IND_PRINCIPAL":"SI","RESPONSABILIDAD":"DOCENTE","HRS_SEMANA":99}]}
            """;
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, "application/json", body));

        var registro = Assert.Single(await client.ObtenerProgramacionesAsync("202601"));

        Assert.Equal("202601", registro.Periodo);
        Assert.Equal("123", registro.NumeroPersonal);
        Assert.Equal("0800", registro.Horarios[0].Inicio);
        Assert.Null(registro.Horarios[1].Inicio);
        Assert.Equal("01", registro.IndDocente);
        Assert.Equal("SI", registro.IndPrincipal);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_respuesta_http_fallida()
    {
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.ServiceUnavailable, "application/json", "{}"));
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
        var handler = new CapturaHandler(HttpStatusCode.OK, "{}");
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
        var handler = new CapturaHandler(HttpStatusCode.OK, "{}");
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
        var handler = new CapturaHandler(HttpStatusCode.OK, "{}");
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
    public async Task ObtenerProgramaciones_descarta_resultado_grande_sin_asignar_memoria_proporcional()
    {
        // 700 mil filas JSON simulan más de 45 MB en resultado sin almacenar el body.
        using var bodyStream = new GeneratedIgnoredResultStream(700_000);
        var content = new StreamContent(bodyStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var client = CrearCliente(new ContenidoHandler(content));
        var asignadoAntes = GC.GetAllocatedBytesForCurrentThread();
        var registros = await client.ObtenerProgramacionesAsync("202701");
        var asignado = GC.GetAllocatedBytesForCurrentThread() - asignadoAntes;
        _output.WriteLine($"resultado grande ignorado: {bodyStream.BytesRead:N0} bytes leídos; asignado durante la lectura={asignado:N0} bytes.");
        Assert.Single(registros);
        Assert.True(asignado < 15 * 1024 * 1024,
            $"El parser asignó {asignado:N0} bytes al saltar un resultado de {bodyStream.BytesRead:N0} bytes.");
    }

    [Fact]
    public async Task Humo_lee_muestra_local_si_se_configura()
    {
        var ruta = Environment.GetEnvironmentVariable("SGPLA_PLANEA_SAMPLE_PATH");
        if (string.IsNullOrWhiteSpace(ruta)) return;
        Assert.True(File.Exists(ruta), "SGPLA_PLANEA_SAMPLE_PATH no apunta a un archivo existente.");
        await using var archivo = File.OpenRead(ruta);
        using var contenido = new StreamContent(archivo);
        contenido.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var cliente = CrearCliente(new ContenidoHandler(contenido));
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var heapAntes = GC.GetTotalMemory(forceFullCollection: true);
        var registros = await cliente.ObtenerProgramacionesAsync("202701");
        sw.Stop();
        var heapDespues = GC.GetTotalMemory(forceFullCollection: true);
        var nrcs = registros.Where(x => !string.IsNullOrWhiteSpace(x.Nrc)).Select(x => x.Nrc!).Distinct(StringComparer.Ordinal).Count();
        var sinSesiones = registros.Count(x => x.Horarios.All(h => string.IsNullOrWhiteSpace(h.Inicio) && string.IsNullOrWhiteSpace(h.Fin)));
        var sesionesInformadas = registros.Sum(x => x.Horarios.Count(h => !string.IsNullOrWhiteSpace(h.Inicio) && !string.IsNullOrWhiteSpace(h.Fin)));
        _output.WriteLine($"PLANEA sample parsed: registros={registros.Count}; NRC distintos={nrcs}; franjas horarias informadas={sesionesInformadas}; sin sesión={sinSesiones}; tiempo={sw.ElapsedMilliseconds} ms; heap antes={heapAntes:N0}; heap después GC={heapDespues:N0} bytes.");

        var cadena = Environment.GetEnvironmentVariable("SGPLA_SQLSERVER_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(cadena))
        {
            _output.WriteLine("NRC sin programación/sesiones generadas/advertencias: no medidos; falta SGPLA_SQLSERVER_TEST_CONNECTION.");
            return;
        }
        var options = new DbContextOptionsBuilder<SgplaDbContext>().UseSqlServer(cadena).Options;
        await using var db = new SgplaDbContext(options);
        var periodo = await db.PeriodosEscolares.AsNoTracking().SingleOrDefaultAsync(x => x.Clave == "202701" && x.FechaEliminacion == null);
        if (periodo is null)
        {
            _output.WriteLine("No se pudo completar la comparación del validador: la base de pruebas no contiene el periodo 202701.");
            return;
        }
        var programaciones = await (from p in db.ProgramacionAcademicas.AsNoTracking()
                                    join ee in db.ExperienciasEducativas.AsNoTracking() on p.ExperienciaEducativaId equals ee.Id
                                    join plan in db.PlanesEstudios.AsNoTracking() on ee.PlanEstudiosId equals plan.Id
                                    join programa in db.ProgramasEducativos.AsNoTracking() on plan.ProgramaEducativoId equals programa.Id
                                    join entidad in db.EntidadAcademicas.AsNoTracking() on programa.EntidadAcademicaId equals entidad.Id
                                    where p.PeriodoEscolarId == periodo!.Id && p.FechaEliminacion == null
                                          && ee.FechaEliminacion == null && plan.FechaEliminacion == null
                                          && programa.FechaEliminacion == null && entidad.FechaEliminacion == null
                                    select new { p.Nrc, p.Id }).ToListAsync();
        var porNrc = programaciones.GroupBy(x => x.Nrc, StringComparer.Ordinal).ToDictionary(x => x.Key, x => x.Select(v => v.Id).Single(), StringComparer.Ordinal);
        var snapshot = new PlaneaSnapshotValidator().Validar("202701", periodo.FechaInicio, periodo.FechaFin, porNrc, registros);
        _output.WriteLine($"PLANEA sample validation: sesiones={snapshot.Sesiones.Count}; ignorados={snapshot.RegistrosIgnorados}; NRC sin programación={snapshot.NrcSinProgramacion}; duplicados={snapshot.DuplicadosDescartados}; advertencias={snapshot.Advertencias}.");
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
    [InlineData("{\"periodo\":\"202701\",\"horarios\":[{\"PERIODO\":{}}]}", "PLANEA devolvió un valor no escalar para PERIODO.")]
    [InlineData("{\"periodo\":\"202701\",\"horarios\":[{", "PLANEA devolvió JSON malformado.")]
    public async Task ObtenerProgramaciones_conserva_errores_de_json(string body, string mensaje)
    {
        var client = CrearCliente(new RespuestaHandler(HttpStatusCode.OK, "application/json", body));

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal(mensaje, error.Message);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_raiz_no_objeto_antes_de_leer_el_cuerpo()
    {
        var stream = new RootThenLargeStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var client = CrearCliente(new ContenidoHandler(content));

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202701"));

        Assert.Equal("PLANEA debe devolver un objeto JSON con la sección horarios.", error.Message);
        Assert.Equal(1, stream.BytesRead);
    }

    [Fact]
    public async Task ObtenerProgramaciones_acepta_respuesta_gzip_despues_de_descompresion_http()
    {
        var original = Encoding.UTF8.GetBytes("{\"periodo\":\"202701\",\"horarios\":[{\"PERIODO\":\"202701\"}]}");
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
        private static readonly byte[] Prefix = Encoding.UTF8.GetBytes("{\"periodo\":\"202701\",\"resultado\":[],\"horarios\":[");
        private static readonly byte[] Suffix = Encoding.UTF8.GetBytes("]}");
        private static readonly byte[] Record = Encoding.UTF8.GetBytes("{\"PERIODO\":\"202701\",\"NRC\":\"A12B3\"}");
        private int _record = -1;
        private int _offset;
        private bool _finished;
        private bool _separadorPendiente;
        private int _prefixOffset;
        private int _suffixOffset;
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
                if (_prefixOffset < Prefix.Length)
                {
                    var copied = Math.Min(buffer.Length - written, Prefix.Length - _prefixOffset);
                    Prefix.AsSpan(_prefixOffset, copied).CopyTo(buffer[written..]);
                    _prefixOffset += copied;
                    written += copied;
                    continue;
                }
                if (_record == -1) _record = 0;
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
                var suffixCopied = Math.Min(buffer.Length - written, Suffix.Length - _suffixOffset);
                Suffix.AsSpan(_suffixOffset, suffixCopied).CopyTo(buffer[written..]);
                _suffixOffset += suffixCopied;
                written += suffixCopied;
                if (_suffixOffset == Suffix.Length) _finished = true;
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
            if (_read == 0) buffer[0] = (byte)'{';
            _read += count;
            if (_read == bytes) buffer[count - 1] = (byte)']';
            return count;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Read(buffer.Span));
    }

    private sealed class GeneratedIgnoredResultStream(int count) : Stream
    {
        private static readonly byte[] Prefix = Encoding.UTF8.GetBytes("{\"periodo\":\"202701\",\"resultado\":[");
        private static readonly byte[] Element = Encoding.UTF8.GetBytes("{\"radoc_id\":\"12345678901234567890123456789012345678901234567890\"}");
        private static readonly byte[] Suffix = Encoding.UTF8.GetBytes("],\"horarios\":[{\"PERIODO\":\"202701\",\"NRC\":\"A12B3\",\"FECHA_INICIO\":\"2027-01-01\",\"FECHA_FIN\":\"2027-06-30\"}]}");
        private int _fase;
        private int _posicion;
        private int _indice;
        private bool _comaPendiente;
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
            if (buffer.IsEmpty || _fase == 3) return 0;
            var escrito = 0;
            while (escrito < buffer.Length && _fase < 3)
            {
                if (_fase == 0)
                {
                    var n = Math.Min(buffer.Length - escrito, Prefix.Length - _posicion);
                    Prefix.AsSpan(_posicion, n).CopyTo(buffer[escrito..]);
                    escrito += n;
                    _posicion += n;
                    if (_posicion == Prefix.Length) { _fase = 1; _posicion = 0; }
                    continue;
                }
                if (_fase == 1)
                {
                    if (_indice == count) { _fase = 2; _posicion = 0; continue; }
                    if (_comaPendiente)
                    {
                        buffer[escrito++] = (byte)',';
                        _comaPendiente = false;
                        continue;
                    }
                    var n = Math.Min(buffer.Length - escrito, Element.Length - _posicion);
                    Element.AsSpan(_posicion, n).CopyTo(buffer[escrito..]);
                    escrito += n;
                    _posicion += n;
                    if (_posicion == Element.Length) { _indice++; _posicion = 0; _comaPendiente = _indice < count; }
                    continue;
                }
                var copiadas = Math.Min(buffer.Length - escrito, Suffix.Length - _posicion);
                Suffix.AsSpan(_posicion, copiadas).CopyTo(buffer[escrito..]);
                escrito += copiadas;
                _posicion += copiadas;
                if (_posicion == Suffix.Length) _fase = 3;
            }
            BytesRead += escrito;
            return escrito;
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
            buffer[0] = (byte)'[';
            return 1;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Read(buffer.Span));
    }
}
