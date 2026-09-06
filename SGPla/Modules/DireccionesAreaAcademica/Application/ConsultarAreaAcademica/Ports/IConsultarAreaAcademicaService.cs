using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Ports;

public interface IConsultarAreaAcademicaService
{
    Task<AreaAcademicaResultado<AreaAcademicaResponse>> ConsultarAsync(
        ConsultarAreaAcademicaQuery query,
        CancellationToken cancellationToken);
}
