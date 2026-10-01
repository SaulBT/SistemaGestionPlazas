using System.Text.Json.Serialization;

namespace SGPla.Models.DTOs.Planea
{
    public sealed class PlaneaRespuesta
    {
        [JsonPropertyName("periodo")] public string? Periodo { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
        [JsonPropertyName("resultado")] public List<PlaneaSeccion>? Resultado { get; set; }
        [JsonPropertyName("horarios")] public List<PlaneaHorario>? Horarios { get; set; }
    }
    public sealed class PlaneaSeccion
    {
        [JsonPropertyName("radoc_nrc")] public string? Nrc { get; set; }
        [JsonPropertyName("radoc_materia")] public string? Materia { get; set; }
        [JsonPropertyName("radoc_curso")] public string? Curso { get; set; }
        [JsonPropertyName("sec_programa")] public string? CodigoPlan { get; set; }
        [JsonPropertyName("sec_titulo")] public string? Titulo { get; set; }
        [JsonPropertyName("sec_campus")] public string? Campus { get; set; }
        [JsonPropertyName("nivel")] public string? Nivel { get; set; }
        [JsonPropertyName("Region")] public string? Region { get; set; }
        [JsonPropertyName("Area")] public string? Area { get; set; }
        [JsonPropertyName("ID_TITULAR")] public string? NumeroPersonalDocente { get; set; }
        [JsonPropertyName("radoc_nombre")] public string? NombreDocente { get; set; }
        [JsonPropertyName("IND_IMPARTE")] public string? Imparte { get; set; }
    }
    public sealed class PlaneaHorario
    {
        [JsonPropertyName("NRC")] public string? Nrc { get; set; }
        [JsonPropertyName("rhs_id")] public string? IdHorario { get; set; }
        [JsonPropertyName("EDIFICIO")] public string? Edificio { get; set; }
        [JsonPropertyName("AULA")] public string? Aula { get; set; }
        [JsonPropertyName("FECHA_INICIO")] public string? FechaInicio { get; set; }
        [JsonPropertyName("FECHA_FIN")] public string? FechaFin { get; set; }
        [JsonPropertyName("LUN_INI")] public string? LunIni { get; set; }
        [JsonPropertyName("LUN_FIN")] public string? LunFin { get; set; }
        [JsonPropertyName("MAR_INI")] public string? MarIni { get; set; }
        [JsonPropertyName("MAR_FIN")] public string? MarFin { get; set; }
        [JsonPropertyName("MIE_INI")] public string? MieIni { get; set; }
        [JsonPropertyName("MIE_FIN")] public string? MieFin { get; set; }
        [JsonPropertyName("JUE_INI")] public string? JueIni { get; set; }
        [JsonPropertyName("JUE_FIN")] public string? JueFin { get; set; }
        [JsonPropertyName("VIE_INI")] public string? VieIni { get; set; }
        [JsonPropertyName("VIE_FIN")] public string? VieFin { get; set; }
        [JsonPropertyName("SAB_INI")] public string? SabIni { get; set; }
        [JsonPropertyName("SAB_FIN")] public string? SabFin { get; set; }
    }
}
