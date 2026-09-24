using SGPla.Models.DTOs.PlanEstudios;

namespace SGPla.Services.Interfaces;

public interface IPlanEstudiosExcelImportador
{
    PlanEstudiosExcelImportacionMvc Leer(Stream archivo, string codigoPlan, CancellationToken cancellationToken = default);
}
