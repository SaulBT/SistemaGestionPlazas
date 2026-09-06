using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Contracts;
using SGPla.Modules.DireccionesAreaAcademica.Application.Models;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Domain;

namespace SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas;

public sealed class ConsultarAreasAcademicasService : IConsultarAreasAcademicasService
{
    private readonly IAreaAcademicaRepository _repository;

    public ConsultarAreasAcademicasService(IAreaAcademicaRepository repository)
    {
        _repository = repository;
    }

    public async Task<AreaAcademicaResultado<AreasAcademicasResponse>> ConsultarAsync(
        ConsultarAreasAcademicasQuery query,
        CancellationToken cancellationToken)
    {
        var validacion = AreaAcademicaReglas.ValidarConsulta(
            query.Pagina,
            query.Cantidad);

        if (validacion is not null)
        {
            return AreaAcademicaResultado<AreasAcademicasResponse>.Error(
                TipoResultadoAreaAcademica.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var pagina = await _repository.ObtenerPorFiltroAsync(
            new AreaAcademicaFiltro(
                query.Busqueda?.Trim(),
                query.Pagina,
                query.Cantidad),
            cancellationToken);

        var items = pagina.Items.Select(Mapear).ToList();

        return AreaAcademicaResultado<AreasAcademicasResponse>.Exito(
            new AreasAcademicasResponse(
                items,
                query.Pagina,
                query.Cantidad,
                pagina.Total));
    }

    private static AreaAcademicaResponse Mapear(AreaAcademicaRegistro registro)
    {
        return new AreaAcademicaResponse(
            registro.IdAreaAcademica,
            registro.Nombre,
            registro.Telefono,
            registro.Extension);
    }
}
