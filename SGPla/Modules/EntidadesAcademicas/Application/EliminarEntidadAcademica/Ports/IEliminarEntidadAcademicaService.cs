using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Contracts;

namespace SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Ports;

public interface IEliminarEntidadAcademicaService
{
    Task<EntidadAcademicaResultado<bool>> EliminarAsync(
        EliminarEntidadAcademicaCommand command,
        CancellationToken cancellationToken);
}
