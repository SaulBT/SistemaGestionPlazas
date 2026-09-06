using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Models;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;
using SGPla.Modules.EntidadesAcademicas.Domain;

namespace SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica;

public sealed class ActualizarEntidadAcademicaService : IActualizarEntidadAcademicaService
{
    private readonly IEntidadAcademicaRepository _repository;

    public ActualizarEntidadAcademicaService(IEntidadAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> ActualizarAsync(
        ActualizarEntidadAcademicaCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdEntidadAcademica <= 0)
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.Validacion,
                "El ID de la entidad académica no es válido.",
                "idEntidadAcademica");
        }

        var validacion = EntidadAcademicaReglas.ValidarDatos(
            command.Clave,
            command.Nombre,
            command.CalleNumero,
            command.Colonia,
            command.Cp,
            command.Municipio,
            command.Telefono,
            command.Extension,
            command.IdAreaAcademica,
            command.Region);

        if (validacion is not null)
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var datos = EntidadAcademicaReglas.Normalizar(
            command.Clave!,
            command.Nombre!,
            command.CalleNumero!,
            command.Colonia!,
            command.Cp!,
            command.Municipio!,
            command.Telefono!,
            command.Extension!,
            command.IdAreaAcademica!.Value,
            command.Region!);

        if (!await _repository.ExisteAreaAcademicaActivaAsync(
                datos.IdAreaAcademica,
                cancellationToken))
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.NoEncontrado,
                "El área académica indicada no existe.",
                "idAreaAcademica");
        }

        if (await _repository.ExistePorClaveAsync(
                datos.Clave,
                command.IdEntidadAcademica,
                cancellationToken))
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.Conflicto,
                "La clave ya está en uso.",
                "clave");
        }

        var actualizado = await _repository.ActualizarAsync(
            new EntidadAcademicaParaActualizar(
                command.IdEntidadAcademica,
                datos.Clave,
                datos.Nombre,
                datos.CalleNumero,
                datos.Colonia,
                datos.Cp,
                datos.Municipio,
                datos.Telefono,
                datos.Extension,
                datos.IdAreaAcademica,
                datos.Region),
            cancellationToken);

        if (actualizado is null)
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.NoEncontrado,
                "La entidad académica indicada no existe.");
        }

        return EntidadAcademicaResultado<EntidadAcademicaResponse>.Exito(Mapear(actualizado));
    }

    private static EntidadAcademicaResponse Mapear(
        EntidadAcademicaRegistro registro)
    {
        return new EntidadAcademicaResponse(
            registro.IdEntidadAcademica,
            registro.Clave,
            registro.Nombre,
            registro.CalleNumero,
            registro.Colonia,
            registro.Cp,
            registro.Municipio,
            registro.Telefono,
            registro.Extension,
            registro.IdAreaAcademica,
            registro.NombreAreaAcademica,
            registro.Region);
    }
}
