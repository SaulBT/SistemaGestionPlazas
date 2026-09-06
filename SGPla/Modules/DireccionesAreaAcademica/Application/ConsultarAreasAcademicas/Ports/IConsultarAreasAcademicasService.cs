using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Ports;

public interface IConsultarAreasAcademicasService
{
    Task<AreaAcademicaResultado<AreasAcademicasResponse>> ConsultarAsync(
        ConsultarAreasAcademicasQuery query,
        CancellationToken cancellationToken);
}
