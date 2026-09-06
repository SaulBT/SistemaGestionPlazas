using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Models;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Domain;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica;

public sealed class CrearAreaAcademicaService : ICrearAreaAcademicaService
{
    private readonly IAreaAcademicaRepository _repository;

    public CrearAreaAcademicaService(IAreaAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaAcademicaResultado<AreaAcademicaResponse>> CrearAsync(
        CrearAreaAcademicaCommand command,
        CancellationToken cancellationToken)
    {
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

        var registro = await _repository.CrearAsync(
            new AreaAcademicaParaCrear(datos.Nombre, datos.Telefono, datos.Extension),
            cancellationToken);

        return AreaAcademicaResultado<AreaAcademicaResponse>.Exito(
            new AreaAcademicaResponse(
                registro.IdAreaAcademica,
                registro.Nombre,
                registro.Telefono,
                registro.Extension));
    }
}
