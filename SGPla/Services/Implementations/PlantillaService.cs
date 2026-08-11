using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class PlantillaService : IPlantillaService
    {
        private readonly IArchivoService _archivoService;
        private readonly IArticuloService _articuloService;

        public PlantillaService(
            IArchivoService archivoService,
            IArticuloService articuloService)
        {
            _archivoService = archivoService;
            _articuloService = articuloService;
        }

        // ==========
        // AVISOS
        // ==========

        public async Task<int> GenerarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var plantilla = await _archivoService.ObtenerArchivoEnBytesAsync("plantillas/aviso.docx");

            if (plantilla is null || plantilla.Length == 0)
            {
                throw new ArgumentException("El arreglo de bytes 'plantilla' está vacío o es nulo.");
            }

            using (var stream =  new MemoryStream())
            {
                stream.Write(plantilla, 0, plantilla.Length);
                stream.Position = 0;

                using (var wordDoc = WordprocessingDocument.Open(stream, true))
                {
                    if (wordDoc.MainDocumentPart is null)
                    {
                        throw new InvalidDataException(
                            $"El stream se creó con {plantilla.Length} bytes, pero Open XML no reconoció un 'MainDocumentPart'. " +
                            "Verifica que la plantilla sea un archivo .docx válido y no un .doc reetiquetado.");
                    }

                    await reemplazarEtiquetasGeneralesAsync(wordDoc, plantillaAvisoDTO);
                    llenarTablaExperiencias(wordDoc, plantillaAvisoDTO.ListaExperiencias);
                    generarListaPerfiles(wordDoc, plantillaAvisoDTO.ListaExperiencias);

                    wordDoc.MainDocumentPart.Document.Save();
                }

                var nombreGuardado = $"{plantillaAvisoDTO.Folio}.docx";
                var id = await _archivoService.GuardarArchivoBytesAsync(stream.ToArray(), "aviso-original", nombreGuardado);

                return id;
            }
        }

        private async Task reemplazarEtiquetasGeneralesAsync(WordprocessingDocument wordDoc, PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "AreaAcademica", plantillaAvisoDTO.AreaAcademica },
                { "EntidadAcademica", plantillaAvisoDTO.EntidadAcademica },
                { "Articulo", plantillaAvisoDTO.Articulo },
                { "Region", plantillaAvisoDTO.Region },
                { "PerfilArticulo", await determinarPerfilArticuloAsync(plantillaAvisoDTO.Articulo) },
                { "Periodo", plantillaAvisoDTO.Periodo },
                { "Campus", plantillaAvisoDTO.Campus },
                { "NombreArea", determinarNombreArea(plantillaAvisoDTO.AreaAcademica) },
                { "Sistema", plantillaAvisoDTO.Sistema },
                { "ProgramaEducativo", plantillaAvisoDTO.ProgramaEducativo },
                { "Requisitos", plantillaAvisoDTO.Requisitos },
                { "DiasAceptacion", plantillaAvisoDTO.DiasAceptacion },
                { "FechaConsejoTecnico", plantillaAvisoDTO.FechaConsejoTecnico },
                { "FechaPublicacion", plantillaAvisoDTO.FechaPublicacion },
                { "NombreTitular", plantillaAvisoDTO.Titular }
            };

            foreach (var kvp in mapeo)
            {
                reemplazarEncabezado(wordDoc, kvp.Key, kvp.Value);
            }

            // ===
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart is null) return;

            if (!mainPart.IsRootElementLoaded)
            {
                _ = mainPart.Document;
            }

            var document = mainPart.Document;
            var body = document?.Body;

            if (body is null) return;

            var controles = body.Descendants<SdtElement>().ToList();

            // ===

            foreach (var control in controles)
            {
                var tag = control.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;

                if (!string.IsNullOrEmpty(tag) && mapeo.ContainsKey(tag))
                {
                    if (tag.Contains("Requisitos"))
                    {
                        escribirRequisitos(control, mapeo[tag]);
                    }
                    else
                    {
                        var textElements = control.Descendants<Text>().ToList();
                        if (textElements.Any())
                        {
                            textElements.First().Text = mapeo[tag] ?? string.Empty;

                            foreach (var extraText in textElements.Skip(1))
                            {
                                extraText.Text = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        private string determinarNombreArea(string areaAcademica)
        {
            if (areaAcademica.Contains("Artes")) return "Artes";
            if (areaAcademica.Contains("Biologicas y Agropecuarias")) return "Biologicas y Agropecuarias";
            if (areaAcademica.Contains("Ciencias de la Salud")) return "Ciencias de la Salud";
            if (areaAcademica.Contains("Económico-Administrativa")) return "Económico-Administrativa";
            if (areaAcademica.Contains("Técnica")) return "Técnica";
            if (areaAcademica.Contains("Humanidades")) return "Humanidades";
            if (areaAcademica.Contains("Investigaciones")) return "Investigaciones";
            if (areaAcademica.Contains("Relaciones Internacionales")) return "Relaciones Internacionales";

            return areaAcademica;
        }

        private async Task<string> determinarPerfilArticuloAsync(string articulo)
        {
            var articulos = await _articuloService.ObtenerTodosAsync();
            var articuloDTO = articulos.FirstOrDefault(a => a.Numero == articulo);

            return articuloDTO?.Descripcion ?? "";
        }

        private void escribirRequisitos(SdtElement control, string nuevoTexto)
        {
            var run = control.Descendants<Run>().FirstOrDefault();
            if (run is null) return;

            run.RemoveAllChildren<Text>();
            run.RemoveAllChildren<Break>();

            foreach (var extraRun in control.Descendants<Run>().Skip(1).ToList())
            {
                extraRun.Remove();
            }

            if (string.IsNullOrEmpty(nuevoTexto)) return;

            string[] lineas = nuevoTexto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lineas.Length; i++)
            {
                if (i > 0)
                {
                    run.AppendChild(new Break());
                }

                run.AppendChild(new Text(lineas[i]));
            }
        }

        private void llenarTablaExperiencias(WordprocessingDocument wordDoc, List<PlantillaAvisoExperienciaEducativaDTO> experiencias)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;
            var tabla = body.Elements<Table>().FirstOrDefault();

            if (tabla == null) return;

            var filas = tabla.Elements<TableRow>().ToList();
            if (filas.Count < 3) return;

            var filaMolde = filas[2];
            int pa = 1;

            foreach (var ee in experiencias)
            {
                var nuevaFila = (TableRow)filaMolde.CloneNode(true);
                var celdas = nuevaFila.Elements<TableCell>().ToArray();

                llenarCelda(celdas[0], ee.Horas);
                llenarCelda(celdas[1], ee.Nombre);
                llenarCelda(celdas[2], ee.NRC);
                llenarCelda(celdas[3], ee.Plaza);
                llenarCelda(celdas[4], ee.HorarioLunes);
                llenarCelda(celdas[5], ee.HorarioMartes);
                llenarCelda(celdas[6], ee.HorarioMiercoles);
                llenarCelda(celdas[7], ee.HorarioJueves);
                llenarCelda(celdas[8], ee.HorarioViernes);
                llenarCelda(celdas[9], ee.HorarioSabado);
                llenarCelda(celdas[10], ee.TipoContratacion);
                llenarCelda(celdas[11], pa.ToString());

                tabla.AppendChild(nuevaFila); ;
                pa++;
            }

            filaMolde.Remove();
        }

        private void llenarCelda(TableCell celda, string texto)
        {
            var parrafo = celda.Elements<Paragraph>().FirstOrDefault();
            if (parrafo is null)
            {
                parrafo = new Paragraph();
                celda.Append(parrafo);
            }

            RunProperties rPrOriginal = null;

            var rPrExistente = parrafo.Descendants<RunProperties>().FirstOrDefault();
            if (rPrExistente is not null)
            {
                rPrOriginal = (RunProperties)rPrExistente.CloneNode(true);
            }
            else if (parrafo.ParagraphProperties?.ParagraphMarkRunProperties is not null)
            {
                rPrOriginal = new RunProperties(parrafo.ParagraphProperties.ParagraphMarkRunProperties.OuterXml);
            }

            parrafo.RemoveAllChildren<Run>();

            var nuevoRun = new Run();

            if (rPrOriginal is not null)
            {
                nuevoRun.AppendChild(rPrOriginal);
            }
            else
            {
                var rPrFallback = new RunProperties(
                    new RunFonts() { Ascii = "Gill Sans MT", HighAnsi = "Gill Sans MT", ComplexScript = "Gill Sans MT" },
                    new FontSize() { Val = "18" },
                    new FontSizeComplexScript() { Val = "18" }
                );
                nuevoRun.AppendChild(rPrFallback);
            }

            nuevoRun.AppendChild(new Text(texto ?? string.Empty));

            parrafo.AppendChild(nuevoRun);
        }

        private void generarListaPerfiles(WordprocessingDocument wordDoc, List<PlantillaAvisoExperienciaEducativaDTO> experiencias)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            var sdtContenedor = body.Descendants<SdtElement>()
                .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "ListaPerfiles");

            if (sdtContenedor is null) return;

            var sdtNombreMolde = sdtContenedor.Descendants<SdtElement>()
                .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "NombreExperiencia");
            var sdtPerfilMolde = sdtContenedor.Descendants<SdtElement>()
                .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "PerfilDocente");

            if (sdtNombreMolde is null || sdtPerfilMolde is null) return;

            foreach (var ee in experiencias)
            {
                var nuevoSdtNombre = (SdtElement)sdtNombreMolde.CloneNode(true);
                var nuevoSdtPerfil = (SdtElement)sdtPerfilMolde.CloneNode(true);

                actualizarTextoControl(nuevoSdtNombre, ee.Nombre);
                actualizarTextoControl(nuevoSdtPerfil, ee.PerfilDocente);

                sdtContenedor.InsertBeforeSelf(nuevoSdtNombre);
                sdtContenedor.InsertBeforeSelf(nuevoSdtPerfil);
            }

            sdtContenedor.Remove();
        }

        private void reemplazarEncabezado(WordprocessingDocument wordDoc, string etiqueta, string nuevoTexto)
        {
            foreach (var headerPart in wordDoc.MainDocumentPart.HeaderParts)
            {
                var header = headerPart.Header;
                if (header is null) continue;

                var control = header.Descendants<SdtElement>()
                    .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == etiqueta);

                if (control is not null)
                {
                    var textElements = control.Descendants<Text>().ToList();
                    if (textElements.Any())
                    {
                        textElements.First().Text = nuevoTexto ?? string.Empty;
                        foreach (var extraText in textElements.Skip(1))
                        {
                            extraText.Text = string.Empty;
                        }
                    }
                }
                else
                {
                    string textoBuscado = $"[{etiqueta}]";
                    foreach (var textNode in header.Descendants<Text>())
                    {
                        if (textNode.Text.Contains(textoBuscado))
                        {
                            textNode.Text = textNode.Text.Replace(textoBuscado, nuevoTexto ?? string.Empty);
                        }
                    }
                }

                header.Save();
            }
        }

        // ==========
        // UTILS
        // ==========

        private void actualizarTextoControl(SdtElement control, string nuevoTexto)
        {
            var textElements = control.Descendants<Text>().ToList();
            if (textElements.Any())
            {
                textElements.First().Text = nuevoTexto ?? string.Empty;
                foreach (var extraText in textElements.Skip(1))
                {
                    extraText.Text = string.Empty;
                }
            }
        }
    }
}
