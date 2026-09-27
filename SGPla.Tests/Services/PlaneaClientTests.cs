using System.Net;
using Microsoft.Extensions.Options;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class PlaneaClientTests
{
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
}
