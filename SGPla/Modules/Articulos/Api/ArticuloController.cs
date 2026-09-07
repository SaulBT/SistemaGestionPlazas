using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.Articulos.Application.ActualizarArticulo;
using SGPla.Modules.Articulos.Application.ActualizarArticulo.Contracts;
using SGPla.Modules.Articulos.Application.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulo;
using SGPla.Modules.Articulos.Application.ConsultarArticulo.Contracts;
using SGPla.Modules.Articulos.Application.ConsultarArticulos;
using SGPla.Modules.Articulos.Application.ConsultarArticulos.Contracts;
using SGPla.Modules.Articulos.Application.CrearArticulo;
using SGPla.Modules.Articulos.Application.CrearArticulo.Contracts;
using SGPla.Modules.Articulos.Application.EliminarArticulo;
using SGPla.Modules.Articulos.Application.EliminarArticulo.Contracts;

namespace SGPla.Modules.Articulos.Api;

[ApiController]
[Route("api/v1/articulos")]
[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public sealed class ArticuloController : ControllerBase
{
    private readonly IConsultarArticulosService _consultarArticulosService;
    private readonly IConsultarArticuloService _consultarArticuloService;
    private readonly ICrearArticuloService _crearArticuloService;
    private readonly IActualizarArticuloService _actualizarArticuloService;
    private readonly IEliminarArticuloService _eliminarArticuloService;

    public ArticuloController(
        IConsultarArticulosService consultarArticulosService,
        IConsultarArticuloService consultarArticuloService,
        ICrearArticuloService crearArticuloService,
        IActualizarArticuloService actualizarArticuloService,
        IEliminarArticuloService eliminarArticuloService)
    {
        _consultarArticulosService = consultarArticulosService;
        _consultarArticuloService = consultarArticuloService;
        _crearArticuloService = crearArticuloService;
        _actualizarArticuloService = actualizarArticuloService;
        _eliminarArticuloService = eliminarArticuloService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ArticuloResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArticuloResponse>>> ObtenerTodosAsync(
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken)
    {
        var respuesta = await _consultarArticulosService.ConsultarAsync(
            new ConsultarArticulosQuery(busqueda),
            cancellationToken);

        return Ok(respuesta);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ArticuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var resultado = await _consultarArticuloService.ConsultarAsync(
            new ConsultarArticuloQuery(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoArticulo.Exito => Ok(resultado.Respuesta),
            TipoResultadoArticulo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoArticulo.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la consulta del artículo no es válido.")
        };
    }

    [HttpPost]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(ArticuloResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromBody] CrearArticuloRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _crearArticuloService.CrearAsync(
            new CrearArticuloCommand(request.Numero, request.Descripcion),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoArticulo.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoArticulo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoArticulo.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la creación del artículo no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(typeof(ArticuloResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromBody] ActualizarArticuloRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _actualizarArticuloService.ActualizarAsync(
            new ActualizarArticuloCommand(id, request.Numero, request.Descripcion),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoArticulo.Exito => Ok(resultado.Respuesta),
            TipoResultadoArticulo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoArticulo.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoArticulo.Conflicto => Conflict(CrearProblema(resultado)),
            _ => Problem("El resultado de la actualización del artículo no es válido.")
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var resultado = await _eliminarArticuloService.EliminarAsync(
            new EliminarArticuloCommand(id),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoArticulo.Exito => NoContent(),
            TipoResultadoArticulo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoArticulo.NoEncontrado => NotFound(CrearProblema(resultado)),
            _ => Problem("El resultado de la eliminación del artículo no es válido.")
        };
    }

    private static ActionResult CrearRespuestaValidacion<T>(ArticuloResultado<T> resultado)
    {
        var campo = resultado.Campo ?? string.Empty;
        var mensaje = resultado.Mensaje ?? "La información proporcionada no es válida.";
        var errores = new Dictionary<string, string[]>
        {
            [campo] = new[] { mensaje }
        };

        return new BadRequestObjectResult(new ValidationProblemDetails(errores));
    }

    private static ProblemDetails CrearProblema<T>(ArticuloResultado<T> resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
