using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;

namespace SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica;

public sealed class EliminarEntidadAcademicaService : IEliminarEntidadAcademicaService
{
    private readonly IEntidadAcademicaRepository _repository;

    public EliminarEntidadAcademicaService(IEntidadAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntidadAcademicaResultado<bool>> EliminarAsync(
        EliminarEntidadAcademicaCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdEntidadAcademica <= 0)
        {
            return EntidadAcademicaResultado<bool>.Error(
                TipoResultadoEntidadAcademica.Validacion,
                "El ID de la entidad académica no es válido.",
                "idEntidadAcademica");
        }

        var eliminado = await _repository.EliminarAsync(
            command.IdEntidadAcademica,
            DateTime.UtcNow,
            cancellationToken);

        if (!eliminado)
        {
            return EntidadAcademicaResultado<bool>.Error(
                TipoResultadoEntidadAcademica.NoEncontrado,
                "La entidad académica indicada no existe.");
        }

        return EntidadAcademicaResultado<bool>.Exito(true);
    }
}
