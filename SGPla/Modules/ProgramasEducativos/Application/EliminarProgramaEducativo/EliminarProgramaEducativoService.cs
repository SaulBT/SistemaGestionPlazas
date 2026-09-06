using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Ports;

namespace SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo;

public sealed class EliminarProgramaEducativoService : IEliminarProgramaEducativoService
{
    private readonly IProgramaEducativoRepository _repository;

    public EliminarProgramaEducativoService(IProgramaEducativoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProgramaEducativoResultado<bool>> EliminarAsync(
        EliminarProgramaEducativoCommand command,
        CancellationToken cancellationToken)
    {
        if (command.IdProgramaEducativo <= 0)
        {
            return ProgramaEducativoResultado<bool>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                "El ID del programa educativo no es válido.",
                "idProgramaEducativo");
        }

        var eliminado = await _repository.EliminarAsync(
            command.IdProgramaEducativo,
            DateTime.UtcNow,
            cancellationToken);

        if (!eliminado)
        {
            return ProgramaEducativoResultado<bool>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "El programa educativo indicado no existe.");
        }

        return ProgramaEducativoResultado<bool>.Exito(true);
    }
}
