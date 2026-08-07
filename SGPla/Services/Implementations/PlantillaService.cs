using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class PlantillaService : IPlantillaService
    {
        private readonly IArchivoService _archivoService;

        public PlantillaService(IArchivoService archivoService)
        {
            _archivoService = archivoService;
        }

        public async Task GenerarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var plantilla = await _archivoService.ObtenerArchivoEnBytesAsync("plantillas/aviso70temporal.docx");

            using (var stream =  new MemoryStream())
            {
                stream.Write(plantilla, 0, plantilla.Length);

                using(var wordDoc = WordprocessingDocument.Open(stream, true))
                {
                    reemplazarEtiquetasGenerales(wordDoc, plantillaAvisoDTO);
                    llenarTablaExperiencias(wordDoc, plantillaAvisoDTO.ListaExperiencias);
                    generarListaPerfiles(wordDoc, plantillaAvisoDTO.ListaExperiencias);

                    wordDoc.MainDocumentPart.Document.Save();
                }

                var nombreGuardado = $"{Guid.NewGuid()}.docx";
                await _archivoService.GuardarArchivoBytesAsync(stream.ToArray(), $"aviso-original/{nombreGuardado}");
            }
        }

        private void reemplazarEtiquetasGenerales(WordprocessingDocument wordDoc, PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "NombreFacultad", plantillaAvisoDTO.NombreEntidadAcademica },
                { "Region", plantillaAvisoDTO.Region },
                { "Campus", plantillaAvisoDTO.Campus },
                { "NombreArea", plantillaAvisoDTO.NombreAreaAcademica },
                { "Sistema", plantillaAvisoDTO.Sistema },
                { "NombrePrograma", plantillaAvisoDTO.NombreProgramaEducativo },
                { "Requisitos", plantillaAvisoDTO.Requisitos },
                { "DiasAceptacion", plantillaAvisoDTO.DiasAceptacion },
                { "FechaConsejoTecnico", plantillaAvisoDTO.FechaConsejoTecnico },
                { "FechaPublicacion", plantillaAvisoDTO.FechaPublicacion },
                { "NombreTitular", plantillaAvisoDTO.NombreTitular }
            };

            var controles = wordDoc.MainDocumentPart.Document.Body.Descendants<SdtElement>().ToList();

            foreach (var control in controles)
            {
                var tag = control.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;

                if (!string.IsNullOrEmpty(tag) && mapeo.ContainsKey(tag))
                {
                    var textElement = control.Descendants<Text>().FirstOrDefault();
                    if (textElement is not null)
                    {
                        textElement.Text = mapeo[tag] ?? string.Empty;
                    }
                }
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

            parrafo.RemoveAllChildren<Run>();
            parrafo.Append(new Run(new Text(texto ?? string.Empty)));
        }

        private void generarListaPerfiles(WordprocessingDocument wordDoc, List<PlantillaAvisoExperienciaEducativaDTO> experiencias)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            var sdtContenedor = body.Descendants<SdtElement>().FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>().Val?.Value == "ListaPerfiles");

            if (sdtContenedor == null) return;

            var parrafosMolde = sdtContenedor.Descendants<Paragraph>().ToList();
            if (parrafosMolde.Count < 2) return;

            foreach (var ee in experiencias)
            {
                var pTitulo = (Paragraph)parrafosMolde[0].CloneNode(true);
                reemplazarEtiquetaOTexto(pTitulo, "NombreExperiencia", ee.Nombre);
                var pPerfil = (Paragraph)parrafosMolde[1].CloneNode(true);
                reemplazarEtiquetaOTexto(pPerfil, "PerfilDocente", ee.PerfilDocente);

                sdtContenedor.InsertBeforeSelf(pTitulo);
                sdtContenedor.InsertBeforeSelf(pPerfil);
            }

            sdtContenedor.Remove();
        }

        private void reemplazarEtiquetaOTexto(Paragraph parrafo, string etiqueta, string nuevoTexto)
        {
            var control = parrafo.Descendants<SdtElement>().FirstOrDefault(sdt => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value == etiqueta);

            if (control is not null)
            {
                var textElement = control.Descendants<Text>().FirstOrDefault();
                if (textElement != null) textElement.Text = nuevoTexto ?? string.Empty;
            }
            else
            {
                string textoBuscado = $"[{etiqueta}]";
                foreach (var textNode in parrafo.Descendants<Text>())
                {
                    if (textNode.Text.Contains(textoBuscado))
                    {
                        textNode.Text = textNode.Text.Replace(textoBuscado, nuevoTexto ?? string.Empty);
                    }
                }
            }
        }
    }
}
