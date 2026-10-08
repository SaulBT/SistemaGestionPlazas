using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Helpers;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models.DTOs.Planea;
using SGPla.Models.DTOs.ProgramacionAcademica;
using SGPla.Models.ViewModels.ProgramacionesAcademicas;
using SGPla.Services.Interfaces;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using static SGPla.Services.Implementations.ClavesEstado.ProgramacionAcademica;

namespace SGPla.Controllers;

enum TipoTablaOferta
{
    Asignadas,
    Vacantes
}

[Authorize]
public class ProgramacionesAcademicasController : Controller
{
    // ── Dependencias ────────────────────────────────────────────────────

    private readonly ILogger<ProgramacionesAcademicasController> _logger;
    private readonly IProgramacionAcademicaService _programacionAcademicaService;
    private readonly IPeriodoEscolarService _periodoEscolarService;
    private readonly IEstadoNavegacion _estado;
    private readonly IProgramacionPlaneaService _programacionPlaneaService;
    private int _paginaActual = 1;


    private static readonly List<string> HEADERS_TABLA_ASIGNADAS =
        ["NRC", "Experiencia educativa", "H/S/M", "Tipo contratación", "Horario", "Docente"];
    private static readonly List<string> HEADERS_TABLA_VACANTES =
        ["NRC", "Experiencia educativa", "H/S/M", "Tipo contratación", "Horario"];
    private static readonly List<string> HEADERS_TABLA_RESUMEN_OFERTA =
        ["Entidad Academica", "Programa Educativo", "Modalidad", "Código de plan", "EE Convocadas", "EE Vacantes", "Acciones"];
    private static readonly List<string> HEADERS_TABLA_HORARIOS =
        ["Dîa", "Horario", "Salon", "Acciones"];
    private static readonly List<string> HEADERS_TABLA_PLANEA =
        ["", "NRC", "Experiencia educativa", "Horario y espacio", "Imparte", "Docente", "Estado"];
    private static readonly Dictionary<string, string> ABREVIATURA_DIA = new()
    {
        ["Lunes"] = "Lun", ["Martes"] = "Mar", ["Miercoles"] = "Mié",
        ["Jueves"] = "Jue", ["Viernes"] = "Vie", ["Sabado"] = "Sáb"
    };

    public ProgramacionesAcademicasController(
        ILogger<ProgramacionesAcademicasController> logger,
        IProgramacionAcademicaService programacionAcademicaService,
        IPeriodoEscolarService periodoEscolarService,
        IEstadoNavegacion estado,
        IProgramacionPlaneaService programacionPlaneaService)
    {
        _logger = logger;
        _programacionAcademicaService = programacionAcademicaService;
        _periodoEscolarService = periodoEscolarService;
        _estado = estado;
        _programacionPlaneaService = programacionPlaneaService;
    }

