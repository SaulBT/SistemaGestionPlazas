using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Contracts;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Ports;

public interface IEliminarAreaAcademicaService
{
    Task<AreaAcademicaResultado<bool>> EliminarAsync(
        EliminarAreaAcademicaCommand command,
        CancellationToken cancellationToken);
}
