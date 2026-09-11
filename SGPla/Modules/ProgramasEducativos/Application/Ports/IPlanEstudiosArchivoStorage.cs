using SGPla.Modules.ProgramasEducativos.Application.Models;

namespace SGPla.Modules.ProgramasEducativos.Application.Ports;

public interface IPlanEstudiosArchivoStorage
{
    Task<ArchivoPlanGuardado> GuardarAsync(
        ArchivoPlanParaGuardar archivo,
        CancellationToken cancellationToken);

    Task EliminarAsync(string ruta, CancellationToken cancellationToken);
}
