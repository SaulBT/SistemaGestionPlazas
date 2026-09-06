using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Domain;

namespace SGPla.Modules.SolicitudesApertura.Api;

[ApiController]
[Route("api/v1/solicitudes-apertura")]
[Authorize(Roles = Constantes.COORDINADOR_EA)]
public sealed class SolicitudAperturaController : ControllerBase
{
    private readonly ICrearSolicitudAperturaService _service;

    public SolicitudAperturaController(ICrearSolicitudAperturaService service)
    {
        _service = service;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(SolicitudAperturaConstantes.TAMANIO_MAXIMO_SOLICITUD_HTTP)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = SolicitudAperturaConstantes.TAMANIO_MAXIMO_SOLICITUD_HTTP)]
    [ProducesResponseType(typeof(CrearSolicitudAperturaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CrearAsync(
        [FromForm] CrearSolicitudAperturaRequest request,
        CancellationToken cancellationToken)
    {
        var archivo = request.ArchivoOficio;
        var contenido = Array.Empty<byte>();

        if (archivo is not null
            && archivo.Length > 0
            && archivo.Length <= SolicitudAperturaConstantes.TAMANIO_MAXIMO_ARCHIVO)
        {
            await using var stream = archivo.OpenReadStream();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria, cancellationToken);
            contenido = memoria.ToArray();
        }

        var command = new CrearSolicitudAperturaCommand(
            request.IdExperienciaEducativa,
            request.Seccion,
            request.IdModalidad,
            request.CantidadSolicitantes,
            request.Justificacion,
            archivo?.FileName ?? string.Empty,
            archivo?.Length ?? 0,
            contenido);

        var resultado = await _service.CrearAsync(command, cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoCrearSolicitudApertura.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoCrearSolicitudApertura.Validacion =>
                CrearRespuestaValidacion(resultado),
            TipoResultadoCrearSolicitudApertura.NoEncontrado =>
                NotFound(CrearProblema(resultado)),
            TipoResultadoCrearSolicitudApertura.Prohibido =>
                Forbid(),
            TipoResultadoCrearSolicitudApertura.Conflicto =>
                Conflict(CrearProblema(resultado)),
            TipoResultadoCrearSolicitudApertura.ReglaNegocio =>
                UnprocessableEntity(CrearProblema(resultado)),
            _ => Problem("El resultado de la solicitud de apertura no es válido.")
        };
    }

    private ActionResult CrearRespuestaValidacion(
        CrearSolicitudAperturaResultado resultado)
    {
        var campo = resultado.Campo ?? string.Empty;
        var mensaje = resultado.Mensaje ?? "La información proporcionada no es válida.";
        var errores = new Dictionary<string, string[]>
        {
            [campo] = new[] { mensaje }
        };

        return ValidationProblem(new ValidationProblemDetails(errores));
    }

    private static ProblemDetails CrearProblema(
        CrearSolicitudAperturaResultado resultado)
    {
        return new ProblemDetails
        {
            Detail = resultado.Mensaje
        };
    }
}
