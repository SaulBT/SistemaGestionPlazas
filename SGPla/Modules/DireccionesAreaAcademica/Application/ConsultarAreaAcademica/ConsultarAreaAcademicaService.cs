using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Domain;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica;

public sealed class ConsultarAreaAcademicaService : IConsultarAreaAcademicaService
{
    private readonly IAreaAcademicaRepository _repository;

    public ConsultarAreaAcademicaService(IAreaAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaAcademicaResultado<AreaAcademicaResponse>> ConsultarAsync(
        ConsultarAreaAcademicaQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IdAreaAcademica <= 0)
        {
            return AreaAcademicaResultado<AreaAcademicaResponse>.Error(
                TipoResultadoAreaAcademica.Validacion,
                "El ID del área académica no es válido.",
                "idAreaAcademica");
        }

        var registro = await _repository.ObtenerPorIdAsync(
            query.IdAreaAcademica,
            cancellationToken);

        if (registro is null)
        {
            return AreaAcademicaResultado<AreaAcademicaResponse>.Error(
                TipoResultadoAreaAcademica.NoEncontrado,
                "El área académica indicada no existe.");
        }

        return AreaAcademicaResultado<AreaAcademicaResponse>.Exito(
            new AreaAcademicaResponse(
                registro.IdAreaAcademica,
                registro.Nombre,
                registro.Telefono,
                registro.Extension));
    }
}
