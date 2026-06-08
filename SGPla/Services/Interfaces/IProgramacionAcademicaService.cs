using HtmlAgilityPack;
using SGPla.Models.DTOs.Oferta;
using System.Text;
using ExcelDataReader;


using System.Text.RegularExpressions;


public class CargaConOfertaDTO
{
    public string NumeroPersonal { get; set; } = string.Empty;
    public string NombreDocente { get; set; } = string.Empty;
    public string Plaza { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string TipoContratacion { get; set; } = string.Empty;
    public string Nrc { get; set; } = string.Empty;
    public string ExperienciaEducativa { get; set; } = string.Empty;
    public int HorasContacto { get; set; }
    public int HorasPago { get; set; }
    public string MotivoRh { get; set; } = string.Empty;
    public string IndActDocente { get; set; } = string.Empty;
    public bool Imparte { get; set; }

    public string? Programa { get; set; }
    public string? NpOferta { get; set; }   // NP de la oferta (puede diferir)
    public string? DocenteOferta { get; set; }
    public bool NrcEncontrado { get; set; }
}


namespace SGPla.Models.DTOs.Cargas
{

    public class CargaItemDTO
    {
        public string Nrc { get; set; } = string.Empty;

        public string ExperienciaEducativa { get; set; } = string.Empty;

        public int HorasContacto { get; set; }

        public int HorasPago { get; set; }

        public string ClaveProgramatica { get; set; } = string.Empty;

        public string MotivoRh { get; set; } = string.Empty;

        public string IndActDocente { get; set; } = string.Empty;

        public bool Imparte { get; set; }

        public int HorasExcCarga { get; set; }

        public string? NumPersonalSuplente { get; set; }

        public string? NombreSuplente { get; set; }
    }

  
    public class DocenteCargaDTO
    {
        public string NumeroPersonal { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Antiguedad { get; set; } = string.Empty;

        public string Plaza { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Puesto { get; set; } = string.Empty;

        public string TipoContratacion { get; set; } = string.Empty;

        public int TotalHoras { get; set; }

        public List<CargaItemDTO> Materias { get; set; } = new();
    }


    public class CargasAcademicasDTO
    {
        public string Periodo { get; set; } = string.Empty;

        public List<DocenteCargaDTO> Docentes { get; set; } = new();
    }
}



namespace SGPla.Parsers
{
    using SGPla.Models.DTOs.Cargas;

    public static class CargasParser
    {
        private static readonly string[] ExpectedHeaders =
        {
            "NRC", "Experiencia Educativa", "Horas Contacto", "Horas Pago",
            "Clave Programática", "Motivo RH", "Ind. Act. Docente",
            "Imparte", "Horas Exc. Carga", "N.P.", "Nombre del suplente / interino"
        };

        public static CargasAcademicasDTO Parse(Stream stream)
        {
            Span<byte> magic = stackalloc byte[8];
            stream.Read(magic);
            stream.Position = 0;

            bool isRealExcel =
                (magic[0] == 0xD0 && magic[1] == 0xCF) ||   // CFBF (xls binario)
                (magic[0] == 0x50 && magic[1] == 0x4B);      // ZIP  (xlsx)

            if (isRealExcel)
                throw new NotSupportedException(
                    "El archivo es un Excel binario real. " +
                    "Este parser sólo procesa el HTML exportado por el módulo UV.");

            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var html = reader.ReadToEnd();

            if (html.Contains("<frameset", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "El archivo fue guardado como 'Página web con marcos'. " +
                    "Descárgalo directamente desde el módulo UV.");

            return ParseHtml(html);
        }


        private static CargasAcademicasDTO ParseHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var result = new CargasAcademicasDTO
            {
                Periodo = ExtractPeriodo(doc)
            };

           
            var docenteNodes = doc.DocumentNode
                .SelectNodes("//span[contains(@class,'contenedorNumpersonal')]");

            if (docenteNodes is null)
                return result;

            foreach (var spanNode in docenteNodes)
            {
                var docente = ParseDocente(spanNode);
                result.Docentes.Add(docente);
            }

            return result;
        }


