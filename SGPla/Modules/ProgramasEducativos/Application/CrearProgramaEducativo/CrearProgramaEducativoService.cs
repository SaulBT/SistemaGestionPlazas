using SGPla.Modules.ProgramasEducativos.Application.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Contracts;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Models;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Domain;

namespace SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo;

public sealed class CrearProgramaEducativoService : ICrearProgramaEducativoService
{
    private readonly IProgramaEducativoRepository _repository;

    public CrearProgramaEducativoService(IProgramaEducativoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProgramaEducativoResultado<ProgramaEducativoResponse>> CrearAsync(
        CrearProgramaEducativoCommand command,
        CancellationToken cancellationToken)
    {
        var validacion = ProgramaEducativoReglas.ValidarDatos(
            command.Nombre,
            command.Campus,
            command.IdEntidadAcademica);

        if (validacion is not null)
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Validacion,
                validacion.Mensaje,
                validacion.Campo);
        }

        var datos = ProgramaEducativoReglas.Normalizar(
            command.Nombre!,
            command.Campus!,
            command.IdEntidadAcademica!.Value);

        if (!await _repository.ExisteEntidadAcademicaActivaAsync(
                datos.IdEntidadAcademica,
                cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.NoEncontrado,
                "La entidad académica indicada no existe.",
                "idEntidadAcademica");
        }

        var clave = ProgramaEducativoReglas.ObtenerClave(datos.Nombre);
        if (await _repository.ExistePorClaveAsync(clave, null, cancellationToken))
        {
            return ProgramaEducativoResultado<ProgramaEducativoResponse>.Error(
                TipoResultadoProgramaEducativo.Conflicto,
                "Ya existe un programa educativo con la misma clave.",
                "nombre");
        }

        var registro = await _repository.CrearAsync(
            new ProgramaEducativoParaCrear(
                datos.Nombre,
                datos.Campus,
                datos.IdEntidadAcademica),
            cancellationToken);

        return ProgramaEducativoResultado<ProgramaEducativoResponse>.Exito(Mapear(registro));
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
