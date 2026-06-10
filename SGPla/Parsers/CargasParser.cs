using ExcelDataReader;
using HtmlAgilityPack;
using SGPla.Models.DTOs.ProgramacionAcademica;
using System.Text;
using System.Text.RegularExpressions;

namespace SGPla.Parsers
{

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
            stream.ReadExactly(magic);
            stream.Position = 0;

            bool isRealExcel =
                (magic[0] == 0xD0 && magic[1] == 0xCF) ||
                (magic[0] == 0x50 && magic[1] == 0x4B);

            if (isRealExcel)
                return ParseExcel(stream);

            using var reader = new StreamReader(stream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var html = reader.ReadToEnd();

            if (html.Contains("<frameset", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    "El archivo fue guardado como 'Página web con marcos'. " +
                    "Descárgalo directamente desde el módulo UV.");

            return ParseHtml(html);
        }

        private static CargasAcademicasDTO ParseExcel(Stream stream)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var result = new CargasAcademicasDTO();
            DocenteCargaDTO? docenteActual = null;
            bool inDataSection = false;

            string plazaActual = "", categoriaActual = "", puestoActual = "", tipoContActual = "";

            using var excelReader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                while (excelReader.Read())
                {
                    var vals = Enumerable.Range(0, excelReader.FieldCount)
                        .Select(i => excelReader.GetValue(i)?.ToString()?.Trim() ?? "")
                        .ToArray();

                    var firstCell = vals.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "";
                    if (string.IsNullOrWhiteSpace(firstCell)) continue;

                    var firstNorm = NormalizeSpaces(firstCell);

                    if (string.IsNullOrEmpty(result.Periodo))
                    {
                        var periodoCell = vals
                            .Select(v => NormalizeSpaces(v))
                            .FirstOrDefault(v => v.StartsWith("PERIODO", StringComparison.OrdinalIgnoreCase));
                        if (periodoCell != null)
                        {
                            var m = Regex.Match(periodoCell, @"PERIODO\s+(.+)", RegexOptions.IgnoreCase);
                            result.Periodo = m.Success ? m.Groups[1].Value.Trim() : periodoCell;
                            continue;
                        }
                    }

                    var mAcademico = Regex.Match(firstNorm,
                        @"Acad[eé]mico:\s*(\d+)-(.+?)\s+Antig[üu]edad:\s*(.+)",
                        RegexOptions.IgnoreCase);
                    if (mAcademico.Success)
                    {
                        docenteActual = new DocenteCargaDTO
                        {
                            NumeroPersonal = mAcademico.Groups[1].Value.Trim(),
                            Nombre = mAcademico.Groups[2].Value.Trim(),
                            Antiguedad = mAcademico.Groups[3].Value.Trim(),
                        };
                        result.Docentes.Add(docenteActual);
                        inDataSection = false;
                        plazaActual = categoriaActual = puestoActual = tipoContActual = "";
                        continue;
                    }

                    if (docenteActual == null) continue;


                    if (firstNorm.StartsWith("Plaza:", StringComparison.OrdinalIgnoreCase))
                    {
                        var mPlaza = Regex.Match(firstNorm, @"Plaza:\s*(\S+)", RegexOptions.IgnoreCase);
                        plazaActual = mPlaza.Success ? mPlaza.Groups[1].Value.Trim() : "";

                        var mCat = Regex.Match(firstNorm,
                            @"Categor[íi]a:\s*(.+?)\s+Puesto:", RegexOptions.IgnoreCase);
                        categoriaActual = mCat.Success ? mCat.Groups[1].Value.Trim() : "";

                        var mPuesto = Regex.Match(firstNorm,
                            @"Puesto:\s*(.+?)\s+Tipo\s+contrataci[oó]n:", RegexOptions.IgnoreCase);
                        puestoActual = mPuesto.Success ? mPuesto.Groups[1].Value.Trim() : "";

                        var mTipo = Regex.Match(firstNorm,
                            @"Tipo\s+contrataci[oó]n:\s*(.+)", RegexOptions.IgnoreCase);
                        tipoContActual = mTipo.Success ? mTipo.Groups[1].Value.Trim() : "";

                        if (string.IsNullOrEmpty(docenteActual.Plaza))
                        {
                            docenteActual.Plaza = plazaActual;
                            docenteActual.Categoria = categoriaActual;
                            docenteActual.Puesto = puestoActual;
                            docenteActual.TipoContratacion = tipoContActual;
                        }

                        inDataSection = false;
                        continue;
                    }

                    if (firstNorm.Equals("NRC", StringComparison.OrdinalIgnoreCase))
                    {
                        inDataSection = true;
                        continue;
                    }



                    if (!inDataSection) continue;

                    if (vals.Length >= 3 &&
                        NormalizeSpaces(vals[1]).Equals("Total de horas:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(vals[2], out var horas))
                            docenteActual.TotalHoras += horas;
                        inDataSection = false;
                        continue;
                    }

                    if (vals.Length >= 8 &&
      int.TryParse(vals[0], System.Globalization.NumberStyles.Any,
                   System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        docenteActual.Experiencias.Add(new CargaItemDTO
                        {
                            Nrc = vals[0],
                            ExperienciaEducativa = vals.ElementAtOrDefault(1) ?? "",
                            HorasContacto = ParseInt(vals.ElementAtOrDefault(2)),
                            HorasPago = ParseInt(vals.ElementAtOrDefault(3)),
                            ClaveProgramatica = vals.ElementAtOrDefault(4) ?? "",
                            MotivoRh = vals.ElementAtOrDefault(5) ?? "",
                            IndActDocente = vals.ElementAtOrDefault(6) ?? "",
                            Imparte = string.Equals(
                                                      vals.ElementAtOrDefault(7), "SI",
                                                      StringComparison.OrdinalIgnoreCase),
                            HorasExcCarga = ParseInt(vals.ElementAtOrDefault(8)),
                            NumPersonalSuplente = NullIfEmpty(vals.ElementAtOrDefault(9)),
                            NombreSuplente = NullIfEmpty(vals.ElementAtOrDefault(10)),
                            // ← plaza del bloque actual
                            Plaza = plazaActual,
                            Categoria = categoriaActual,
                            Puesto = puestoActual,
                            TipoContratacion = tipoContActual,
                        });
                    }
                }
            }
            while (excelReader.NextResult());

            return result;
        }

        private static string NormalizeSpaces(string s) =>
            Regex.Replace(s.Trim(), @"[\u00A0\u2009\u202F\u2002\u2003]+", " ");

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
                    docente.Experiencias.Add(ParseCargaItem(cells));
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

