using System.Text.Json;
using System.Text.Json.Serialization;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public sealed class PlaneaCliente : IPlaneaCliente
    {
        private static readonly JsonSerializerOptions OpcionesJson = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        private readonly HttpClient _httpClient;
        public PlaneaCliente(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken = default)
        {
            using var respuesta = await _httpClient.GetAsync($"periodo/{Uri.EscapeDataString(codigoPeriodo.Trim())}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            respuesta.EnsureSuccessStatusCode();
            await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
            try
            {
                return await JsonSerializer.DeserializeAsync<PlaneaRespuesta>(stream, OpcionesJson, cancellationToken)
                    ?? throw new PlaneaRespuestaInvalidaException("PLANEA devolvió una respuesta vacía.");
            }
            catch (JsonException ex)
            {
                throw new PlaneaRespuestaInvalidaException($"PLANEA devolvió un JSON inválido: {ex.Message}");
            }
        }
    }
}
