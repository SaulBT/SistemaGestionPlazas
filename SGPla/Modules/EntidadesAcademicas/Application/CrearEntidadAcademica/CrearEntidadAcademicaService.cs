using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Models;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;
using SGPla.Modules.EntidadesAcademicas.Domain;

namespace SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica;

public sealed class CrearEntidadAcademicaService : ICrearEntidadAcademicaService
{
    private readonly IEntidadAcademicaRepository _repository;

    public CrearEntidadAcademicaService(IEntidadAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> CrearAsync(
        CrearEntidadAcademicaCommand command,
        CancellationToken cancellationToken)
    {
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
                null,
                cancellationToken))
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.Conflicto,
                "La clave ya está en uso.",
                "clave");
        }

        var registro = await _repository.CrearAsync(
            new EntidadAcademicaParaCrear(
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

        return EntidadAcademicaResultado<EntidadAcademicaResponse>.Exito(Mapear(registro));
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
