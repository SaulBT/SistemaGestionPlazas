using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGPla.Commons;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Domain;

namespace SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura;

public sealed class CrearSolicitudAperturaService : ICrearSolicitudAperturaService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFechaActualProvider _fechaActualProvider;
    private readonly ISolicitudAperturaRepository _repository;
    private readonly IArchivoOficioStorage _archivoOficioStorage;
    private readonly ILogger<CrearSolicitudAperturaService> _logger;

    public CrearSolicitudAperturaService(
        ICurrentUserContext currentUserContext,
        IFechaActualProvider fechaActualProvider,
        ISolicitudAperturaRepository repository,
        IArchivoOficioStorage archivoOficioStorage,
        ILogger<CrearSolicitudAperturaService> logger)
    {
        _currentUserContext = currentUserContext;
        _fechaActualProvider = fechaActualProvider;
        _repository = repository;
        _archivoOficioStorage = archivoOficioStorage;
        _logger = logger;
    }

    public async Task<CrearSolicitudAperturaResultado> CrearAsync(
        CrearSolicitudAperturaCommand command,
        CancellationToken cancellationToken)
    {
        var validacion = ValidarEntrada(command);

        if (validacion is not null)
        {
            return validacion;
        }

        if (!_currentUserContext.EstaAutenticado || !_currentUserContext.EsCoordinadorEa)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Prohibido,
                "Solo un Coordinador de Entidad Académica puede crear solicitudes de apertura.");
        }

        if (string.IsNullOrWhiteSpace(_currentUserContext.Correo))
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Prohibido,
                "El usuario autenticado no tiene un correo asociado.");
        }

        var coordinador = await _repository.ObtenerCoordinadorEaAsync(
            _currentUserContext.Correo,
            cancellationToken);

        if (coordinador is null)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Prohibido,
                "El usuario autenticado no está asociado a una Entidad Académica.");
        }

        var experienciaEducativa = await _repository.ObtenerExperienciaEducativaAsync(
            command.IdExperienciaEducativa,
            cancellationToken);

        if (experienciaEducativa is null)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.NoEncontrado,
                "La Experiencia Educativa indicada no existe.",
                "IdExperienciaEducativa");
        }

        if (experienciaEducativa.IdEntidadAcademica != coordinador.IdEntidadAcademica)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Prohibido,
                "La Experiencia Educativa no pertenece a la Entidad Académica del usuario.");
        }

        var limitesResultado = ValidarLimitesSolicitantes(experienciaEducativa, command);

        if (limitesResultado is not null)
        {
            return limitesResultado;
        }

        var modalidad = await _repository.ObtenerModalidadAsync(
            command.IdModalidad,
            cancellationToken);

        if (modalidad is null)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.NoEncontrado,
                "La Modalidad indicada no existe o no está activa.",
                "IdModalidad");
        }

        var periodo = await _repository.ObtenerPeriodoSiguienteAsync(
            _fechaActualProvider.ObtenerFechaActual(),
            cancellationToken);

        if (periodo is null)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.ReglaNegocio,
                "No fue posible determinar el periodo escolar inmediato siguiente.");
        }

        var seccion = command.Seccion?.Trim() ?? string.Empty;
        var contextoDuplicidad = new SolicitudAperturaDuplicidadContexto(
            experienciaEducativa.IdExperienciaEducativa,
            seccion,
            periodo.IdPeriodo,
            coordinador.IdEntidadAcademica,
            experienciaEducativa.IdProgramaEducativo,
            experienciaEducativa.IdPlanEstudios);

        if (await _repository.ExisteSolicitudActivaAsync(contextoDuplicidad, cancellationToken))
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Conflicto,
                "Ya existe una solicitud de apertura activa para la misma Experiencia Educativa, sección, periodo y contexto académico.");
        }

        var archivoParaGuardar = new ArchivoOficioParaGuardar(
            command.ContenidoArchivo,
            Path.GetFileName(command.NombreOriginalArchivo.Trim()),
            SolicitudAperturaConstantes.TIPO_ARCHIVO_OFICIO);

        ArchivoOficioGuardado? archivoGuardado = null;

        try
        {
            archivoGuardado = await _archivoOficioStorage.GuardarAsync(
                archivoParaGuardar,
                cancellationToken);

            var solicitud = new SolicitudAperturaParaCrear(
                experienciaEducativa.IdExperienciaEducativa,
                periodo.IdPeriodo,
                coordinador.IdEntidadAcademica,
                experienciaEducativa.IdProgramaEducativo,
                experienciaEducativa.IdPlanEstudios,
                modalidad.IdModalidad,
                seccion,
                command.CantidadSolicitantes,
                NormalizarJustificacion(command.Justificacion),
                SolicitudAperturaConstantes.ESTADO_PENDIENTE);

            await _repository.CrearAsync(solicitud, archivoGuardado, cancellationToken);

            return CrearSolicitudAperturaResultado.Exito(
                new CrearSolicitudAperturaResponse(
                    experienciaEducativa.Nombre,
                    seccion,
                    modalidad.Nombre));
        }
        catch (DbUpdateException exception) when (EsViolacionDuplicidad(exception))
        {
            await EliminarArchivoGuardadoAsync(archivoGuardado, cancellationToken);

            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Conflicto,
                "Ya existe una solicitud de apertura activa para la misma Experiencia Educativa, sección, periodo y contexto académico.");
        }
        catch
        {
            await EliminarArchivoGuardadoAsync(archivoGuardado, cancellationToken);
            throw;
        }
    }

    private static CrearSolicitudAperturaResultado? ValidarEntrada(
        CrearSolicitudAperturaCommand command)
    {
        if (command.IdExperienciaEducativa <= 0)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "La Experiencia Educativa es obligatoria.",
                "IdExperienciaEducativa");
        }

        if (string.IsNullOrWhiteSpace(command.Seccion))
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "La sección es obligatoria.",
                "Seccion");
        }

        if (command.Seccion.Trim().Length > 50)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "La sección no puede exceder los 50 caracteres.",
                "Seccion");
        }

        if (command.IdModalidad <= 0)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "La Modalidad es obligatoria.",
                "IdModalidad");
        }

        if (command.CantidadSolicitantes <= 0)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "La cantidad de solicitantes debe ser mayor que cero.",
                "CantidadSolicitantes");
        }

        if (command.TamanioArchivo <= 0 || command.ContenidoArchivo.Length == 0)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "El archivo de oficio es obligatorio.",
                "ArchivoOficio");
        }

        if (command.TamanioArchivo > SolicitudAperturaConstantes.TAMANIO_MAXIMO_ARCHIVO)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "El archivo de oficio no puede exceder los 5 MB.",
                "ArchivoOficio");
        }

        if (string.IsNullOrWhiteSpace(command.NombreOriginalArchivo)
            || !string.Equals(
                Path.GetExtension(command.NombreOriginalArchivo),
                SolicitudAperturaConstantes.EXTENSION_ARCHIVO_OFICIO,
                StringComparison.OrdinalIgnoreCase))
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "El archivo de oficio debe tener extensión PDF.",
                "ArchivoOficio");
        }

        if (!EsArchivoPdf(command.ContenidoArchivo))
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                "El contenido del archivo de oficio no es un PDF válido.",
                "ArchivoOficio");
        }

        return null;
    }

    private static CrearSolicitudAperturaResultado? ValidarLimitesSolicitantes(
        ExperienciaEducativaContexto experienciaEducativa,
        CrearSolicitudAperturaCommand command)
    {
        if (!experienciaEducativa.CantidadMinimaSolicitantes.HasValue
            || !experienciaEducativa.CantidadMaximaSolicitantes.HasValue
            || experienciaEducativa.CantidadMinimaSolicitantes <= 0
            || experienciaEducativa.CantidadMaximaSolicitantes
                < experienciaEducativa.CantidadMinimaSolicitantes)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.ReglaNegocio,
                "La Experiencia Educativa no tiene configurados correctamente sus límites de solicitantes.");
        }

        if (command.CantidadSolicitantes < experienciaEducativa.CantidadMinimaSolicitantes)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                $"La cantidad de solicitantes no puede ser menor que {experienciaEducativa.CantidadMinimaSolicitantes}.",
                "CantidadSolicitantes");
        }

        if (command.CantidadSolicitantes > experienciaEducativa.CantidadMaximaSolicitantes)
        {
            return CrearSolicitudAperturaResultado.Error(
                TipoResultadoCrearSolicitudApertura.Validacion,
                $"La cantidad de solicitantes no puede ser mayor que {experienciaEducativa.CantidadMaximaSolicitantes}.",
                "CantidadSolicitantes");
        }

        return null;
    }

    private static bool EsArchivoPdf(byte[] contenido)
    {
        const int LONGITUD_CABECERA_PDF = 5;

        if (contenido.Length < LONGITUD_CABECERA_PDF)
        {
            return false;
        }

        return Encoding.ASCII.GetString(contenido, 0, LONGITUD_CABECERA_PDF) == "%PDF-";
    }

    private static string? NormalizarJustificacion(string? justificacion)
    {
        return string.IsNullOrWhiteSpace(justificacion)
            ? null
            : justificacion.Trim();
    }

    private static bool EsViolacionDuplicidad(DbUpdateException exception)
    {
        var sqlException = exception.GetBaseException() as SqlException;
        return sqlException?.Number is 2601 or 2627;
    }

    private async Task EliminarArchivoGuardadoAsync(
        ArchivoOficioGuardado? archivo,
        CancellationToken cancellationToken)
    {
        if (archivo is null)
        {
            return;
        }

        try
        {
            await _archivoOficioStorage.EliminarAsync(archivo.Ruta, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No fue posible eliminar el archivo temporal de la solicitud de apertura: {Ruta}",
                archivo.Ruta);
        }
    }
}
