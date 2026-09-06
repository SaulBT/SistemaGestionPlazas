using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Models;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Domain;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica;

public sealed class ActualizarAreaAcademicaService : IActualizarAreaAcademicaService
{
    private readonly IAreaAcademicaRepository _repository;

    public ActualizarAreaAcademicaService(IAreaAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaAcademicaResultado<AreaAcademicaResponse>> ActualizarAsync(
        ActualizarAreaAcademicaCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdAreaAcademica <= 0)
        {
            return AreaAcademicaResultado<AreaAcademicaResponse>.Error(
                TipoResultadoAreaAcademica.Validacion,
                "El ID del área académica no es válido.",
                "idAreaAcademica");
        }

        var validacion = AreaAcademicaReglas.ValidarDatos(
            command.Nombre,
            command.Telefono,
            command.Extension);

        if (validacion is not null)
        {
            return AreaAcademicaResultado<AreaAcademicaResponse>.Error(
                TipoResultadoAreaAcademica.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var datos = AreaAcademicaReglas.Normalizar(
            command.Nombre!,
            command.Telefono!,
            command.Extension!);

        var actualizado = await _repository.ActualizarAsync(
            new AreaAcademicaParaActualizar(
                command.IdAreaAcademica,
                datos.Nombre,
                datos.Telefono,
                datos.Extension),
            cancellationToken);

        if (actualizado is null)
        {
            return AreaAcademicaResultado<AreaAcademicaResponse>.Error(
                TipoResultadoAreaAcademica.NoEncontrado,
                "El área académica indicada no existe.");
        }

        return AreaAcademicaResultado<AreaAcademicaResponse>.Exito(
            new AreaAcademicaResponse(
                actualizado.IdAreaAcademica,
                actualizado.Nombre,
                actualizado.Telefono,
                actualizado.Extension));
    }
}
