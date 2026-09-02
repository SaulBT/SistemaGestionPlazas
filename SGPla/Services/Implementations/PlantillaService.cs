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
                    // ===
                    if (wordDoc.MainDocumentPart is null)
                    {
                        throw new InvalidDataException(
                            $"El stream se creó con {plantilla.Length} bytes, pero Open XML no reconoció un 'MainDocumentPart'. " +
                            "Verifica que la plantilla sea un archivo .docx válido y no un .doc reetiquetado.");
                    }

                    var mainPart = wordDoc.MainDocumentPart;
                    if (mainPart is null) throw new InvalidDataException();

                    if (!mainPart.IsRootElementLoaded)
                    {
                        _ = mainPart.Document;
                    }

                    var document = mainPart.Document;
                    var body = document?.Body;

                    if (body is null) throw new InvalidDataException();

                    // ===

                    reemplazarEtiquetasGenerales(wordDoc, body, plantillaAvisoDTO);
                    generarSeccionProgramas(body, plantillaAvisoDTO.Programas);

                    wordDoc.MainDocumentPart.Document.Save();
                }

                var nombreGuardado = $"{plantillaAvisoDTO.Folio}.docx";
                var id = await _archivoService.GuardarArchivoBytesAsync(stream.ToArray(), "aviso-original", nombreGuardado);

                return id;
            }
        }

        private void reemplazarEtiquetasGenerales(WordprocessingDocument wordDoc, Body body, PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "AreaAcademica", plantillaAvisoDTO.AreaAcademica },
                { "EntidadAcademica", plantillaAvisoDTO.EntidadAcademica },
                { "Articulo", plantillaAvisoDTO.Articulo },
                { "Region", plantillaAvisoDTO.Region },
                { "PerfilArticulo", plantillaAvisoDTO.PerfilArticulo },
                { "Periodo", plantillaAvisoDTO.Periodo },
                { "Campus", plantillaAvisoDTO.Campus },
                { "NombreArea", plantillaAvisoDTO.AreaAcademica },
                { "Sistema", plantillaAvisoDTO.Sistema },
                { "Requisitos", plantillaAvisoDTO.Requisitos },
                { "HorarioAceptacion", plantillaAvisoDTO.HorarioAceptacion },
                { "FechaConsejoTecnico", plantillaAvisoDTO.FechaConsejoTecnico },
                { "FechaPublicacion", plantillaAvisoDTO.FechaPublicacion },
                { "NombreTitular", plantillaAvisoDTO.Titular }
            };

            foreach (var kvp in mapeo)
            {
                reemplazarEncabezado(wordDoc, kvp.Key, kvp.Value);
            }

            var controles = body.Descendants<SdtElement>().ToList();

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

        private void generarSeccionProgramas(Body body, List<PlantillaAvisoProgramaEducativoDTO> programas)
        {
            var sdtProgramaMolde = body.Descendants<SdtElement>()
                .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "BloquePrograma");

            if (sdtProgramaMolde is null) return;

            foreach (var programa in programas)
            {
                var nuevoBloque = (SdtElement)sdtProgramaMolde.CloneNode(true);

                //ProgramaEducativo
                var controlPrograma = nuevoBloque.Descendants<SdtElement>()
                    .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "ProgramaEducativo");
                if (controlPrograma is not null)
                    actualizarTextoControl(controlPrograma, programa.ProgramaEducativo);

                //Tabla
                var tabla = nuevoBloque.Descendants<Table>().FirstOrDefault();
                if (tabla is not null)
                    llenarTablaExperienciasAviso(tabla, programa.Experiencias);

                //Perfiles
                var sdtPerfiles = nuevoBloque.Descendants<SdtElement>()
                    .FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == "ListaPerfiles");
                if (sdtPerfiles is not null)
                    generarListaPerfilesContenedor(sdtPerfiles, programa.Experiencias);

                sdtProgramaMolde.InsertBeforeSelf(nuevoBloque);
            }

            sdtProgramaMolde.Remove();
        }

        private void llenarTablaExperienciasAviso(Table tabla, List<PlantillaAvisoExperienciaEducativaDTO> experiencias)
        {
            var filas = tabla.Elements<TableRow>().ToList();
            if (filas.Count < 3) return;

            var filaMolde = filas[2];
            int pa = 1;

            foreach (var ee in experiencias)
            {
                var nuevaFila = (TableRow)filaMolde.CloneNode(true);
                var celdas = nuevaFila.Elements<TableCell>().ToArray();

                llenarCeldaExperienciasAviso(celdas[0], ee.Horas);
                llenarCeldaExperienciasAviso(celdas[1], ee.Nombre);
                llenarCeldaExperienciasAviso(celdas[2], ee.NRC);
                llenarCeldaExperienciasAviso(celdas[3], ee.Plaza);
                llenarCeldaExperienciasAviso(celdas[4], ee.HorarioLunes);
                llenarCeldaExperienciasAviso(celdas[5], ee.HorarioMartes);
                llenarCeldaExperienciasAviso(celdas[6], ee.HorarioMiercoles);
                llenarCeldaExperienciasAviso(celdas[7], ee.HorarioJueves);
                llenarCeldaExperienciasAviso(celdas[8], ee.HorarioViernes);
                llenarCeldaExperienciasAviso(celdas[9], ee.HorarioSabado);
                llenarCeldaExperienciasAviso(celdas[10], ee.TipoContratacion);
                llenarCeldaExperienciasAviso(celdas[11], pa.ToString());

                tabla.AppendChild(nuevaFila); ;
                pa++;
            }

            filaMolde.Remove();
        }

        private void llenarCeldaExperienciasAviso(TableCell celda, string texto)
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

        private void generarListaPerfilesContenedor(SdtElement sdtContenedor, List<PlantillaAvisoExperienciaEducativaDTO> experiencias)
        {
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