        private static string ExtractPeriodo(HtmlDocument doc)
        {
            var periodoNode = doc.DocumentNode
                .SelectSingleNode("//td[contains(translate(text(),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ'),'PERIODO')]");

            if (periodoNode is null)
                return string.Empty;

            var texto = periodoNode.InnerText.Trim();
            var match = Regex.Match(texto, @"PERIODO\s+(.+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : texto;
        }


        private static DocenteCargaDTO ParseDocente(HtmlNode spanNumpersonal)
        {
            var docente = new DocenteCargaDTO();


          
            var nombreNode = spanNumpersonal
                .SelectSingleNode(".//span[contains(@class,'nombreAcademico')]");
            if (nombreNode is not null)
            {
                var raw = HtmlEntity.DeEntitize(nombreNode.InnerText).Trim();
                var dashIdx = raw.IndexOf('-');
                if (dashIdx > 0)
                {
                    docente.NumeroPersonal = raw[..dashIdx].Trim();
                    docente.Nombre = raw[(dashIdx + 1)..].Trim();

                }
                else
                {
                    docente.Nombre = raw;
                }
            }

          
            var current = spanNumpersonal.NextSibling;

            while (current != null)
            {
                if (current.Name == "span")
                {
                    var cls = current.GetAttributeValue("class", "");

                    if (cls.Contains("contenedorNumpersonal"))
                        break;

                    if (cls.Contains("contenedorPlaza"))
                        docente.Plaza = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");

                    else if (cls.Contains("contenedorCategoria"))
                        docente.Categoria = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");

                    else if (cls.Contains("contenedorPuesto"))
                        docente.Puesto = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");

                    else if (cls.Contains("contenedorTipocontratacion"))
                        docente.TipoContratacion = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");

                    else if (string.IsNullOrEmpty(docente.Antiguedad))
                        docente.Antiguedad = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");
                }

                if (current.Name == "table")
                {
                    ParseTable(current, docente);
                }

                current = current.NextSibling;
            }
    

            return docente;
        }

        private static void ParseTable(HtmlNode table, DocenteCargaDTO docente)
        {
            var rows = table.SelectNodes(".//tr");
            if (rows is null) return;

            bool inDataSection = false;

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td|th")
                    ?.Select(n => HtmlEntity.DeEntitize(n.InnerText).Trim())
                    .ToArray()
                    ?? Array.Empty<string>();

                if (cells.Length == 0) continue;

                if (IsHeaderRow(cells))
                {
                    inDataSection = true;
                    continue;
                }

                if (!inDataSection) continue;

                if (cells.Length >= 4 && cells[1] == "Total de horas:")
                {
                    if (int.TryParse(cells[2], out var horas))
                        docente.TotalHoras += horas;

                    break;
                }

                if (cells.Length >= 8 && IsNrcRow(cells[0]))
                {
                    docente.Materias.Add(ParseCargaItem(cells));
                }
            }
        }

        private static CargaItemDTO ParseCargaItem(string[] cells)
        {
          

            return new CargaItemDTO
            {
                Nrc = cells.ElementAtOrDefault(0) ?? string.Empty,
                ExperienciaEducativa = cells.ElementAtOrDefault(1) ?? string.Empty,
                HorasContacto = ParseInt(cells.ElementAtOrDefault(2)),
                HorasPago = ParseInt(cells.ElementAtOrDefault(3)),
                ClaveProgramatica = cells.ElementAtOrDefault(4) ?? string.Empty,
                MotivoRh = cells.ElementAtOrDefault(5) ?? string.Empty,
                IndActDocente = cells.ElementAtOrDefault(6) ?? string.Empty,
                Imparte = string.Equals(cells.ElementAtOrDefault(7), "SI",
                                         StringComparison.OrdinalIgnoreCase),
                HorasExcCarga = ParseInt(cells.ElementAtOrDefault(8)),
                NumPersonalSuplente = NullIfEmpty(cells.ElementAtOrDefault(9)),
                NombreSuplente = NullIfEmpty(cells.ElementAtOrDefault(10)),
            };
        }


        private static bool IsHeaderRow(string[] cells) =>
            cells.Length >= 2 && cells[0] == "NRC" && cells[1] == "Experiencia Educativa";

        private static bool IsNrcRow(string? value) =>
            !string.IsNullOrWhiteSpace(value) && value.All(char.IsDigit);

