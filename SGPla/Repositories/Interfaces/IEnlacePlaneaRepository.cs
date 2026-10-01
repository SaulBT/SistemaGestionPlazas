namespace SGPla.Repositories.Interfaces
{
    public interface IEnlacePlaneaRepository
    {
        /// Ajusta el enlace de las copias PLANEA con un plan de estudios recién cargado o editado:
        /// deja pendientes las que ya no coinciden y enlaza las pendientes que sí. Devuelve las enlazadas.
        Task<int> ReconciliarPlanAsync(int idPlanEstudios, CancellationToken cancellationToken = default);
    }
}
