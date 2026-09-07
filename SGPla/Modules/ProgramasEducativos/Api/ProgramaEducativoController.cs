using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Ports;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Ports;

namespace SGPla.Modules.ProgramasEducativos.Api;

[ApiController]
[Route("api/v1/programas-educativos")]
[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class ProgramaEducativoController : ControllerBase
{
    private readonly IConsultarProgramasEducativosService _consultarProgramasService;
    private readonly IConsultarProgramaEducativoService _consultarProgramaService;
    private readonly ICrearProgramaEducativoService _crearProgramaService;
    private readonly IActualizarProgramaEducativoService _actualizarProgramaService;
    private readonly IEliminarProgramaEducativoService _eliminarProgramaService;

    public ProgramaEducativoController(
        IConsultarProgramasEducativosService consultarProgramasService,
        IConsultarProgramaEducativoService consultarProgramaService,
        ICrearProgramaEducativoService crearProgramaService,
        IActualizarProgramaEducativoService actualizarProgramaService,
        IEliminarProgramaEducativoService eliminarProgramaService)
    {
        _consultarProgramasService = consultarProgramasService;
        _consultarProgramaService = consultarProgramaService;
        _crearProgramaService = crearProgramaService;
        _actualizarProgramaService = actualizarProgramaService;
        _eliminarProgramaService = eliminarProgramaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProgramasEducativosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerTodosAsync(
        [FromQuery] string? busqueda,
        [FromQuery] string? region,
        [FromQuery] int? idAreaAcademica,
        [FromQuery] int? idEntidadAcademica,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarProgramasService.ConsultarAsync(
            new ConsultarProgramasEducativosQuery(
                busqueda,
                region,
                idAreaAcademica,
                idEntidadAcademica,
                pagina,
                cantidad),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito => Ok(resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            _ => Problem("El resultado de la consulta de programas educativos no es válido.")
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProgramaEducativoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarProgramaService.ConsultarAsync(
            new ConsultarProgramaEducativoQuery(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito => Ok(resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la consulta del programa educativo no es válido.")
        };
    }

    [HttpPost]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(ProgramaEducativoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody] CrearProgramaEducativoRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _crearProgramaService.CrearAsync(
            new CrearProgramaEducativoCommand(
                request.Nombre,
                request.Campus,
                request.IdEntidadAcademica),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoProgramaEducativo.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la creación del programa educativo no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(ProgramaEducativoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromBody] ActualizarProgramaEducativoRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _actualizarProgramaService.ActualizarAsync(
            new ActualizarProgramaEducativoCommand(
                id,
                request.Nombre,
                request.Campus,
                request.IdEntidadAcademica),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito => Ok(resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoProgramaEducativo.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la actualización del programa educativo no es válido.")
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _eliminarProgramaService.EliminarAsync(
            new EliminarProgramaEducativoCommand(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito => NoContent(),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la eliminación del programa educativo no es válido.")
        };
    }

    private static ActionResult CrearRespuestaValidacion<T>(
        ProgramaEducativoResultado<T> resultado)
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
        ProgramaEducativoResultado<T> resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
