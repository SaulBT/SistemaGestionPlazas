using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Helpers;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Articulo;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace SGPla.Controllers;

enum TipoTablaOferta
{
    Asignadas,
    Vacantes
}

public class ProgramacionesAcademicasController : Controller
{
    private readonly ILogger<ProgramacionesAcademicasController> _logger;
    private readonly IProgramacionAcademicaService _programacionAcademicaService;
    private readonly IPeriodoEscolarService _periodoEscolarService;
    private readonly IArticuloService _articuloService;
    private int _paginaActual = 1;
    private static List<string> HEADERS_TABLA_ASIGNADAS = ["Experiencia educativa", "NRC", "H/S/M", "Tipo contratación", "Horario", "Docente"];
    private static List<string> HEADERS_TABLA_VACANTES = ["Experiencia educativa", "NRC", "H/S/M", "Tipo contratación", "Horario", "Artículo"];
    private static List<string> HEADERS_TABLA_RESUMEN_OFERTA = ["Entidad Academica", "Programa Educativo", "Periodo", "EE Asignadas", "EE Vacantes", "Acciones"];
    private static List<string> HEADERS_TABLA_CARGAS = ["NP", "Docente", "Plaza", "NRC", "Experiencia Educativa", "Hrs Contacto", "Hrs Pago", "Imparte"];
    private const string SESSION_REGION = "Region";
    private const string SESSION_ID_PERIODO = "IdPeriodo";
    private const string SESSION_ID_ENTIDAD = "IdEntidad";
    private const string SESSION_NOMBRE_PERIODO = "NombrePeriodo";
    private const string SESSION_NOMBRE_ENTIDAD = "NombreEntidadAcademica";

