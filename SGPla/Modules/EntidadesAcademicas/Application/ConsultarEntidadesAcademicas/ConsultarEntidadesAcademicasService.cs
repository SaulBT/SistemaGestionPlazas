using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Contracts;
using SGPla.Modules.EntidadesAcademicas.Application.Models;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;
using SGPla.Modules.EntidadesAcademicas.Domain;

namespace SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas;

public sealed class ConsultarEntidadesAcademicasService : IConsultarEntidadesAcademicasService
{
    private readonly IEntidadAcademicaRepository _repository;

    public ConsultarEntidadesAcademicasService(IEntidadAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntidadAcademicaResultado<EntidadesAcademicasResponse>> ConsultarAsync(
        ConsultarEntidadesAcademicasQuery query,
        CancellationToken cancellationToken)
    {
        var validacion = EntidadAcademicaReglas.ValidarConsulta(
            query.Region,
            query.IdAreaAcademica,
            query.Pagina,
            query.Cantidad);

        if (validacion is not null)
        {
            return EntidadAcademicaResultado<EntidadesAcademicasResponse>.Error(
                TipoResultadoEntidadAcademica.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var pagina = await _repository.ObtenerPorFiltroAsync(
            new EntidadAcademicaFiltro(
                query.Busqueda?.Trim(),
                query.Region?.Trim(),
                query.IdAreaAcademica,
                query.Pagina,
                query.Cantidad),
            cancellationToken);

        var items = pagina.Items.Select(Mapear).ToList();

        return EntidadAcademicaResultado<EntidadesAcademicasResponse>.Exito(
            new EntidadesAcademicasResponse(
                items,
                query.Pagina,
                query.Cantidad,
                pagina.Total));
    }

    private static EntidadAcademicaResponse Mapear(EntidadAcademicaRegistro registro)
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
