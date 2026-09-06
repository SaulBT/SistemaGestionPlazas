using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Ports;

namespace SGPla.Modules.DireccionesAreaAcademica.Api;

[ApiController]
[Route("api/v1/areas-academicas")]
[Authorize]
public sealed class AreaAcademicaController : ControllerBase
{
    private readonly IConsultarAreasAcademicasService _consultarAreasService;
    private readonly IConsultarAreaAcademicaService _consultarAreaService;
    private readonly ICrearAreaAcademicaService _crearAreaService;
    private readonly IActualizarAreaAcademicaService _actualizarAreaService;
    private readonly IEliminarAreaAcademicaService _eliminarAreaService;

    public AreaAcademicaController(
        IConsultarAreasAcademicasService consultarAreasService,
        IConsultarAreaAcademicaService consultarAreaService,
        ICrearAreaAcademicaService crearAreaService,
        IActualizarAreaAcademicaService actualizarAreaService,
        IEliminarAreaAcademicaService eliminarAreaService)
    {
        _consultarAreasService = consultarAreasService;
        _consultarAreaService = consultarAreaService;
        _crearAreaService = crearAreaService;
        _actualizarAreaService = actualizarAreaService;
        _eliminarAreaService = eliminarAreaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AreasAcademicasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerTodosAsync(
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarAreasService.ConsultarAsync(
            new ConsultarAreasAcademicasQuery(busqueda, pagina, cantidad),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoAreaAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoAreaAcademica.Validacion => CrearRespuestaValidacion(resultado),
            _ => Problem("El resultado de la consulta de áreas académicas no es válido.")
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AreaAcademicaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _consultarAreaService.ConsultarAsync(
            new ConsultarAreaAcademicaQuery(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoAreaAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoAreaAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoAreaAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la consulta del área académica no es válido.")
        };
    }

    [HttpPost]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(AreaAcademicaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearAsync(
        [FromBody] CrearAreaAcademicaRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _crearAreaService.CrearAsync(
            new CrearAreaAcademicaCommand(
                request.Nombre,
                request.Telefono,
                request.Extension),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoAreaAcademica.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoAreaAcademica.Validacion => CrearRespuestaValidacion(resultado),
            _ => Problem("El resultado de la creación del área académica no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(AreaAcademicaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromBody] ActualizarAreaAcademicaRequest request,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _actualizarAreaService.ActualizarAsync(
            new ActualizarAreaAcademicaCommand(
                id,
                request.Nombre,
                request.Telefono,
                request.Extension),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoAreaAcademica.Exito => Ok(resultado.Respuesta),
            TipoResultadoAreaAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoAreaAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la actualización del área académica no es válido.")
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
        var resultado = await _eliminarAreaService.EliminarAsync(
            new EliminarAreaAcademicaCommand(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoAreaAcademica.Exito => NoContent(),
            TipoResultadoAreaAcademica.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoAreaAcademica.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la eliminación del área académica no es válido.")
        };
    }

    private static ActionResult CrearRespuestaValidacion<T>(
        AreaAcademicaResultado<T> resultado)
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
        AreaAcademicaResultado<T> resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
