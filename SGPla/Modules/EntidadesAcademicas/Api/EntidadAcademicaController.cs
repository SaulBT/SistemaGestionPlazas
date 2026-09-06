using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Ports;

namespace SGPla.Modules.EntidadesAcademicas.Api;

[ApiController]
[Route("api/v1/entidades-academicas")]
[Authorize]
public sealed class EntidadAcademicaController : ControllerBase
{
    private readonly IConsultarEntidadesAcademicasService _consultarEntidadesService;
    private readonly IConsultarEntidadAcademicaService _consultarEntidadService;
    private readonly ICrearEntidadAcademicaService _crearEntidadService;
    private readonly IActualizarEntidadAcademicaService _actualizarEntidadService;
    private readonly IEliminarEntidadAcademicaService _eliminarEntidadService;

    public EntidadAcademicaController(
        IConsultarEntidadesAcademicasService consultarEntidadesService,
        IConsultarEntidadAcademicaService consultarEntidadService,
        ICrearEntidadAcademicaService crearEntidadService,
        IActualizarEntidadAcademicaService actualizarEntidadService,
        IEliminarEntidadAcademicaService eliminarEntidadService)
    {
        _consultarEntidadesService = consultarEntidadesService;
        _consultarEntidadService = consultarEntidadService;
        _crearEntidadService = crearEntidadService;
        _actualizarEntidadService = actualizarEntidadService;
        _eliminarEntidadService = eliminarEntidadService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(EntidadesAcademicasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerTodosAsync(
        [FromQuery] string? busqueda,
        [FromQuery] string? region,
        [FromQuery] int? idAreaAcademica,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarEntidadesService.ConsultarAsync(
            new ConsultarEntidadesAcademicasQuery(
                busqueda,
                region,
                idAreaAcademica,
                pagina,
                cantidad),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoEntidadAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoEntidadAcademica.Validacion => CrearRespuestaValidacion(resultado),
            _ => Problem("El resultado de la consulta de entidades académicas no es válido.")
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EntidadAcademicaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarEntidadService.ConsultarAsync(
            new ConsultarEntidadAcademicaQuery(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoEntidadAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoEntidadAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoEntidadAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la consulta de la entidad académica no es válido.")
        };
    }

    [HttpPost]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(EntidadAcademicaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody] CrearEntidadAcademicaRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _crearEntidadService.CrearAsync(
            new CrearEntidadAcademicaCommand(
                request.Clave,
                request.Nombre,
                request.CalleNumero,
                request.Colonia,
                request.Cp,
                request.Municipio,
                request.Telefono,
                request.Extension,
                request.IdAreaAcademica,
                request.Region),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoEntidadAcademica.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoEntidadAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoEntidadAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoEntidadAcademica.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la creación de la entidad académica no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(EntidadAcademicaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromBody] ActualizarEntidadAcademicaRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _actualizarEntidadService.ActualizarAsync(
            new ActualizarEntidadAcademicaCommand(
                id,
                request.Clave,
                request.Nombre,
                request.CalleNumero,
                request.Colonia,
                request.Cp,
                request.Municipio,
                request.Telefono,
                request.Extension,
                request.IdAreaAcademica,
                request.Region),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoEntidadAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoEntidadAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoEntidadAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoEntidadAcademica.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la actualización de la entidad académica no es válido.")
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
        var resultado = await _eliminarEntidadService.EliminarAsync(
            new EliminarEntidadAcademicaCommand(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoEntidadAcademica.Exito => NoContent(),
            TipoResultadoEntidadAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoEntidadAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la eliminación de la entidad académica no es válido.")
        };
    }

    private static ActionResult CrearRespuestaValidacion<T>(
        EntidadAcademicaResultado<T> resultado)
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
        EntidadAcademicaResultado<T> resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
