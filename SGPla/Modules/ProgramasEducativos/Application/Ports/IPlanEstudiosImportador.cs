using SGPla.Modules.ProgramasEducativos.Application.Models;

namespace SGPla.Modules.ProgramasEducativos.Application.Ports;

public interface IPlanEstudiosImportador
{
    PlanEstudiosImportacionResultado Importar(ArchivoPlanContenido archivo);
}
