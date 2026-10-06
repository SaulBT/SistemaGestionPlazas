using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using SGPla.Models.DTOs.Plantillas;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations
{
    public class PlantillaService : IPlantillaService
    {
        private const string VISTA_AVISO = "~/Views/Avisos/_ContenidoAviso.cshtml";

        private readonly IArchivoService _archivoService;
        private readonly IRazorViewEngine _motorVistas;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public PlantillaService(
            IArchivoService archivoService,
            IRazorViewEngine motorVistas,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _archivoService = archivoService;
            _motorVistas = motorVistas;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        // ==========
        // AVISOS
        // ==========

        public async Task<int> GenerarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO)
        {
            var contenido = await RenderizarAvisoAsync(plantillaAvisoDTO);
            var nombreGuardado = $"aviso-{Guid.NewGuid():N}.html";

            return await _archivoService.GuardarArchivoBytesAsync(
                Encoding.UTF8.GetBytes(contenido), "aviso-original", nombreGuardado);
        }

        public async Task<string> RenderizarAvisoAsync(PlantillaAvisoDTO plantillaAvisoDTO)
        {
            ArgumentNullException.ThrowIfNull(plantillaAvisoDTO);

            var contextoHttp = new DefaultHttpContext { RequestServices = _serviceProvider };
            var contextoAccion = new ActionContext(contextoHttp, new RouteData(), new ActionDescriptor());

            var resultadoVista = _motorVistas.GetView(null, VISTA_AVISO, isMainPage: false);
            if (!resultadoVista.Success)
                throw new InvalidOperationException($"No se encontró la vista de la plantilla del aviso ({VISTA_AVISO}).");

            await using var escritor = new StringWriter();
            var datosVista = new ViewDataDictionary<PlantillaAvisoDTO>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = plantillaAvisoDTO
            };
            var contextoVista = new ViewContext(
                contextoAccion,
                resultadoVista.View,
                datosVista,
                new TempDataDictionary(contextoHttp, _tempDataProvider),
                escritor,
                new HtmlHelperOptions());

            await resultadoVista.View.RenderAsync(contextoVista);
            return escritor.ToString();
        }
    }
}
