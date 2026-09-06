using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;

namespace SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Ports;

public interface IConsultarEntidadesAcademicasService
{
    Task<EntidadAcademicaResultado<EntidadesAcademicasResponse>> ConsultarAsync(
        ConsultarEntidadesAcademicasQuery query,
        CancellationToken cancellationToken);
}
