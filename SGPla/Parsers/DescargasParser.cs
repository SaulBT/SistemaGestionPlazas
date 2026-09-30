using ExcelDataReader;
using HtmlAgilityPack;
using SGPla.Models.DTOs.Oferta;
using System.Text;

namespace SGPla.Parsers
{

    public static class DescargasParser
    {
        public static List<OfertaDTO> Parse(Stream stream, string fileName)
        {
            Span<byte> magic = stackalloc byte[8];
            stream.ReadExactly(magic);
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
                Incluida = true
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
}