    public ProgramacionesAcademicasController(
        ILogger<ProgramacionesAcademicasController> logger,
        IProgramacionAcademicaService programacionAcademicaService,
        IPeriodoEscolarService periodoEscolarService,
        IArticuloService articuloService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
        _periodoEscolarService = periodoEscolarService;
        _articuloService = articuloService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? region,
        int? idEntidadAcademica,
        int? idProgramaEducativo,
        int? idPeriodo,
        string? busqueda)
    {
        var filtro = new BuscarProgramacionAcademicaDTO
        {
            Region = region,
            IdEntidadAcademica = idEntidadAcademica,
            IdProgramaEducativo = idProgramaEducativo,
            IdPeriodo = idPeriodo,
            Busqueda = busqueda
        };

        IndexViewModel modelo = new IndexViewModel
        {
            Region = filtro.Region,
            IdEntidadAcademica = filtro.IdEntidadAcademica,
            IdProgramaEducativo = filtro.IdProgramaEducativo,
            IdPeriodo = filtro.IdPeriodo
        };

        modelo.Regiones = Constantes.REGIONES
            .Select(r => new OptionModel { Value = r, Text = r })
            .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        modelo.Periodos = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
            })
            .ToList();

        List<EntidadAcademica> entidades = [];

        if (!string.IsNullOrEmpty(filtro.Region))
        {
            entidades =
                await _programacionAcademicaService
                    .ObtenerOpcionesEntidadAcademicaAsync(filtro.Region);
        }

        modelo.Entidades = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre
            })
            .ToList();

        List<ProgramaEducativo> programas = [];

        if (filtro.IdEntidadAcademica.HasValue)
        {
            programas =
                await _programacionAcademicaService
                    .ObtenerOpcionesProgramaEducativoAsync(filtro.IdEntidadAcademica.Value);
        }

        modelo.Programas = programas
           .Select(p => new OptionModel
           {
               Value = p.IdProgramaEducativo.ToString(),
               Text = p.Nombre
           })
           .ToList();

        var resumen = await _programacionAcademicaService.ObtenerResumenPorProgramaPeriodoAsync(filtro);

        modelo.ResumenesProgramacionesAcademicas = resumen;

        modelo.Table = LlenarTablaResumen(resumen);

        return View("Index", modelo);
    }

    private TableModel LlenarTablaResumen(List<ResumenOfertaProgramacionAcademicaDTO> resumen)
    {
        if (resumen.Count() == 0)
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_RESUMEN_OFERTA, string.Format(Constantes.TABLA_VACIA, Constantes.PROGRAMACIONES_ACADEMICAS));

        try
        {
            return new TableModel
            {
                TableId = "tablaResumenOferta",
                Headers = HEADERS_TABLA_RESUMEN_OFERTA,
                Rows = resumen.Select(r => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                {
                    new() { Value = r.EntidadAcademica },
                    new() { Value = r.ProgramaEducativo },
                    new() { Value = r.PeriodoMostrar },
                    new() { Value = r.EEAsignadas.ToString() },
                    new() { Value = r.EEVacantes.ToString() },
                    new()
                    {
                         Actions = new List<TableActionModel>
                         {
                            new TableActionModel()
                            {
                                Accion = "ver",
                               OnClick = $"location.href='{Url.Action("Ver", new {
                                idEntidadAcademica = r.IdEntidadAcademica,
                                idProgramaEducativo = r.IdProgramaEducativo,
                                idPeriodo = r.IdPeriodo,
                                nombreEntidadAcademica = r.EntidadAcademica,
                                nombrePeriodo = r.PeriodoMostrar,
                                nombrePrograma = r.ProgramaEducativo,
                            })}'"
                            },
                            new TableActionModel()
                            {
                                Accion = "solicitudes",
                                //OnClick = $"abrirModalConfirmacion('¿Desea eliminar este periodo?', function() {{ eliminarPeriodoEscolar({a.IdPeriodoEscolar}); }})"
                            }
                        }
                        }
                }
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = resumen.Count,
                    OnPageChange = "cambiarPagina",
                    PaginationMode = "client"
                }
            };
        }
        catch (Exception)
        {
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_RESUMEN_OFERTA, string.Format(Constantes.ERROR_TABLA, Constantes.PLANES_ESTUDIOS));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso1()
    {
        HttpContext.Session.Remove("Ofertas");
        HttpContext.Session.Remove("Cargas");

        CargarProgramacionAcademica1ViewModel modelo = new CargarProgramacionAcademica1ViewModel();
        await CargarCombos(modelo);

        return (View("CargarProgramacionAcademicaPaso1", modelo));
    }

    public async Task CargarCombos(CargarProgramacionAcademica1ViewModel modelo)
    {
        modelo.Regiones = Constantes.REGIONES
             .Select(r => new OptionModel { Value = r, Text = r })
             .ToList();

        var periodos = await _periodoEscolarService.ObtenerTodosAsync();

        modelo.Periodos = periodos
            .Select(p => new OptionModel
            {
                Value = p.IdPeriodoEscolar.ToString(),
                Text = p.PeriodoMostrar,
            })
            .ToList();

        List<EntidadAcademica> entidades = [];

        if (!string.IsNullOrEmpty(modelo.Region))
        {
            entidades =
                await _programacionAcademicaService
                    .ObtenerOpcionesEntidadAcademicaAsync(modelo.Region);
        }

        modelo.Entidades = entidades
            .Select(e => new OptionModel
            {
                Value = e.IdEntidadAcademica.ToString(),
                Text = e.Nombre
            })
            .ToList();
    }

    [HttpPost]
    public async Task<IActionResult> Guardar()
    {
        var ofertas = ObtenerOfertasSesion();
        var cargas = ObtenerCargasSesion();

        if (ofertas.Count == 0)
            return BadRequest("No hay ofertas para guardar.");

        try
        {
            var guardado =
                await _programacionAcademicaService
                    .GuardarOfertasyCargasAsync(ofertas, cargas);
            TempData["Success"] = "Cambios guardados con éxito";
            return RedirectToAction(nameof(Index));

        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var vm = await ObtenerViewModelCompletoAsync();
            return View("CargarProgramacionAcademicaPaso2", vm);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CargarCargas(IFormFile archivoCarga)
    {
        var vm = new CargarProgramacionAcademica2ViewModel();

        try
        {
            vm.CargasAcademicas = await _programacionAcademicaService
                .ProcesarCargasAsync(archivoCarga);
        }
        catch (Exception ex)
        {
            vm.Error = $"Error al procesar las cargas: {ex.Message}";
        }

        return View("Index", vm);
    }

    private List<OfertaDTO> ObtenerOfertasSesion()
    {
        var json = HttpContext.Session.GetString("Ofertas");

        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<OfertaDTO>>(json)!;
    }


    private List<CargaConOfertaDTO> ObtenerCargasSesion()
    {
        var json = HttpContext.Session.GetString("Cargas");

        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<CargaConOfertaDTO>>(json)!;
    }

    [HttpGet]
    public async Task<IActionResult> FiltrarPrograma(string? programa)
    {
        var vm = await ObtenerViewModelCompletoAsync(programa);
        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    private async Task<CargarProgramacionAcademica2ViewModel> ObtenerViewModelCompletoAsync(string? programa = null)
    {
        var vm = await ObtenerViewModelDesdeSesion(programa);

        vm.Region = HttpContext.Session.GetString(SESSION_REGION);
        vm.IdEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD)!.Value;
        vm.NombrePeriodo = HttpContext.Session.GetString(SESSION_NOMBRE_PERIODO);
        vm.NombreEntidadAcademica = HttpContext.Session.GetString(SESSION_NOMBRE_ENTIDAD);
        vm.ProgramaSeleccionado = programa;

        return vm;
    }

    private async Task<CargarProgramacionAcademica2ViewModel> ObtenerViewModelDesdeSesion(string? programa = null)
    {
        var ofertas = ObtenerOfertasSesion();
        var cargas = ObtenerCargasSesion();

        var programasCombo = ofertas
         .Select(o => o.Programa)
         .Distinct()
         .OrderBy(p => p)
         .Select(p => new OptionModel
         {
             Value = p,
             Text = p
         })
         .ToList();

        HttpContext.Session.SetString(
            "Ofertas",
            JsonSerializer.Serialize(ofertas));

        var ofertasFiltradas = programa == null ? ofertas : ofertas.Where(o => o.Programa == programa);

        var ofertasAsignadas = ofertasFiltradas
            .Where(o => o.NP != null)
            .ToList();

        var ofertasVacantes = ofertasFiltradas
            .Where(o => o.NP == null)
            .ToList();

        IEnumerable<DetallesArticuloDTO> articulos = await _articuloService.ObtenerTodosAsync();

        var articulosCombo = articulos
           .Select(a => new OptionModel
           {
               Value = a.IdArticulo.ToString(),
               Text = a.Numero,
           })
           .ToList();

        return new CargarProgramacionAcademica2ViewModel
        {
            OfertasVacantes = ofertasVacantes,
            OfertasAsignadas = ofertasAsignadas,

            TableAsignadas = await LlenarTablaAsync(TipoTablaOferta.Asignadas, articulos, ofertasAsignadas, programa),
            TableVacantes = await LlenarTablaAsync(TipoTablaOferta.Vacantes, articulos, ofertasVacantes, programa),
            TableCargas = await LlenarTablaCargasAsync(cargas),

            Articulos = articulosCombo,
            Programas = programasCombo,
            CargasAcademicas = cargas
        };
    }

    private async Task<TableModel> LlenarTablaAsync(TipoTablaOferta tipoOferta, IEnumerable<DetallesArticuloDTO> articulos, List<OfertaDTO>? ofertas, string? programa, AccionesDisponibles? permisos = null)
    {

        if (ofertas.Count() == 0)
            return TablaFactory.GenerarTablaConMensaje(tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS, string.Format(Constantes.TABLA_VACIA, Constantes.EXPERIENCIAS_EDUCATIVAS));


        List<TableActionModel> acciones = new();

        if (permisos != null)
        {
            if (permisos.Puede(Acciones.ProgramacionAcademica.VerHistorial))
                acciones.Add(new TableActionModel { Accion = "historial", OnClick = "abrirModalEditarOferta()" });

            if (permisos.Puede(Acciones.ProgramacionAcademica.Editar))
                acciones.Add(new TableActionModel { Accion = "editar", OnClick = "abrirModalEditarOferta()" });

            if (permisos.Puede(Acciones.ProgramacionAcademica.AsignarDocente))
                acciones.Add(new TableActionModel { Accion = tipoOferta == TipoTablaOferta.Asignadas ? "derecha" : "izquierda", OnClick = "abrirModalAsignarDocente()" });

            if (permisos.Puede(Acciones.ProgramacionAcademica.Ofertar) && tipoOferta == TipoTablaOferta.Vacantes)
                acciones.Add(new TableActionModel { Accion = "SwitchField" });

            if (permisos.Puede(Acciones.ProgramacionAcademica.Eliminar))
                acciones.Add(new TableActionModel { Accion = "eliminar", OnClick = "abrirModalConfirmacion('¿Desea eliminar esta oferta?', function() { eliminarOferta(); })" });
        }

        var headers = new List<string>(
            tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS
        );

        if (acciones.Any())
            headers.Add("Acciones");



        try
        {
            return new TableModel
            {
                TableId = tipoOferta == TipoTablaOferta.Vacantes ? "tablaVacantes" : "tablaAsignadas",
                Headers = headers,
                Rows = ofertas.Select(oferta =>
                {
                    var cells = new List<TableCellModel>
                    {
                        new() { Value = oferta.ExperienciaEducativa },
                        new() { Value = oferta.NRC },
                        new() { Value = oferta.HorasPago.ToString() },
                        new() { Value = oferta.TC },
                        new() { Actions = new List<TableActionModel>
                        {
                            new TableActionModel
                            {
                                Accion = "informacion",
                                OnClick = $"abrirModalHorario({JsonSerializer.Serialize(oferta)})"
                            }
                        }},
                        tipoOferta == TipoTablaOferta.Vacantes
                            ? new() { Value = articulos.FirstOrDefault(a => a.IdArticulo == oferta.Articulo)?.Numero ?? "—" }
                            : new() { Value = oferta.NombreDocente }
                    };

                    if (acciones.Any())
                    {
                        cells.Add(new TableCellModel { Actions = acciones });
                    }

                    return new TableRowModel { Cells = cells };
                }).ToList(),

                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = ofertas.Count,
                    OnPageChange = "cambiarPagina",
                    PaginationMode = "client"
                }
            };
        }
        catch (Exception)
        {
            return TablaFactory.GenerarTablaConMensaje(tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS, string.Format(Constantes.ERROR_TABLA, Constantes.EXPERIENCIAS_EDUCATIVAS));
        }
    }

    private async Task<TableModel> LlenarTablaCargasAsync(List<CargaConOfertaDTO>? cargas)
    {

        if (cargas.Count() == 0)
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_CARGAS, string.Format(Constantes.TABLA_VACIA, Constantes.EXPERIENCIAS_EDUCATIVAS));

        try
        {
            return new TableModel
            {
                TableId = "tablaCargas",
                Headers = HEADERS_TABLA_CARGAS,
                Rows = cargas.Select(carga => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                        {
                            new() { Value = carga.NumeroPersonal },
                            new() { Value = carga.NombreDocente },
                            new() { Value = carga.Plaza ?? "—" },
                            new() { Value = carga.Nrc ?? "—"},
                            new() { Value = carga.ExperienciaEducativa },
                            new() { Value = carga.HorasContacto.ToString() },
                            new() { Value = carga.HorasPago.ToString() },
                            new()
                            {
                                Value = carga.Imparte switch
                                {
                                    true => "SÍ",
                                    false => "NO",
                                    null => "—"
                                }
                            },

                    }
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = cargas.Count,
                    OnPageChange = "cambiarPagina",
                    PaginationMode = "client"
                }
            };
        }
        catch (Exception)
        {
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_CARGAS, string.Format(Constantes.ERROR_TABLA, Constantes.EXPERIENCIAS_EDUCATIVAS));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ValidarPaso1(CargarProgramacionAcademica1ViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            var erroresArchivos = ModelState
                .Where(x => x.Key == nameof(modelo.ArchivoVacantes)
                         || x.Key == nameof(modelo.ArchivoDescargas))
                .SelectMany(x => x.Value.Errors)
                .Select(e => e.ErrorMessage);

            TempData["Error"] = string.Join("|", erroresArchivos);

            await CargarCombos(modelo);
            return View("CargarProgramacionAcademicaPaso1", modelo);
        }

        try
        {
            var ofertasVacantes = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoVacantes, TipoArchivoOferta.Vacantes);
            var ofertasDescargas = await _programacionAcademicaService.ProcesarArchivoOfertasAsync(modelo.ArchivoDescargas, TipoArchivoOferta.Descargas);
            var cargas = await _programacionAcademicaService.ProcesarCargasAsync(modelo.ArchivoCargas);

            var todas = new List<OfertaDTO>();
            todas.AddRange(ofertasVacantes);
            todas.AddRange(ofertasDescargas);

            foreach (var oferta in todas)
            {
                oferta.IdPeriodo = modelo.IdPeriodo.Value;
            }

            HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(todas));
            HttpContext.Session.SetString("Cargas", JsonSerializer.Serialize(cargas));

            var periodoSeleccionado = (await _periodoEscolarService.ObtenerTodosAsync())
                .FirstOrDefault(p => p.IdPeriodoEscolar == modelo.IdPeriodo!.Value);
            var entidadSeleccionada = (await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(modelo.Region))
                .FirstOrDefault(e => e.IdEntidadAcademica == modelo.IdEntidadAcademica!.Value);

            HttpContext.Session.SetString(SESSION_REGION, modelo.Region);
            HttpContext.Session.SetInt32(SESSION_ID_PERIODO, modelo.IdPeriodo.Value);
            HttpContext.Session.SetString(SESSION_NOMBRE_PERIODO, periodoSeleccionado?.PeriodoMostrar ?? "");
            HttpContext.Session.SetInt32(SESSION_ID_ENTIDAD, modelo.IdEntidadAcademica.Value);
            HttpContext.Session.SetString(SESSION_NOMBRE_ENTIDAD, entidadSeleccionada?.Nombre ?? "");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar los archivos: {ex.Message}";
            await CargarCombos(modelo);
            return View("CargarProgramacionAcademicaPaso1", modelo);
        }
        return await CargarProgramacionAcademicaPaso2();
    }

    [HttpGet]
    public async Task<IActionResult> CargarProgramacionAcademicaPaso2()
    {
        var region = HttpContext.Session.GetString(SESSION_REGION);
        var idPeriodo = HttpContext.Session.GetInt32(SESSION_ID_PERIODO);
        var idEntidadAcademica = HttpContext.Session.GetInt32(SESSION_ID_ENTIDAD);

        if (string.IsNullOrEmpty(region) || idPeriodo is null || idEntidadAcademica is null)
        {
            TempData["Error"] = "No hay información de la carga. Vuelve a iniciar el proceso.";
            return RedirectToAction(nameof(CargarProgramacionAcademicaPaso1));
        }

        var vm = await ObtenerViewModelCompletoAsync();
        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    [HttpPost]
    public async Task<IActionResult> AsignarArticulo(int idArticulo)
    {
        var ofertas = ObtenerOfertasSesion();

        foreach (var oferta in ofertas)
            oferta.Articulo = idArticulo;

        HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(ofertas));

        var vm = await ObtenerViewModelCompletoAsync();
        vm.IdArticulo = idArticulo;

        return View("CargarProgramacionAcademicaPaso2", vm);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEntidades(string region)
    {
        var entidades = await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(region);
        var result = entidades.Select(e => new { value = e.IdEntidadAcademica, text = e.Nombre });
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProgramas(int entidad)
    {
        var programas = await _programacionAcademicaService.ObtenerOpcionesProgramaEducativoAsync(entidad);
        var result = programas.Select(p => new { value = p.IdProgramaEducativo, text = p.Nombre });
        return Json(result);
    }


    [HttpGet]
    public async Task<IActionResult> Ver(
    int idEntidadAcademica,
    int idProgramaEducativo,
    int idPeriodo,
    string? nombreEntidadAcademica,
    string? nombrePeriodo,
    string? nombrePrograma)
    {

        var permisos = MatrizPermisos.Para(User);



        var ofertas = await _programacionAcademicaService
            .ObtenerOfertasGuardadasAsync(idEntidadAcademica, idProgramaEducativo, idPeriodo);

        var ofertasAsignadas = ofertas.Where(o => o.NP != null).ToList();
        var ofertasVacantes = ofertas.Where(o => o.NP == null).ToList();

        IEnumerable<DetallesArticuloDTO> articulos = await _articuloService.ObtenerTodosAsync();

        var modelo = new VerProgramacionAcademicaViewModel(User)
        {
            Region = ofertas.First().Region,
            NombreEntidadAcademica = nombreEntidadAcademica,
            NombrePeriodo = nombrePeriodo,
            NombrePrograma = nombrePrograma,
            TableAsignadas = await LlenarTablaAsync(TipoTablaOferta.Asignadas, articulos, ofertasAsignadas, null, permisos),
            TableVacantes = await LlenarTablaAsync(TipoTablaOferta.Vacantes, articulos, ofertasVacantes, null, permisos)
        };

        return View("VerProgramacionAcademica", modelo);
    }

    [HttpPost]
    public async Task<IActionResult> EditarExperienciaEducativa(int idOferta)
    {
        var ofertas = ObtenerOfertasSesion();
        var oferta = ofertas.FirstOrDefault(o => o.IdOferta== idOferta);
        if (oferta == null)
            return NotFound("Oferta no encontrada.");
       
        HttpContext.Session.SetString("Ofertas", JsonSerializer.Serialize(ofertas));
        var vm = await ObtenerViewModelCompletoAsync();
        return View("FormularioExperienciaEducativa", vm);
    }

}