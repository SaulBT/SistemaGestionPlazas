using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public sealed class EnlacePlaneaRepository(GestionDePlazasDbContext context) : IEnlacePlaneaRepository
    {
        private readonly GestionDePlazasDbContext _context = context;

        public async Task<int> ReconciliarPlanAsync(int idPlanEstudios, CancellationToken cancellationToken = default)
        {
            // Si quien llama ya abrió una transacción, el enlace forma parte de ella.
            await using var transaccion = _context.Database.CurrentTransaction is null
                ? await _context.Database.BeginTransactionAsync(cancellationToken)
                : null;

            await _context.Database.ExecuteSqlRawAsync(EnlacePlaneaSql.DesenlazarDesactualizadas,
                [new SqlParameter("@idPlanEstudios", idPlanEstudios)], cancellationToken);

            var enlazadas = await _context.Database.ExecuteSqlRawAsync(EnlacePlaneaSql.EnlazarPendientes,
                [new SqlParameter("@idPlanEstudios", idPlanEstudios), new SqlParameter("@idPeriodo", SqlDbType.Int) { Value = DBNull.Value }],
                cancellationToken);

            if (transaccion is not null)
                await transaccion.CommitAsync(cancellationToken);
            return enlazadas;
        }
    }
}
