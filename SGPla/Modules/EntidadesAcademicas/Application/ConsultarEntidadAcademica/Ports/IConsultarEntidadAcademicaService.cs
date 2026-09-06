using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;

namespace SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Ports;

public interface IConsultarEntidadAcademicaService
{
    Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> ConsultarAsync(
        ConsultarEntidadAcademicaQuery query,
        CancellationToken cancellationToken);
}
