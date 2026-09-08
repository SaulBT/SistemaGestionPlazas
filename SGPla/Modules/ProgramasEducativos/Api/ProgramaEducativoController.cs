using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
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
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Api;

[ApiController]
[Route("api/v1/programas-educativos")]
[Authorize]
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
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PlanEstudiosConstantes.TamanioMaximoSolicitudHttp)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = PlanEstudiosConstantes.TamanioMaximoSolicitudHttp)]
    [ProducesResponseType(typeof(ProgramaEducativoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearAsync(
        [FromForm] CrearProgramaEducativoRequest request,
        CancellationToken cancellationToken = default)
    {
        var planesEstudio = await MapearPlanesEstudioAsync(
            request.PlanesEstudio,
            cancellationToken);
        var resultado = await _crearProgramaService.CrearAsync(
            new CrearProgramaEducativoCommand(
                request.Nombre,
                request.Campus,
                request.IdEntidadAcademica,
                planesEstudio),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito =>
                StatusCode(StatusCodes.Status201Created, resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoProgramaEducativo.Conflicto => CrearRespuestaConflicto(resultado),
            _ => Problem("El resultado de la creación del programa educativo no es válido.")
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Constantes.SUPERUSUARIO)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PlanEstudiosConstantes.TamanioMaximoSolicitudHttp)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = PlanEstudiosConstantes.TamanioMaximoSolicitudHttp)]
    [ProducesResponseType(typeof(ProgramaEducativoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarAsync(
        int id,
        [FromForm] ActualizarProgramaEducativoRequest request,
        CancellationToken cancellationToken = default)
    {
        var planesEstudio = await MapearPlanesEstudioAsync(
            request.PlanesEstudio,
            cancellationToken);
        var resultado = await _actualizarProgramaService.ActualizarAsync(
            new ActualizarProgramaEducativoCommand(
                id,
                request.Nombre,
                request.Campus,
                request.IdEntidadAcademica,
                planesEstudio),
            cancellationToken);

        return resultado.Tipo switch
        {
            TipoResultadoProgramaEducativo.Exito => Ok(resultado.Respuesta),
            TipoResultadoProgramaEducativo.Validacion => CrearRespuestaValidacion(resultado),
            TipoResultadoProgramaEducativo.NoEncontrado => NotFound(CrearProblema(resultado)),
            TipoResultadoProgramaEducativo.Conflicto => CrearRespuestaConflicto(resultado),
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

    private static ActionResult CrearRespuestaConflicto<T>(
        ProgramaEducativoResultado<T> resultado)
    {
        if (string.IsNullOrWhiteSpace(resultado.Campo))
        {
            return new ConflictObjectResult(CrearProblema(resultado));
        }

        var errores = new Dictionary<string, string[]>
        {
            [resultado.Campo] =
                [resultado.Mensaje ?? "La información proporcionada no es válida."]
        };

        return new ObjectResult(new ValidationProblemDetails(errores))
        {
            StatusCode = StatusCodes.Status409Conflict
        };
    }

    private static async Task<IReadOnlyList<PlanEstudioParaGuardar>> MapearPlanesEstudioAsync(
        IEnumerable<PlanEstudioRequest> planesEstudio,
        CancellationToken cancellationToken)
    {
        var planes = new List<PlanEstudioParaGuardar>();

        foreach (var plan in planesEstudio)
        {
            ArchivoPlanContenido? archivo = null;
            if (plan.Archivo is not null)
            {
                var contenido = Array.Empty<byte>();
                if (plan.Archivo.Length <= PlanEstudiosConstantes.TamanioMaximoArchivo)
                {
                    await using var stream = plan.Archivo.OpenReadStream();
                    using var memoria = new MemoryStream();
                    await stream.CopyToAsync(memoria, cancellationToken);
                    contenido = memoria.ToArray();
                }

                archivo = new ArchivoPlanContenido(
                    plan.Archivo.FileName,
                    plan.Archivo.ContentType,
                    plan.Archivo.Length,
                    contenido);
            }

            planes.Add(new PlanEstudioParaGuardar(
                plan.IdPlanEstudios,
                plan.Nombre,
                plan.Modalidad,
                archivo));
        }

        return planes;
    }
}
