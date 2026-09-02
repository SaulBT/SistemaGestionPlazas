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
                        var mPlaza = Regex.Match(firstNorm, @"Plaza:\s*(\d+)", RegexOptions.IgnoreCase);

                        if (mPlaza.Success)
                        {
                            plazaActual = mPlaza.Groups[1].Value.Trim();

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
                        }
                        else
                        {
                            // Bloque placeholder (Plaza vacía, Puesto: "0-", Tipo: "---").
                            // NO heredamos los valores del bloque anterior: si esta experiencia
                            // perteneciera a esa misma plaza, el archivo la habría puesto en la
                            // misma tabla. Al estar en un bloque/tabla separado, lo correcto es
                            // tratar la plaza como desconocida (null), no como "la misma de antes".
                            plazaActual = categoriaActual = puestoActual = tipoContActual = "";
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

                    // ── Fila "Total de horas:" → cierra la sección de datos ──────────
                    if (vals.Length >= 3 &&
                        NormalizeSpaces(vals[1]).Equals("Total de horas:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(vals[2], out var horas))
                            docenteActual.TotalHoras += horas;
                        inDataSection = false;
                        continue;
                    }

                    // ── Fila "HORAS DE ASIGNATURA" → sin NRC, pero SÍ suma horas ─────
                    // Ejemplo real: ['', 'HORAS DE ASIGNATURA', '', '1', '14367', ...]
                    // Antes esta fila se perdía porque vals[0] está vacío y no pasaba
                    // el chequeo de "es un NRC numérico".
                    if (vals.Length >= 4 &&
                        string.IsNullOrWhiteSpace(vals[0]) &&
                        NormalizeSpaces(vals[1]).Equals("HORAS DE ASIGNATURA", StringComparison.OrdinalIgnoreCase))
                    {
                        docenteActual.Experiencias.Add(new CargaItemDTO
                        {
                            Nrc = string.Empty,
                            ExperienciaEducativa = "HORAS DE ASIGNATURA",
                            HorasContacto = ParseInt(vals.ElementAtOrDefault(2)),
                            HorasPago = ParseInt(vals.ElementAtOrDefault(3)),
                            ClaveProgramatica = vals.ElementAtOrDefault(4) ?? "",
                            Plaza = plazaActual,
                            Categoria = categoriaActual,
                            Puesto = puestoActual,
                            TipoContratacion = tipoContActual,
                        });
                        continue;
                    }

                    // ── Fila normal con NRC ───────────────────────────────────────────
                    if (vals.Length >= 8 &&
                        int.TryParse(vals[0], System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        var valorImparte = vals.ElementAtOrDefault(7)?.Trim();

                        docenteActual.Experiencias.Add(new CargaItemDTO
                        {
                            Nrc = vals[0],
                            ExperienciaEducativa = vals.ElementAtOrDefault(1) ?? "",
                            HorasContacto = ParseInt(vals.ElementAtOrDefault(2)),
                            HorasPago = ParseInt(vals.ElementAtOrDefault(3)),
                            ClaveProgramatica = vals.ElementAtOrDefault(4) ?? "",
                            MotivoRh = vals.ElementAtOrDefault(5) ?? "",
                            IndActDocente = vals.ElementAtOrDefault(6) ?? "",
                            Imparte = string.IsNullOrWhiteSpace(valorImparte)
                            ? null
                            : string.Equals(valorImparte, "SI", StringComparison.OrdinalIgnoreCase),
                            HorasExcCarga = ParseInt(vals.ElementAtOrDefault(8)),
                            NumPersonalSuplente = NullIfEmpty(vals.ElementAtOrDefault(9)),
                            NombreSuplente = NullIfEmpty(vals.ElementAtOrDefault(10)),
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

            // Valores del docente "principal" (su plaza/categoría base) — se fijan
            // UNA SOLA VEZ, con el primer bloque que sí traiga Plaza.
            // (sirve para mostrar "quién es" el académico en general)

            // Valores que se le asignan a las EXPERIENCIAS de la tabla que sigue
            // inmediatamente. A diferencia del dato anterior, estos se toman
            // TAL CUAL vienen en cada bloque — incluso vacíos — porque el
            // archivo de la UV los deja en blanco a propósito cuando una
            // asignación específica no pertenece a la plaza/puesto principal
            // (por ejemplo, horas extra que exceden el límite de pago del
            // docente y se gestionan aparte). No hay que "rellenar" ese vacío
            // con el bloque anterior: sería inventar un dato que el archivo
            // no está dando.
            string plazaItem = "", categoriaItem = "", puestoItem = "", tipoItem = "";

            // Buffer temporal del bloque que se está leyendo en este momento.
            // Los 4 campos siempre aparecen consecutivos (Plaza, Categoría,
            // Puesto, Tipo contratación) y se confirman juntos al llegar al
            // último (Tipo contratación).
            string bufPlaza = "", bufCategoria = "", bufPuesto = "", bufTipo = "";

            var current = spanNumpersonal.NextSibling;

            while (current != null)
            {
                if (current.Name == "span")
                {
                    var cls = current.GetAttributeValue("class", "");

                    if (cls.Contains("contenedorNumpersonal"))
                        break;

                    if (cls.Contains("contenedorPlaza"))
                    {
                        bufPlaza = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");
                    }
                    else if (cls.Contains("contenedorCategoria"))
                    {
                        bufCategoria = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");
                    }
                    else if (cls.Contains("contenedorPuesto"))
                    {
                        bufPuesto = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");
                    }
                    else if (cls.Contains("contenedorTipocontratacion"))
                    {
                        bufTipo = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");

                        // Fin del bloque: se aplica TAL CUAL a las experiencias
                        // de la tabla que sigue, vacío o no.
                        plazaItem = bufPlaza;
                        categoriaItem = bufCategoria;
                        puestoItem = bufPuesto;
                        tipoItem = bufTipo;

                        // El dato "oficial" del docente (su plaza principal) es
                        // el del PRIMER bloque que sí trae Plaza.
                        if (string.IsNullOrEmpty(docente.Plaza) && !string.IsNullOrWhiteSpace(bufPlaza))
                        {
                            docente.Plaza = bufPlaza;
                            docente.Categoria = bufCategoria;
                            docente.Puesto = bufPuesto;
                            docente.TipoContratacion = bufTipo;
                        }

                        bufPlaza = bufCategoria = bufPuesto = bufTipo = "";
                    }
                    else if (string.IsNullOrEmpty(docente.Antiguedad))
                    {
                        docente.Antiguedad = InnerTextOf(current, ".//span[contains(@class,'textoacademico')]");
                    }
                }

                if (current.Name == "table")
                {
                    ParseTable(current, docente, plazaItem, categoriaItem, puestoItem, tipoItem);
                }

                current = current.NextSibling;
            }

            return docente;
        }

        private static void ParseTable(HtmlNode table, DocenteCargaDTO docente,
            string plaza, string categoria, string puesto, string tipoContratacion)
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

                // ── "Total de horas:" → cierra la sección ────────────────────────
                if (cells.Length >= 4 && cells[1] == "Total de horas:")
                {
                    if (int.TryParse(cells[2], out var horas))
                        docente.TotalHoras += horas;

                    break;
                }

                // ── "HORAS DE ASIGNATURA" → sin NRC, pero SÍ suma horas ──────────
                if (IsHorasAsignaturaRow(cells))
                {
                    docente.Experiencias.Add(new CargaItemDTO
                    {
                        Nrc = string.Empty,
                        ExperienciaEducativa = "HORAS DE ASIGNATURA",
                        HorasContacto = ParseInt(cells.ElementAtOrDefault(2)),
                        HorasPago = ParseInt(cells.ElementAtOrDefault(3)),
                        ClaveProgramatica = cells.ElementAtOrDefault(4) ?? string.Empty,
                        Plaza = plaza,
                        Categoria = categoria,
                        Puesto = puesto,
                        TipoContratacion = tipoContratacion,
                    });
                    continue;
                }

                if (cells.Length >= 8 && IsNrcRow(cells[0]))
                {
                    var item = ParseCargaItem(cells);
                    item.Plaza = plaza;
                    item.Categoria = categoria;
                    item.Puesto = puesto;
                    item.TipoContratacion = tipoContratacion;
                    docente.Experiencias.Add(item);
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

        private static bool IsHorasAsignaturaRow(string[] cells) =>
            cells.Length >= 4 &&
            string.IsNullOrWhiteSpace(cells[0]) &&
            cells[1].Trim().Equals("HORAS DE ASIGNATURA", StringComparison.OrdinalIgnoreCase);

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