using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;

namespace SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica;

public sealed class ConsultarEntidadAcademicaService : IConsultarEntidadAcademicaService
{
    private readonly IEntidadAcademicaRepository _repository;

    public ConsultarEntidadAcademicaService(IEntidadAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntidadAcademicaResultado<EntidadAcademicaResponse>> ConsultarAsync(
        ConsultarEntidadAcademicaQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IdEntidadAcademica <= 0)
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.Validacion,
                "El ID de la entidad académica no es válido.",
                "idEntidadAcademica");
        }

        var registro = await _repository.ObtenerPorIdAsync(
            query.IdEntidadAcademica,
            cancellationToken);

        if (registro is null)
        {
            return EntidadAcademicaResultado<EntidadAcademicaResponse>.Error(
                TipoResultadoEntidadAcademica.NoEncontrado,
                "La entidad académica indicada no existe.");
        }

        return EntidadAcademicaResultado<EntidadAcademicaResponse>.Exito(Mapear(registro));
    }

    private static EntidadAcademicaResponse Mapear(
        Application.Models.EntidadAcademicaRegistro registro)
    {
        return new EntidadAcademicaResponse(
            registro.IdEntidadAcademica,
            registro.Clave,
            registro.Nombre,
            registro.CalleNumero,
            registro.Colonia,
            registro.Cp,
            registro.Municipio,
            registro.Telefono,
            registro.Extension,
            registro.IdAreaAcademica,
            registro.NombreAreaAcademica,
            registro.Region);
    }
}
