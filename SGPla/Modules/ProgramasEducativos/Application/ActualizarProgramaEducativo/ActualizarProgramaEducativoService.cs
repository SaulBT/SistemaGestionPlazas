using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo;

public sealed class ActualizarProgramaEducativoService : IActualizarProgramaEducativoService
{
    private readonly IProgramaEducativoRepository _repository;

    public ActualizarProgramaEducativoService(IProgramaEducativoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> ActualizarAsync(
        ActualizarProgramaEducativoCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdProgramaEducativo <= 0)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                "El ID del programa educativo no es válido.",
                "idProgramaEducativo");
        }

        var validacion = ProgramaEducativoReglas.ValidarDatos(
            command.Nombre,
            command.Campus,
            command.IdEntidadAcademica);

        if (validacion is not null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var datos = ProgramaEducativoReglas.Normalizar(
            command.Nombre!,
            command.Campus!,
            command.IdEntidadAcademica!.Value);

        if (!await _repository.ExisteEntidadAcademicaActivaAsync(
                datos.IdEntidadAcademica,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "La entidad académica indicada no existe.",
                "idEntidadAcademica");
        }

        var clave = ProgramaEducativoReglas.ObtenerClave(datos.Nombre);
        if (await _repository.ExistePorClaveAsync(
                clave,
                command.IdProgramaEducativo,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Conflicto,
                "Ya existe un programa educativo con la misma clave.",
                "nombre");
        }

        var actualizado = await _repository.ActualizarAsync(
            new ProgramaEducativoParaActualizar(
                command.IdProgramaEducativo,
                datos.Nombre,
                datos.Campus,
                datos.IdEntidadAcademica),
            cancellationToken);

        if (actualizado is null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "El programa educativo indicado no existe.");
        }

        return ProgramaEducativoResultado<ProgramaEducativoResponse>.Exito(Mapear(actualizado));
    }

    private static ProgramaEducativoResponse Mapear(
        ProgramaEducativoRegistro registro)
    {
        return new ProgramaEducativoResponse(
            registro.IdProgramaEducativo,
            registro.Nombre,
            registro.Campus,
            registro.IdEntidadAcademica,
            registro.NombreEntidadAcademica,
            registro.IdAreaAcademica,
            registro.NombreAreaAcademica,
            registro.Region);
    }
}
