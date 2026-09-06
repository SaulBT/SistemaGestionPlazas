using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Contracts;

namespace SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Ports;

public interface ICrearEntidadAcademicaService
{
    Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> CrearAsync(
        CrearEntidadAcademicaCommand command,
        CancellationToken cancellationToken);
}
