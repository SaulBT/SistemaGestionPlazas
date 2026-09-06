using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Contracts;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Ports;

public interface ICrearAreaAcademicaService
{
    Task<AreaAcademicaResultado<AreaAcademicaResponse>> CrearAsync(
        CrearAreaAcademicaCommand command,
        CancellationToken cancellationToken);
}
