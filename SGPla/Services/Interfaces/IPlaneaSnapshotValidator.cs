using SGPla.Models.DTOs.Integracion;

namespace SGPla.Services.Interfaces;

public interface IPlaneaSnapshotValidator
{
    PlaneaSnapshotValidado Validar(string clavePeriodo, DateOnly inicioPeriodo, DateOnly finPeriodo,
        IReadOnlyDictionary<string, int> programacionesPorNrc, IReadOnlyList<PlaneaRegistro> registros);
}
