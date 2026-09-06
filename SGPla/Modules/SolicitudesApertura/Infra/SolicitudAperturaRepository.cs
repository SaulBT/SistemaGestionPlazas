using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Domain;

namespace SGPla.Modules.SolicitudesApertura.Infra;

public sealed class SolicitudAperturaRepository : ISolicitudAperturaRepository
{
    private readonly GestionDePlazasDbContext _context;

    public SolicitudAperturaRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<CoordinadorEaContexto?> ObtenerCoordinadorEaAsync(
        string correo,
        CancellationToken cancellationToken)
    {
        return await _context.CoordinadorEa
            .AsNoTracking()
            .Where(coordinador => coordinador.Correo == correo)
            .Select(coordinador => new CoordinadorEaContexto(
                coordinador.IdEntidadAcademica))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExperienciaEducativaContexto?> ObtenerExperienciaEducativaAsync(
        int idExperienciaEducativa,
        CancellationToken cancellationToken)
    {
        return await _context.ExperienciaEducativa
            .AsNoTracking()
            .Where(experiencia => experiencia.IdExperienciaEducativa == idExperienciaEducativa)
            .Select(experiencia => new ExperienciaEducativaContexto(
                experiencia.IdExperienciaEducativa,
                experiencia.Nombre,
                experiencia.IdPlanEstudios,
                experiencia.IdPlanEstudiosNavigation.IdProgramaEducativo,
                experiencia.IdPlanEstudiosNavigation
                    .IdProgramaEducativoNavigation.IdEntidadAcademica,
                experiencia.CantidadMinimaSolicitantes,
                experiencia.CantidadMaximaSolicitantes))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ModalidadContexto?> ObtenerModalidadAsync(
        int idModalidad,
        CancellationToken cancellationToken)
    {
        return await _context.Modalidad
            .AsNoTracking()
            .Where(modalidad => modalidad.IdModalidad == idModalidad
                && modalidad.Activa)
            .Select(modalidad => new ModalidadContexto(
                modalidad.IdModalidad,
                modalidad.Nombre))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PeriodoContexto?> ObtenerPeriodoSiguienteAsync(
        DateOnly fechaActual,
        CancellationToken cancellationToken)
    {
        var fechaFinPeriodoActual = await _context.Periodo
            .AsNoTracking()
            .Where(periodo => periodo.FechaInicio.HasValue
                && periodo.FechaFin.HasValue
                && periodo.FechaInicio.Value <= fechaActual
                && periodo.FechaFin.Value >= fechaActual)
            .OrderByDescending(periodo => periodo.FechaInicio)
            .Select(periodo => periodo.FechaFin)
            .FirstOrDefaultAsync(cancellationToken);

        if (!fechaFinPeriodoActual.HasValue)
        {
            return null;
        }

        return await _context.Periodo
            .AsNoTracking()
            .Where(periodo => periodo.FechaInicio.HasValue
                && periodo.FechaFin.HasValue
                && periodo.FechaInicio.Value > fechaFinPeriodoActual.Value)
            .OrderBy(periodo => periodo.FechaInicio)
            .Select(periodo => new PeriodoContexto(
                periodo.IdPeriodo,
                periodo.Codigo,
                periodo.FechaInicio.GetValueOrDefault(),
                periodo.FechaFin.GetValueOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExisteSolicitudActivaAsync(
        SolicitudAperturaDuplicidadContexto contexto,
        CancellationToken cancellationToken)
    {
        return await _context.SolicitudApertura
            .AsNoTracking()
            .AnyAsync(solicitud =>
                solicitud.IdExperienciaEducativa == contexto.IdExperienciaEducativa
                && solicitud.Seccion == contexto.Seccion
                && solicitud.IdPeriodo == contexto.IdPeriodo
                && solicitud.IdEntidadAcademica == contexto.IdEntidadAcademica
                && solicitud.IdProgramaEducativo == contexto.IdProgramaEducativo
                && solicitud.IdPlanEstudios == contexto.IdPlanEstudios
                && (solicitud.Estado == SolicitudAperturaConstantes.ESTADO_PENDIENTE
                    || solicitud.Estado == SolicitudAperturaConstantes.ESTADO_ACEPTADA),
                cancellationToken);
    }

    public async Task CrearAsync(
        SolicitudAperturaParaCrear solicitud,
        ArchivoOficioGuardado archivo,
        CancellationToken cancellationToken)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(
                cancellationToken);

            var archivoEntidad = new Archivo
            {
                Nombre = archivo.NombreOriginal,
                Ruta = archivo.Ruta,
                Tipo = archivo.Tipo,
                Tamanio = archivo.Tamanio
            };

            _context.Archivo.Add(archivoEntidad);
            await _context.SaveChangesAsync(cancellationToken);

            var solicitudEntidad = new SolicitudApertura
            {
                IdExperienciaEducativa = solicitud.IdExperienciaEducativa,
                IdPeriodo = solicitud.IdPeriodo,
                IdEntidadAcademica = solicitud.IdEntidadAcademica,
                IdProgramaEducativo = solicitud.IdProgramaEducativo,
                IdPlanEstudios = solicitud.IdPlanEstudios,
                IdModalidad = solicitud.IdModalidad,
                Seccion = solicitud.Seccion,
                CantidadSolicitantes = solicitud.CantidadSolicitantes,
                Justificacion = solicitud.Justificacion,
                IdArchivoOficio = archivoEntidad.IdArchivo,
                Estado = solicitud.Estado
            };

            _context.SolicitudApertura.Add(solicitudEntidad);
            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        });
    }
}
