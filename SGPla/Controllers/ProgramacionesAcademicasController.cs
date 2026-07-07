using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private static List<string> HEADERS_TABLA_HORARIOS = ["Dîa", "Horario", "Salon", "Acciones"];
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

        HttpContext.Session.SetString("ResumenOferta", JsonSerializer.Serialize(resumen));

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
                                idPeriodo = r.IdPeriodo
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

    private async Task<TableModel> LlenarTablaAsync(
        TipoTablaOferta tipoOferta,
        IEnumerable<DetallesArticuloDTO> articulos,
        List<OfertaDTO>? ofertas,
        string? programa,
        AccionesDisponibles? permisos = null,
        int? idEntidadAcademica = null,
        int? idProgramaEducativo = null,
        int? idPeriodo = null)
    {

        if (ofertas.Count() == 0)
            return TablaFactory.GenerarTablaConMensaje(tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS, string.Format(Constantes.TABLA_VACIA, Constantes.EXPERIENCIAS_EDUCATIVAS));

        bool tieneAcciones = permisos != null && (
            permisos.Puede(Acciones.ProgramacionAcademica.VerHistorial) ||
            permisos.Puede(Acciones.ProgramacionAcademica.Editar) ||
            permisos.Puede(Acciones.ProgramacionAcademica.AsignarDocente) ||
            (permisos.Puede(Acciones.ProgramacionAcademica.Ofertar) && tipoOferta == TipoTablaOferta.Vacantes) ||
            permisos.Puede(Acciones.ProgramacionAcademica.Eliminar)
        );

        var headers = new List<string>(
            tipoOferta == TipoTablaOferta.Vacantes ? HEADERS_TABLA_VACANTES : HEADERS_TABLA_ASIGNADAS
        );

        if (tieneAcciones)
            headers.Add("Acciones");

        List<TableActionModel> ConstruirAcciones(OfertaDTO oferta)
        {
            var acciones = new List<TableActionModel>();
            if (permisos == null) return acciones;

            if (permisos.Puede(Acciones.ProgramacionAcademica.VerHistorial))
                acciones.Add(new TableActionModel
                {
                    Accion = "historial",
                    Url = Url.Action(nameof(VerHistorialExperienciaEducativa), "ProgramacionesAcademicas",
                    new { idOferta = oferta.IdOferta, oferta.ExperienciaEducativa })
                            });

            if (permisos.Puede(Acciones.ProgramacionAcademica.Editar))
                acciones.Add(new TableActionModel
                {
                    Accion = "editar",
                    OnClick = $"location.href='{Url.Action("EditarExperienciaEducativa",
                    new
                    {
                        oferta.IdOferta,
                        idEntidadAcademica,
                        idProgramaEducativo,
                        idPeriodo
                    })}'"
                });

            if (permisos.Puede(Acciones.ProgramacionAcademica.AsignarDocente))
            {
                if (tipoOferta == TipoTablaOferta.Asignadas)
                {
                    // "derecha": asignada -> vacante. Requiere confirmación + motivo.
                    acciones.Add(new TableActionModel
                    {
                        Accion = "derecha",
                        OnClick = $"abrirModalConfirmacionRetirarDocente('Esta Experiencia Educativa regresará a ser Vacante.', function(motivo) {{ cambiarAVacante({oferta.IdOferta}, motivo); }})"
                    });
                }
                else
                {
                    // "izquierda": vacante -> asignar docente. Comportamiento distinto, sin este modal.
                    acciones.Add(new TableActionModel
                    {
                        Accion = "izquierda",
                        OnClick = $"abrirModalConfirmacion('Se abrirá la pantalla para asignar un docente.', function() {{ abrirAsignacionDocente({oferta.IdOferta}); }})"
                    });
                }
            }

            if (permisos.Puede(Acciones.ProgramacionAcademica.Ofertar) && tipoOferta == TipoTablaOferta.Vacantes)
                acciones.Add(new TableActionModel
                {
                    Accion = "SwitchField",
                    Id = oferta.IdOferta.ToString(),
                    OnChange = "cambiarInclusion(this)",
                    Checked = oferta.Incluida
                });

            if (permisos.Puede(Acciones.ProgramacionAcademica.Eliminar))
                acciones.Add(new TableActionModel
                {
                    Accion = "eliminar",
                    OnClick = $"abrirModalConfirmacion('¿Desea eliminar esta oferta?', function() {{ eliminarOferta({oferta.IdOferta}); }})"
                });
            return acciones;
        }

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
                        new TableActionModel { Accion = "informacion", OnClick = $"abrirModalHorario({JsonSerializer.Serialize(oferta)})" }
                    }},
                    tipoOferta == TipoTablaOferta.Vacantes
                        ? new() { Value = articulos.FirstOrDefault(a => a.IdArticulo == oferta.Articulo)?.Numero ?? "—" }
                        : new() { Value = oferta.NombreDocente }
                };

                    if (tieneAcciones)
                        cells.Add(new TableCellModel { Actions = ConstruirAcciones(oferta) });

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
    public async Task<IActionResult> Ver(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo)
    {
        var permisos = MatrizPermisos.Para(User);

        var json = HttpContext.Session.GetString("ResumenOferta");
        var resumenGuardado = string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<ResumenOfertaProgramacionAcademicaDTO>>(json)!;

        var resumenActual = resumenGuardado.FirstOrDefault(r =>
            r.IdEntidadAcademica == idEntidadAcademica &&
            r.IdProgramaEducativo == idProgramaEducativo &&
            r.IdPeriodo == idPeriodo);

        var ofertas = await _programacionAcademicaService
            .ObtenerOfertasGuardadasAsync(idEntidadAcademica, idProgramaEducativo, idPeriodo);

        var ofertasAsignadas = ofertas.Where(o => o.NP != null).ToList();
        var ofertasVacantes = ofertas.Where(o => o.NP == null).ToList();

        IEnumerable<DetallesArticuloDTO> articulos = await _articuloService.ObtenerTodosAsync();

        var modelo = new VerProgramacionAcademicaViewModel(User)
        {
            Region = ofertas.First().Region,
            NombreEntidadAcademica = resumenActual?.EntidadAcademica,
            NombrePeriodo = resumenActual?.PeriodoMostrar,
            NombrePrograma = resumenActual?.ProgramaEducativo,
            TableAsignadas = await LlenarTablaAsync(TipoTablaOferta.Asignadas, articulos, ofertasAsignadas, null, permisos, idEntidadAcademica, idProgramaEducativo, idPeriodo),
            TableVacantes = await LlenarTablaAsync(TipoTablaOferta.Vacantes, articulos, ofertasVacantes, null, permisos, idEntidadAcademica, idProgramaEducativo, idPeriodo)
        };

        return View("VerProgramacionAcademica", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> EditarExperienciaEducativa(
        int idOferta,
        int idEntidadAcademica,
        int idProgramaEducativo,
        int idPeriodo)
    {
        var oferta = await _programacionAcademicaService.ObtenerOfertaPorId(idOferta);

        if (oferta == null)
        {
            return NotFound();
        }

        var tiposContratacion = Constantes.TIPOS_CONTRATACION
                .Select(r => new OptionModel { Value = r, Text = r, Selected = r == oferta.TC })
                .ToList();

        var horarios = new List<(string Dia, HorarioDia Horario)>
    {
        ("Lunes", oferta.Lunes),
        ("Martes", oferta.Martes),
        ("Miércoles", oferta.Miercoles),
        ("Jueves", oferta.Jueves),
        ("Viernes", oferta.Viernes),
        ("Sábado", oferta.Sabado),
    }
          .Where(h => h.Horario != null)
          .ToList();

        var tabla = LlenarTablaHorario(horarios);

        var model = new FormularioExperienciaEducativaViewModel
        {
            NombreProgramaEducativo = oferta.Programa,
            NombreExperienciaEducativa = oferta.ExperienciaEducativa,
            Nrc = oferta.NRC,
            TipoContratacion = oferta.TC,
            Horas = oferta.HorasPago,
            Modalidad = oferta.Modalidad,
            IdExperienciaEducativa = oferta.IdOferta,
            TiposContratacion = tiposContratacion,
            Horarios = tabla,
            IdEntidadAcademica = idEntidadAcademica,
            IdProgramaEducativo = idProgramaEducativo,
            IdPeriodo = idPeriodo
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditarExperienciaEducativa(FormularioExperienciaEducativaViewModel model)
    {
        var oferta = await _programacionAcademicaService.ObtenerOfertaPorId(model.IdExperienciaEducativa);
        if (oferta == null)
        {
            return NotFound();
        }

        oferta.TC = model.TipoContratacion;
        oferta.NRC = model.Nrc;

        await _programacionAcademicaService.EditarOfertaAsync(model.IdExperienciaEducativa, oferta);
        TempData["Success"] = "La experiencia educativa ha sido actualizada correctamente.";

        return RedirectToAction("Ver", new
        {
            idEntidadAcademica = model.IdEntidadAcademica,
            idProgramaEducativo = model.IdProgramaEducativo,
            idPeriodo = model.IdPeriodo
        });
    }

    private TableModel LlenarTablaHorario(List<(string Dia, HorarioDia Horario)> horarios)
    {
        if (horarios.Count == 0)
            return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_HORARIOS, string.Format(Constantes.TABLA_VACIA, Constantes.PROGRAMACIONES_ACADEMICAS));

        try
        {
            return new TableModel
            {
                TableId = "tablaHorarios",
                Headers = HEADERS_TABLA_HORARIOS,
                Rows = horarios.Select(h => new TableRowModel
                {
                    Cells = new List<TableCellModel>
                {
                    new() { Value = h.Dia },
                    new() { Value = h.Horario.ToString() },
                    new() { Value = h.Horario.Salon ?? "No asignado" },
                    new()
                    {
                        Actions = new List<TableActionModel>
                        {
                            new TableActionModel { Accion = "editar" },
                            new TableActionModel { Accion = "eliminar" }
                        }
                    }
                }
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = _paginaActual,
                    TotalItems = horarios.Count,
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
    public async Task<IActionResult> VerHistorialExperienciaEducativa(int idOferta, string experienciaEducativa)
    {
        var mensajes = await _programacionAcademicaService.ObtenerHistorialPorIdOferta(idOferta);

        VerHistorialExperienciaEducativaViewModel vm = new VerHistorialExperienciaEducativaViewModel();
        vm.ExperienciaEducativa = experienciaEducativa;
        vm.Movimientos = mensajes;

        TimeZoneInfo zona;
        try
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (TimeZoneNotFoundException)
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("Mexico Standard Time");
        }

        foreach (var movimiento in vm.Movimientos)
        {
            var fechaUtc = DateTime.SpecifyKind(movimiento.Fecha, DateTimeKind.Utc);
            movimiento.Fecha = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, zona);
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarInclusionOferta([FromBody] OfertaDTO dto)
    {
        if (dto == null || dto.IdOferta <= 0)
            return BadRequest(new { mensaje = "Datos inválidos" });

        try
        {
            await _programacionAcademicaService.CambiarInclusionOfertaAsync(dto.IdOferta, dto.Incluida);
            return Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar oferta {IdOferta}", dto.IdOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error al guardar los cambios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar oferta {IdOferta}", dto.IdOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EliminarOferta([FromBody] int idOferta)
    {
        if (idOferta <= 0)
            return BadRequest(new { mensaje = "ID de oferta inválido" });
        try
        {
            await _programacionAcademicaService.EliminarOfertaAsync(idOferta);
            TempData["Success"] = "La oferta ha sido eliminada correctamente.";
            return Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar oferta {IdOferta}", idOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error al eliminar la oferta" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al eliminar oferta {IdOferta}", idOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado" });
        }
    }

    public class CambiarAVacanteRequest
    {
        public int IdOferta { get; set; }
        public string? Motivo { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CambiarAVacante([FromBody] CambiarAVacanteRequest request)
    {
        if (request == null || request.IdOferta <= 0)
            return BadRequest(new { mensaje = "ID de oferta inválido" });
        try
        {
            var motivo = string.IsNullOrWhiteSpace(request.Motivo) ? "Retiro de docente" : request.Motivo;
            await _programacionAcademicaService.CambiarAVacanteAsync(request.IdOferta, motivo);
            TempData["Success"] = "El docente ha sido retirado correctamente.";
            return Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al retirar docente de oferta {IdOferta}", request.IdOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error al retirar el docente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al retirar docente de oferta {IdOferta}", request.IdOferta);
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AsignarDocente(int idOferta)
    {
        // Obtener la información necesaria
        // Crear el ViewModel
        return RedirectToAction("Index", "Docentes");
    }
}