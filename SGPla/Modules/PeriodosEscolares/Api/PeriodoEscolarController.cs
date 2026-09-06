using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Ports;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Contracts;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Ports;

namespace SGPla.Modules.PeriodosEscolares.Api;

[ApiController]
[Route("api/v1/periodos-escolares")]
[Authorize]
public sealed class PeriodoEscolarController : ControllerBase
{
    private readonly IConsultarPeriodosEscolaresService _consultarPeriodosService;
    private readonly IConsultarPeriodoEscolarService _consultarPeriodoService;
    private readonly ICrearPeriodoEscolarService _crearPeriodoService;
    private readonly IActualizarPeriodoEscolarService _actualizarPeriodoService;
    private readonly IEliminarPeriodoEscolarService _eliminarPeriodoService;

    public PeriodoEscolarController(
        IConsultarPeriodosEscolaresService consultarPeriodosService,
        IConsultarPeriodoEscolarService consultarPeriodoService,
        ICrearPeriodoEscolarService crearPeriodoService,
        IActualizarPeriodoEscolarService actualizarPeriodoService,
        IEliminarPeriodoEscolarService eliminarPeriodoService)
    {
        _consultarPeriodosService = consultarPeriodosService;
        _consultarPeriodoService = consultarPeriodoService;
        _crearPeriodoService = crearPeriodoService;
        _actualizarPeriodoService = actualizarPeriodoService;
        _eliminarPeriodoService = eliminarPeriodoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PeriodosEscolaresResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerTodosAsync(
        [FromQuery] int? anio,
        [FromQuery] string? periodo,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarPeriodosService.ConsultarAsync(
            new ConsultarPeriodosEscolaresQuery(anio, periodo, pagina, cantidad),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoPeriodoEscolar.Exito => Ok(resultado.Respuesta),
            TipoResultadoPeriodoEscolar.Validacion => CrearRespuestaValidacion(resultado),
            _ => Problem("El resultado de la consulta de periodos escolares no es válido.")
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PeriodoEscolarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarPeriodoService.ConsultarAsync(
            new ConsultarPeriodoEscolarQuery(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoPeriodoEscolar.Exito => Ok(resultado.Respuesta),
            TipoResultadoPeriodoEscolar.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoPeriodoEscolar.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la consulta del periodo escolar no es válido.")
        };
    }

    [HttpPost]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(PeriodoEscolarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody] CrearPeriodoEscolarRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _crearPeriodoService.CrearAsync(
            new CrearPeriodoEscolarCommand(request.Anio, request.Periodo),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoPeriodoEscolar.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoPeriodoEscolar.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoPeriodoEscolar.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la creación del periodo escolar no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(PeriodoEscolarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromBody] ActualizarPeriodoEscolarRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _actualizarPeriodoService.ActualizarAsync(
            new ActualizarPeriodoEscolarCommand(id, request.Anio, request.Periodo),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoPeriodoEscolar.Exito => Ok(resultado.Respuesta),
            TipoResultadoPeriodoEscolar.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoPeriodoEscolar.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoPeriodoEscolar.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la actualización del periodo escolar no es válido.")
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _eliminarPeriodoService.EliminarAsync(
            new EliminarPeriodoEscolarCommand(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoPeriodoEscolar.Exito => NoContent(),
            TipoResultadoPeriodoEscolar.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoPeriodoEscolar.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoPeriodoEscolar.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la eliminación del periodo escolar no es válido.")
        };
    }

    private static ActionResult CrearRespuestaValidacion<T>(
        PeriodoEscolarResultado<T> resultado)
    {
        var campo = resultado.Campo ?? string.Empty;
        var mensaje = resultado.Mensaje
            ?? "La información proporcionada no es válida.";
        var errores = new Dictionary<string, string[]>
        {
            [campo] = new[] { mensaje }
        };

        return new BadRequestObjectResult(new ValidationProblemDetails(errores));
    }

    private static ProblemDetails CrearProblema<T>(
        PeriodoEscolarResultado<T> resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
