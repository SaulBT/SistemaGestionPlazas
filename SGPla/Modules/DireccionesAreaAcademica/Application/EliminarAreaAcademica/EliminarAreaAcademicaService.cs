using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica;

public sealed class EliminarAreaAcademicaService : IEliminarAreaAcademicaService
{
    private readonly IAreaAcademicaRepository _repository;

    public EliminarAreaAcademicaService(IAreaAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaAcademicaResultado<bool>> EliminarAsync(
        EliminarAreaAcademicaCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdAreaAcademica <= 0)
        {
            return AreaAcademicaResultado<bool>.Error(
                TipoResultadoAreaAcademica.Validacion,
                "El ID del área académica no es válido.",
                "idAreaAcademica");
        }

        var eliminado = await _repository.EliminarAsync(
            command.IdAreaAcademica,
            DateTime.UtcNow,
            cancellationToken);

        if (!eliminado)
        {
            return AreaAcademicaResultado<bool>.Error(
                TipoResultadoAreaAcademica.NoEncontrado,
                "El área académica indicada no existe.");
        }

        return AreaAcademicaResultado<bool>.Exito(true);
    }
}