        private static int ParseInt(string? value) =>
            int.TryParse(value, out var n) ? n : 0;

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static string InnerTextOf(HtmlNode root, string xpath)
        {
            var node = root.SelectSingleNode(xpath);
            return node is null
                ? string.Empty
                : HtmlEntity.DeEntitize(node.InnerText).Trim();
        }
    }
}
public static class DescargasParser
{
    public static List<OfertaDTO> Parse(Stream stream, string fileName)
    {
        Span<byte> magic = stackalloc byte[8];
        stream.Read(magic);
        stream.Position = 0;


        bool isRealExcel = (magic[0] == 0xD0 && magic[1] == 0xCF) || 
                           (magic[0] == 0x50 && magic[1] == 0x4B);   

        if (isRealExcel)
            return ParseExcel(stream);

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var html = reader.ReadToEnd();

        if (html.Contains("<frameset", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "El archivo fue guardado por Excel como 'Página web con marcos'. " +
                "Por favor, guárdalo como .xlsx (Libro de Excel) e inténtalo de nuevo.");

        return ParseHtml(html);
    }

    public static List<OfertaDTO> ParseHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var dataTable = FindDataTable(doc)
            ?? throw new InvalidDataException("No se encontró la tabla de datos. Verifica que el archivo sea el correcto.");

        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var result = new List<OfertaDTO>();
        var currentProg = "";

        foreach (var tr in dataTable.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
        {
            var cells = tr.SelectNodes(".//td|.//th");
            if (cells == null) continue;

            var vals = cells.Select(c => c.InnerText.Trim()).ToArray();
            if (vals.Length == 0) continue;

            if (vals[0].Equals("NRC", StringComparison.OrdinalIgnoreCase))
            {
                colMap.Clear();
                for (int i = 0; i < vals.Length; i++)
                    colMap[Normalize(vals[i])] = i;
                continue;
            }

            if (vals.Length == 1 && vals[0].StartsWith("Programa:", StringComparison.OrdinalIgnoreCase))
            {
                currentProg = vals[0]["Programa:".Length..].Trim();
                continue;
            }

            if (colMap.Count == 0 || vals.Length < colMap.Count) continue;

            result.Add(BuildClase(currentProg, colMap, vals));
        }

        return result;
    }

    private static List<OfertaDTO> ParseExcel(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var excelReader = ExcelReaderFactory.CreateReader(stream);
        var result = new List<OfertaDTO>();
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var currentProg = "";

        do
        {
            while (excelReader.Read())
            {
                var vals = Enumerable.Range(0, excelReader.FieldCount)
                    .Select(i => excelReader.GetValue(i)?.ToString()?.Trim() ?? "")
                    .ToArray();

                if (vals.All(string.IsNullOrWhiteSpace)) continue;

                if (vals[0].Equals("NRC", StringComparison.OrdinalIgnoreCase))
                {
                    colMap.Clear();
                    for (int i = 0; i < vals.Length; i++)
                        colMap[Normalize(vals[i])] = i;
                    continue;
                }

                if (vals[0].StartsWith("Programa:", StringComparison.OrdinalIgnoreCase))
                {
                    currentProg = vals[0]["Programa:".Length..].Trim();
                    continue;
                }

                if (colMap.Count == 0) continue;

                if (!int.TryParse(vals[0], out _)) continue;

                result.Add(BuildClase(currentProg, colMap, vals));
            }
        }
        while (excelReader.NextResult());

        return result;
    }
    private static HtmlNode? FindDataTable(HtmlDocument doc)
    {
        foreach (var table in doc.DocumentNode.SelectNodes("//table") ?? Enumerable.Empty<HtmlNode>())
        {
            var firstRow = table.SelectSingleNode(".//tr");
            var firstCell = firstRow?.SelectSingleNode(".//td|.//th");
            if (firstCell?.InnerText.Trim().Equals("NRC", StringComparison.OrdinalIgnoreCase) == true)
                return table;
        }
        return null;
    }

    private static OfertaDTO BuildClase(string programa, Dictionary<string, int> colMap, string[] vals)
    {
        return new OfertaDTO
        {
            Programa = programa,
            NRC = Get(colMap, vals, "NRC"),
            ExperienciaEducativa = Get(colMap, vals, "EXPERIENCIAEDUCATIVA"),
            HorasPago = int.TryParse(Get(colMap, vals, "HORASPAGO"), out var hp) ? hp : 0,
            Plaza = Get(colMap, vals, "PLAZA"),
            Lunes = ParseHorario(Get(colMap, vals, "LUNES")),
            Martes = ParseHorario(Get(colMap, vals, "MARTES")),
            Miercoles = ParseHorario(Get(colMap, vals, "MIERCOLES")),
            Jueves = ParseHorario(Get(colMap, vals, "JUEVES")),
            Viernes = ParseHorario(Get(colMap, vals, "VIERNES")),
            Sabado = ParseHorario(Get(colMap, vals, "SABADO")),
            NP = Nullify(Get(colMap, vals, "NP")),
            NombreDocente = Nullify(Get(colMap, vals, "NOMBRE")),
            TipoIngreso = Nullify(Get(colMap, vals, "TIPODEINGRESO")),
            TC = Nullify(Get(colMap, vals, "TC")),
        };
    }

    private static string Get(Dictionary<string, int> map, string[] vals, string key)
    {
        if (map.TryGetValue(key, out var idx) && idx < vals.Length)
            return vals[idx];
        return "";
    }

    private static HorarioDia? ParseHorario(string raw)
    {
        raw = raw.Trim();
        if (string.IsNullOrEmpty(raw) || raw == "----") return null;

        var idx = raw.IndexOf('-', 3);
        if (idx < 0) return null;
        if (!TimeSpan.TryParse(raw[..idx], out var inicio)) return null;
        if (!TimeSpan.TryParse(raw[(idx + 1)..], out var fin)) return null;

        return new HorarioDia { Inicio = inicio, Fin = fin };
    }

    private static string Normalize(string s) =>
        s.ToUpperInvariant().Replace(".", "").Replace(" ", "").Replace("\t", "");

    private static string? Nullify(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public interface IProgramacionAcademicaService
{
    Task<List<OfertaDTO>> ProcesarArchivoAsync(IFormFile archivo);

    Task<bool> GuardarOfertasAsync(List<OfertaDTO> ofertas, int idPeriodo);

    Task<List<CargaConOfertaDTO>> ProcesarCargasAsync(
    IFormFile archivoCarga,
    List<OfertaDTO> ofertasEnSesion);
}