    #region Índice y resumen de programaciones

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
    public async Task<IActionResult> Index(
        string? region,
        int? idEntidadAcademica,
        int? idProgramaEducativo,
        int? idPeriodo,
        string? busqueda)
    {
        // Solo DGAA ve la programación de PLANEA; la entidad académica ve únicamente la ya aprobada (Oferta).
        var esDgaa = User.IsInRole(Constantes.COORDINADOR_DGAA);

        var filtro = new BuscarProgramacionAcademicaDTO
        {
            IncluirPlanea = esDgaa,
            Region = region,
            IdEntidadAcademica = idEntidadAcademica,
            IdProgramaEducativo = idProgramaEducativo,
            // Sin periodo elegido se muestra el más reciente con programación; los anteriores se consultan con el filtro.
            IdPeriodo = idPeriodo ?? await _programacionAcademicaService.ObtenerPeriodoActualAsync(esDgaa),
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

        _estado.Guardar(ResumenOferta, resumen);

        return View("Index", modelo);
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    public async Task<IActionResult> ProgramacionPlanea(int idPlanEstudios, int idPeriodo, string? busqueda)
    {
        var encabezado = await _programacionPlaneaService.ObtenerEncabezadoAsync(idPlanEstudios, idPeriodo);
        if (encabezado is null)
            return NotFound();

        // Todas las filas llegan al DOM (paginación client) para que el form envíe todos los checkboxes.
        var planea = await _programacionPlaneaService.ObtenerParaAprobarAsync(idPlanEstudios, idPeriodo, busqueda);

        var modelo = new ProgramacionPlaneaViewModel
        {
            IdPlanEstudios = idPlanEstudios,
            IdPeriodo = idPeriodo,
            Region = encabezado.Region,
            NombreEntidadAcademica = encabezado.EntidadAcademica,
            NombrePrograma = encabezado.ProgramaEducativo,
            CodigoPlan = encabezado.CodigoPlan,
            NombrePeriodo = encabezado.PeriodoMostrar,
            UltimaSincronizacionPlanea = planea.UltimaSincronizacion,
            Resumen = planea.Resumen,
            HayPendientes = planea.Resumen.Pendientes > 0,
            UltimaRevisionLocal = planea.Resumen.UltimaRevision is { } revision ? ConvertirAHoraDeMexico(revision) : null,
            TablaPlanea = LlenarTablaPlanea(planea.Copias)
        };

        return View("ProgramacionPlanea", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    public async Task<IActionResult> AprobarProgramacionPlanea(int idPlanEstudios, int idPeriodo, int[]? idsAprobados)
    {
        var revisadoPor = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "DGAA";
        if (revisadoPor.Length > 150) revisadoPor = revisadoPor[..150];

        var (exito, mensaje, resultado) = await _programacionPlaneaService.AprobarAsync(
            idPlanEstudios, idPeriodo, idsAprobados ?? [], revisadoPor);

        if (!exito || resultado is null)
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(ProgramacionPlanea), new { idPlanEstudios, idPeriodo });
        }

        var aprobados = resultado.OfertasCreadas + resultado.Enlazadas;
        TempData["Success"] = $"Programación confirmada: {aprobados} NRC aprobados, {resultado.Descartadas} descartados.";
        return RedirectToAction(nameof(Index), new { idPeriodo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
    public async Task<IActionResult> RestaurarCopiaPlanea(int idExperienciaEducativaPeriodo, int idPlanEstudios, int idPeriodo)
    {
        if (await _programacionPlaneaService.RestaurarAsync(idExperienciaEducativaPeriodo))
            TempData["Success"] = "El NRC volvió a quedar pendiente de confirmación.";
        else
            TempData["Error"] = "No se pudo restaurar el NRC: ya no está descartado.";

        return RedirectToAction(nameof(ProgramacionPlanea), new { idPlanEstudios, idPeriodo });
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
    public async Task<IActionResult> ObtenerEntidades(string region)
    {
        var entidades = await _programacionAcademicaService.ObtenerOpcionesEntidadAcademicaAsync(region);
        var result = entidades.Select(e => new { value = e.IdEntidadAcademica, text = e.Nombre });
        return Json(result);
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.OperadorAcademico)]
    public async Task<IActionResult> ObtenerProgramas(int entidad)
    {
        var programas = await _programacionAcademicaService.ObtenerOpcionesProgramaEducativoAsync(entidad);
        var result = programas.Select(p => new { value = p.IdProgramaEducativo, text = p.Nombre });
        return Json(result);
    }

    #endregion

    #region Ver / Detalle de una programación

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    public async Task<IActionResult> Ver(int idEntidadAcademica, int idProgramaEducativo, int idPeriodo, string? busqueda = null)
    {
        var permisos = MatrizPermisos.Para(User);

        var resumenGuardado = _estado.Obtener<List<ResumenOfertaProgramacionAcademicaDTO>>(ResumenOferta) ?? [];

        var resumenActual = resumenGuardado.FirstOrDefault(r =>
            r.IdEntidadAcademica == idEntidadAcademica &&
            r.IdProgramaEducativo == idProgramaEducativo &&
            r.IdPeriodo == idPeriodo);

        var ofertas = await _programacionAcademicaService
            .ObtenerOfertasExperienciasEducativasAsync(idEntidadAcademica, idProgramaEducativo, idPeriodo, busqueda);

        var ofertasAsignadas = ofertas.Where(o => o.TieneDocente).ToList();
        var ofertasVacantes = ofertas.Where(o => !o.TieneDocente).ToList();

        var modelo = new VerProgramacionAcademicaViewModel(User)
        {
            Region = ofertas.First().Region,
            NombreEntidadAcademica = resumenActual?.EntidadAcademica,
            NombrePeriodo = resumenActual?.PeriodoMostrar,
            NombrePrograma = resumenActual?.ProgramaEducativo,
            TableAsignadas = await LlenarTablaAsync(TipoTablaOferta.Asignadas, ofertasAsignadas, null, permisos, idEntidadAcademica, idProgramaEducativo, idPeriodo),
            TableVacantes = await LlenarTablaAsync(TipoTablaOferta.Vacantes, ofertasVacantes, null, permisos, idEntidadAcademica, idProgramaEducativo, idPeriodo),
            IdEntidadAcademica = idEntidadAcademica,
            IdPeriodo = idPeriodo,
            IdProgramaEducativo = idProgramaEducativo
        };

        return View("VerProgramacionAcademica", modelo);
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    public async Task<IActionResult> VerHistorialExperienciaEducativa(
        int idOferta,
        string experienciaEducativa,
        int idEntidadAcademica,
        int idProgramaEducativo,
        int idPeriodo)
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

        ViewBag.RegresarUrl = Url.Action("Ver", "ProgramacionesAcademicas",
            new { idEntidadAcademica, idProgramaEducativo, idPeriodo });

        return View(vm);
    }

    #endregion

    #region Edición de experiencia educativa

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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

    #endregion

    #region Acciones AJAX sobre una oferta (incluir, eliminar, cambiar a vacante)

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
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

    #endregion

    #region Asignar docente (navega a Docentes con contexto guardado)

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
    public IActionResult AsignarDocente(int idOferta, int idEntidadAcademica, int idProgramaEducativo, int idPeriodo)
    {
        _estado.Guardar(IdOfertaAsignar, idOferta);

        var urlActual = Url.Action("Ver", "ProgramacionesAcademicas",
            new { idEntidadAcademica, idProgramaEducativo, idPeriodo })!;

        _estado.PushRetorno(urlActual);

        return RedirectToAction("AsignarDocente", "Docentes", new { idOferta });
    }

    #endregion

    #region Helpers privados — construcción de tablas

    private static DateTime ConvertirAHoraDeMexico(DateTime fechaUtc)
    {
        TimeZoneInfo zona;
        try
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (TimeZoneNotFoundException)
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("Mexico Standard Time");
        }

        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc), zona);
    }

    private static TableModel LlenarTablaPlanea(IReadOnlyList<CopiaProgramacionPlaneaDTO> copias)
    {
        if (copias.Count == 0)
            return TablaFactory.GenerarTablaConMensajeSinPaginacion(HEADERS_TABLA_PLANEA,
                "No se encontraron NRC de PLANEA para este programa educativo y periodo.");

        return new TableModel
        {
            TableId = "tablaPlanea",
            Headers = HEADERS_TABLA_PLANEA,
            Pagination = new PaginationInfo { PageSize = 10, TotalItems = copias.Count, PaginationMode = "client" },
            Rows = copias.Select(c =>
            {
                var pendiente = c.EstadoAprobacion == PlaneaConstantes.APROBACION_PENDIENTE;
                var estado = new TableCellModel { Value = c.EstadoAprobacion };
                if (c.EstadoAprobacion == PlaneaConstantes.APROBACION_DESCARTADA)
                {
                    estado.Actions =
                    [
                        new()
                        {
                            Accion = "izquierda",
                            AriaLabel = "Restaurar",
                            OnClick = $"restaurarCopiaPlanea({c.IdExperienciaEducativaPeriodo})"
                        }
                    ];
                }

                return new TableRowModel
                {
                    Cells =
                    [
                        new()
                        {
                            IsCheckBox = true,
                            CheckboxName = "idsAprobados",
                            CheckboxValue = c.IdExperienciaEducativaPeriodo.ToString(),
                            // Pendiente: marcada y editable; Aprobada: marcada y fija; Descartada: desmarcada y fija.
                            Checked = c.EstadoAprobacion != PlaneaConstantes.APROBACION_DESCARTADA,
                            CheckboxDisabled = !pendiente
                        },
                        new() { Value = c.Nrc },
                        new() { Value = $"{c.CodigoExperiencia} - {c.NombreExperiencia}" },
                        new() { Value = FormatearHorarios(c.Horarios) },
                        new() { Value = FormatearImparte(c.Docentes) },
                        new() { Value = FormatearDocentes(c.Docentes) },
                        estado
                    ]
                };
            }).ToList()
        };
    }

    // Con varios docentes en el NRC basta con que uno imparta para marcarlo como «SÍ».
    private static string FormatearImparte(IReadOnlyList<DocentePlaneaDTO> docentes)
    {
        if (docentes.Any(d => d.Imparte == true)) return "SÍ";
        if (docentes.Any(d => d.Imparte == false)) return "NO";
        return "—";
    }

    private static string FormatearDocentes(IReadOnlyList<DocentePlaneaDTO> docentes)
    {
        var nombres = docentes.Select(d => d.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
        return nombres.Count == 0 ? "Sin asignar" : string.Join("; ", nombres);
    }

    private static string FormatearHorarios(IReadOnlyList<HorarioPlaneaDTO> horarios)
    {
        if (horarios.Count == 0) return "Sin horario";
        return string.Join("; ", horarios.Select(h =>
        {
            var dia = ABREVIATURA_DIA.GetValueOrDefault(h.Dia, h.Dia);
            var espacio = string.Join("/", new[] { h.Edificio, h.Aula }.Where(x => !string.IsNullOrWhiteSpace(x)));
            var horas = $"{h.HoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture)}-{h.HoraFin.ToString("HH:mm", CultureInfo.InvariantCulture)}";
            return string.IsNullOrEmpty(espacio) ? $"{dia} {horas}" : $"{dia} {horas} ({espacio})";
        }));
    }

    private static string DescribirEstadoPlanea(ResumenOfertaProgramacionAcademicaDTO r)
    {
        if (!r.TieneProgramacionPlanea) return "—";
        if (r.NrcPendientes > 0 && r.NrcAprobados > 0) return $"Aprobada · {r.NrcPendientes} nuevos";
        if (r.NrcPendientes > 0) return $"Pendiente ({r.NrcPendientes})";
        if (r.NrcAprobados > 0) return "Aprobada";
        return "Descartada";
    }

    // «Ingeniería de Software (2014)»: distingue los planes de un mismo programa.
    private static string NombreProgramaConPlan(ResumenOfertaProgramacionAcademicaDTO resumen)
    {
        return string.IsNullOrWhiteSpace(resumen.NombrePlan)
            ? resumen.ProgramaEducativo
            : $"{resumen.ProgramaEducativo} ({resumen.NombrePlan.Trim()})";
    }

    private TableModel LlenarTablaResumen(List<ResumenOfertaProgramacionAcademicaDTO> resumen)
    {
        bool esCoordinadorEa = User.IsInRole(Constantes.COORDINADOR_EA);
        bool esDgaa = User.IsInRole(Constantes.COORDINADOR_DGAA);

        var headers = HEADERS_TABLA_RESUMEN_OFERTA.ToList();
        if (esDgaa)
            headers.Insert(headers.Count - 1, "Estado PLANEA");

        if (resumen.Count == 0)
        {
            return TablaFactory.GenerarTablaConMensaje(
                headers,
                string.Format(
                    Constantes.TABLA_VACIA,
                    Constantes.PROGRAMACIONES_ACADEMICAS));
        }

        try
        {
            return new TableModel
            {
                TableId = "tablaResumenOferta",
                Headers = headers,
                Rows = resumen.Select(r =>
                {
                    var cells = new List<TableCellModel>
                    {
                        new() { Value = r.EntidadAcademica },
                        new() { Value = NombreProgramaConPlan(r) },
                        new() { Value = string.IsNullOrWhiteSpace(r.ModalidadPlan) ? "—" : r.ModalidadPlan },
                        new() { Value = r.CodigoPlan ?? "—" },
                        new() { Value = r.EEAsignadas.ToString() },
                        new() { Value = r.EEVacantes.ToString() }
                    };

                    if (esDgaa)
                        cells.Add(new() { Value = DescribirEstadoPlanea(r) });

                    var acciones = new List<TableActionModel>();

                    if (esDgaa && r.TieneProgramacionPlanea && r.IdPlanEstudios.HasValue)
                    {
                        acciones.Add(new()
                        {
                            Accion = "planea",
                            AriaLabel = r.NrcPendientes > 0 ? "Revisar programación PLANEA" : "Ver programación PLANEA",
                            Url = Url.Action(nameof(ProgramacionPlanea), new
                            {
                                idPlanEstudios = r.IdPlanEstudios,
                                idPeriodo = r.IdPeriodo
                            })
                        });
                    }

                    // «Ver» y «Solicitudes» trabajan sobre la oferta; sin oferta cargada no hay nada que mostrar.
                    if (esCoordinadorEa && r.TotalEE > 0)
                    {
                        acciones.Add(new()
                        {
                            Accion = "ver",
                            AriaLabel = "Ver programación académica",
                            Url = Url.Action("Ver", new
                            {
                                idEntidadAcademica = r.IdEntidadAcademica,
                                idProgramaEducativo = r.IdProgramaEducativo,
                                idPeriodo = r.IdPeriodo
                            })
                        });
                        acciones.Add(new()
                        {
                            Accion = "solicitudes",
                            AriaLabel = "Ver solicitudes"
                        });
                    }

                    cells.Add(new TableCellModel { Actions = acciones });

                    return new TableRowModel
                    {
                        Cells = cells
                    };
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
            return TablaFactory.GenerarTablaConMensaje(
                headers,
                string.Format(
                    Constantes.ERROR_TABLA,
                    Constantes.PROGRAMACIONES_ACADEMICAS));
        }
    }

    private async Task<TableModel> LlenarTablaAsync(
        TipoTablaOferta tipoOferta,
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
        {
            headers.Add("Acciones");

            if (tipoOferta == TipoTablaOferta.Vacantes)
                headers.Add("Incluir");
        }

        List<TableActionModel> ConstruirAcciones(OfertaDTO oferta)
        {
            var acciones = new List<TableActionModel>();
            if (permisos == null) return acciones;

            if (permisos.Puede(Acciones.ProgramacionAcademica.VerHistorial))
                acciones.Add(new TableActionModel
                {
                    Accion = "historial",
                    Url = Url.Action(nameof(VerHistorialExperienciaEducativa), "ProgramacionesAcademicas",
                    new
                    {
                        idOferta = oferta.IdOferta,
                        oferta.ExperienciaEducativa,
                        idEntidadAcademica,
                        idProgramaEducativo,
                        idPeriodo
                    })
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
                    // "izquierda": asignada -> vacante. Requiere confirmación + motivo.
                    acciones.Add(new TableActionModel
                    {
                        Accion = "izquierda",
                        OnClick = $"abrirModalConfirmacionRetirarDocente('Esta Experiencia Educativa regresará a ser Vacante.', function(motivo) {{ cambiarAVacante({oferta.IdOferta}, motivo); }})"
                    });
                }
                else
                {
                    // "derecha": vacante -> asignar docente.
                    acciones.Add(new TableActionModel
                    {
                        Accion = "derecha",
                        OnClick = $"location.href='{Url.Action("AsignarDocente", "ProgramacionesAcademicas", new
                        {
                            idOferta = oferta.IdOferta,
                            idEntidadAcademica,
                            idProgramaEducativo,
                            idPeriodo
                        })}'"
                    });
                }
            }

            if (permisos.Puede(Acciones.ProgramacionAcademica.Eliminar))
                acciones.Add(new TableActionModel
                {
                    Accion = "eliminar",
                    OnClick = $"abrirModalConfirmacion('¿Desea eliminar esta oferta?', function() {{ eliminarOferta({oferta.IdOferta}); }})"
                });
            return acciones;
        }

        List<TableActionModel> ConstruirIncluir(OfertaDTO oferta)
        {
            var acciones = new List<TableActionModel>();
            if (permisos == null) return acciones;

            if (permisos.Puede(Acciones.ProgramacionAcademica.Ofertar) && tipoOferta == TipoTablaOferta.Vacantes)
                acciones.Add(new TableActionModel
                {
                    Accion = "SwitchField",
                    Id = oferta.IdOferta.ToString(),
                    OnChange = "cambiarInclusion(this)",
                    Checked = oferta.Incluida
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
                        new() { Value = oferta.NRC },
                        new() { Value = oferta.ExperienciaEducativa },
                        new() { Value = oferta.HorasPago.ToString() },
                        new() { Value = oferta.TC },
                        new() { Actions = new List<TableActionModel>
                        {
                            new TableActionModel { Accion = "informacion", OnClick = $"abrirModalHorario({JsonSerializer.Serialize(oferta)})" }
                        }}
                    };

                    if (tipoOferta == TipoTablaOferta.Asignadas)
                    {
                        cells.Add(new() { Value = oferta.NombreDocente });
                    }

                    if (tieneAcciones)
                        cells.Add(new TableCellModel { Actions = ConstruirAcciones(oferta) });
                    if (tieneAcciones && tipoOferta == TipoTablaOferta.Vacantes)
                        cells.Add(new TableCellModel { Actions = ConstruirIncluir(oferta) });

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

    #endregion
}
