using System.Net;
using System.Text;
using System.Text.Json;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public class PlaneaClienteTests
{
    private sealed class Handler(HttpResponseMessage response) : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) { Uri = request.RequestUri; return Task.FromResult(response); }
    }
    private static string Fixture => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "planea_muestra.json"));

    [Fact]
    public async Task ObtenerPeriodoAsync_LeeStreamYConstruyeRuta()
    {
        var handler = new Handler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(Fixture))) });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://planea.test/planea/index.php/apiroladoovr/") };
        var respuesta = await new PlaneaCliente(http).ObtenerPeriodoAsync("202701");
        Assert.Equal(respuesta.Resultado!.Count, respuesta.Total);
        Assert.Equal("https://planea.test/planea/index.php/apiroladoovr/periodo/202701", handler.Uri!.ToString());
    }
    [Fact]
    public async Task ObtenerPeriodoAsync_ErrorHttp_Propaga()
    {
        using var http = new HttpClient(new Handler(new HttpResponseMessage(HttpStatusCode.InternalServerError))) { BaseAddress = new Uri("https://planea.test/") };
        await Assert.ThrowsAsync<HttpRequestException>(() => new PlaneaCliente(http).ObtenerPeriodoAsync("202701"));
    }
    [Theory]
    [InlineData("{no json")]
    [InlineData("null")]
    public async Task ObtenerPeriodoAsync_RespuestaInvalida_Lanza(string json)
    {
        using var http = new HttpClient(new Handler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) })) { BaseAddress = new Uri("https://planea.test/") };
        await Assert.ThrowsAsync<PlaneaRespuestaInvalidaException>(() => new PlaneaCliente(http).ObtenerPeriodoAsync("202701"));
    }
}
