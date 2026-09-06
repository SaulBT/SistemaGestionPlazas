using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos;

public sealed class ConsultarProgramasEducativosService : IConsultarProgramasEducativosService
{
    private readonly IProgramaEducativoRepository _repository;

    public ConsultarProgramasEducativosService(IProgramaEducativoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProgramaEducativoResultado<ProgramasEducativosResponse>> ConsultarAsync(
        ConsultarProgramasEducativosQuery query,
        CancellationToken cancellationToken)
    {
        var validacion = ProgramaEducativoReglas.ValidarConsulta(
            query.Region,
            query.IdAreaAcademica,
            query.IdEntidadAcademica,
            query.Pagina,
            query.Cantidad);

        if (validacion is not null)
        {
            return ProgramaEducativoResultado<ProgramasEducativosResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var pagina = await _repository.ObtenerPorFiltroAsync(
            new ProgramaEducativoFiltro(
                query.Busqueda?.Trim(),
                query.Region?.Trim(),
                query.IdAreaAcademica,
                query.IdEntidadAcademica,
                query.Pagina,
                query.Cantidad),
            cancellationToken);

        var items = pagina.Items.Select(Mapear).ToList();

        return ProgramaEducativoResultado<ProgramasEducativosResponse>.Exito(
            new ProgramasEducativosResponse(
                items,
                query.Pagina,
                query.Cantidad,
                pagina.Total));
    }

    private static ProgramaEducativoResponse Mapear(
        ProgramaEducativoRegistro registro)
    {
        return new ProgramaEducativoResponse(
            registro.IdProgramaEducativo,
            registro.Nombre,
            registro.Campus,
            registro.IdEntidadAcademica,
            registro.NombreEntidadAcademica,
            registro.IdAreaAcademica,
            registro.NombreAreaAcademica,
            registro.Region);
    }
}
