using System.Net;
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
        var client = new PlaneaClient(new HttpClient(new RespuestaHandler(HttpStatusCode.OK, "application/json", body)));

        var registro = Assert.Single(await client.ObtenerProgramacionesAsync("202601"));

        Assert.Equal("202601", registro.Periodo);
        Assert.Equal("123", registro.NumeroPersonal);
        Assert.Equal("0800", registro.Horarios[0].Inicio);
        Assert.Null(registro.Horarios[1].Inicio);
    }

    [Fact]
    public async Task ObtenerProgramaciones_rechaza_respuesta_http_fallida()
    {
        var client = new PlaneaClient(new HttpClient(new RespuestaHandler(HttpStatusCode.ServiceUnavailable, "application/json", "[]")));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.ObtenerProgramacionesAsync("202601"));
    }

    [Theory]
    [InlineData("text/html", "<html>maintenance</html>")]
    [InlineData("application/json", "{}")]
    [InlineData("application/json", "[]")]
    public async Task ObtenerProgramaciones_rechaza_respuestas_sin_snapshot_json_valido(string mediaType, string body)
    {
        var client = new PlaneaClient(new HttpClient(new RespuestaHandler(HttpStatusCode.OK, mediaType, body)));
        await Assert.ThrowsAsync<InvalidDataException>(() => client.ObtenerProgramacionesAsync("202601"));
    }

    private sealed class RespuestaHandler(HttpStatusCode codigo, string mediaType, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("https://planea.uv.mx/planea/index.php/apiroladoovr/periodo/202601", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(codigo)
            {
                Content = new StringContent(body, null, new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType))
            });
        }
    }
}
