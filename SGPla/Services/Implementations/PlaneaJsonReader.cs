using System.Text.Json;
using SGPla.Models.DTOs.Integracion;

namespace SGPla.Services.Implementations;

/// <summary>Lee la sección horarios recorriendo en streaming el resto del sobre PLANEA.</summary>
internal sealed class PlaneaJsonReader(string periodoSolicitado)
{
    private JsonReaderState _state = new(new JsonReaderOptions { MaxDepth = 32 });
    private string? _propiedadActual;
    private bool _raizLeida;
    private bool _horariosEncontrados;
    private bool _horariosTerminados;
    private bool _periodoEncontrado;
    private readonly List<PlaneaRegistro> _registros = [];

    public IReadOnlyList<PlaneaRegistro> Registros => _registros;
    public bool HorariosEncontrados => _horariosEncontrados;
    public bool HorariosTerminados => _horariosTerminados;
    public bool PeriodoEncontrado => _periodoEncontrado;

    public int Procesar(ReadOnlySpan<byte> bytes, bool final, out bool incompleto)
    {
        var reader = new Utf8JsonReader(bytes, final, _state);
        incompleto = false;
        while (true)
        {
            var stateBeforeRead = reader.CurrentState;
            var consumedBeforeRead = checked((int)reader.BytesConsumed);
            bool tieneToken;
            try
            {
                tieneToken = reader.Read();
            }
            catch (JsonException)
            {
                if (!final)
                {
                    incompleto = true;
                    break;
                }
                throw;
            }
            if (!tieneToken) break;

            if (!_raizLeida)
            {
                if (reader.TokenType != JsonTokenType.StartObject || reader.CurrentDepth != 0)
                    throw new InvalidDataException("PLANEA debe devolver un objeto JSON con la sección horarios.");
                _raizLeida = true;
                continue;
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 1)
            {
                _propiedadActual = reader.GetString();
                continue;
            }

            if (reader.CurrentDepth == 1 && _propiedadActual == "periodo")
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new InvalidDataException("PLANEA no devolvió un periodo válido.");
                var periodo = reader.GetString();
                if (!string.Equals(periodo, periodoSolicitado, StringComparison.Ordinal))
                    throw new InvalidDataException("PLANEA devolvió un periodo distinto al solicitado.");
                _periodoEncontrado = true;
            }

            if (reader.CurrentDepth == 1 && _propiedadActual == "horarios"
                && reader.TokenType == JsonTokenType.StartArray)
            {
                _horariosEncontrados = true;
                continue;
            }

            if (_horariosEncontrados && !_horariosTerminados && reader.CurrentDepth == 2
                && reader.TokenType == JsonTokenType.StartObject)
            {
                try
                {
                    using var documento = JsonDocument.ParseValue(ref reader);
                    _registros.Add(LeerRegistro(documento.RootElement));
                }
                catch (JsonException) when (!final)
                {
                    _state = stateBeforeRead;
                    incompleto = true;
                    return consumedBeforeRead;
                }
                continue;
            }

            if (_horariosEncontrados && !_horariosTerminados && reader.CurrentDepth == 2
                && reader.TokenType != JsonTokenType.EndArray)
                throw new InvalidDataException("La respuesta de PLANEA contiene un elemento de horarios que no es objeto.");

            if (reader.CurrentDepth == 1 && _propiedadActual == "horarios"
                && reader.TokenType == JsonTokenType.EndArray)
            {
                _horariosTerminados = true;
                continue;
            }

            if (reader.TokenType is JsonTokenType.EndArray or JsonTokenType.EndObject && reader.CurrentDepth <= 1)
                _propiedadActual = null;
        }

        _state = reader.CurrentState;
        return checked((int)reader.BytesConsumed);
    }

    private static PlaneaRegistro LeerRegistro(JsonElement objeto)
    {
        if (objeto.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("La respuesta de PLANEA contiene un elemento de horarios que no es objeto.");
        var prefijos = new[] { (1, "LUN"), (2, "MAR"), (3, "MIE"), (4, "JUE"), (5, "VIE"), (6, "SAB") };
        var horarios = prefijos.Select(d => new PlaneaHorarioDia((byte)d.Item1,
            ObtenerEscalar(objeto, $"{d.Item2}_INI"), ObtenerEscalar(objeto, $"{d.Item2}_FIN"))).ToArray();
        return new PlaneaRegistro(ObtenerEscalar(objeto, "PERIODO"), ObtenerEscalar(objeto, "NRC"),
            ObtenerEscalar(objeto, "ID_DOCENTE"), ObtenerEscalar(objeto, "NOMBRE"), ObtenerEscalar(objeto, "EDIFICIO"),
            ObtenerEscalar(objeto, "AULA"), ObtenerEscalar(objeto, "FECHA_INICIO"), ObtenerEscalar(objeto, "FECHA_FIN"),
            horarios, ObtenerEscalar(objeto, "IND_DOCENTE"), ObtenerEscalar(objeto, "IND_PRINCIPAL"),
            ObtenerEscalar(objeto, "RESPONSABILIDAD"));
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
}
