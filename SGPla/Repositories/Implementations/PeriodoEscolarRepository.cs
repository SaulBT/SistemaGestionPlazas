using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class PeriodoEscolarRepository : IPeriodoEscolarRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public PeriodoEscolarRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }
        public async Task<Periodo?> ActualizarAsync(Periodo periodoEscolar)
        {
            var actualizado = await _context.Periodo.FindAsync(periodoEscolar.IdPeriodo);

            if (actualizado is null)
                return null;

            actualizado.Codigo = periodoEscolar.Codigo;
            await _context.SaveChangesAsync();

            return periodoEscolar;
        }

        public async Task<Periodo> CrearAsync(Periodo periodoEscolar)
        {
            _context.Periodo.Add(periodoEscolar);
            await _context.SaveChangesAsync();
            return periodoEscolar;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var periodo = await ObtenerPorIdAsync(id);

            if (periodo is not null)
            {
                _context.Periodo.Remove(periodo);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> TieneRelacionesAsync(int idPeriodo)
        {
            return await _context.Periodo
                .Where(p => p.IdPeriodo == idPeriodo)
                .AnyAsync(p => p.Aviso.Any() || p.Oferta.Any());
        }

        public async Task<Periodo?> ExisteAsync(Periodo periodoEscolar)
        {
            return await _context.Periodo.FirstOrDefaultAsync(a =>
               a.Codigo == periodoEscolar.Codigo);
        }

        public async Task<List<Periodo>> ObtenerPorFiltroAsync(BuscarPeriodoEscolarDTO filtro)
        {
            // Crear una nueva consulta independiente
            var query = _context.Periodo.AsQueryable();

            int.TryParse(filtro.Anio, out int anio);

            if (anio > 0)
            {
                string anioStr = anio.ToString();
                query = query.Where(p => p.Codigo.StartsWith(anioStr));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Periodo))
            {
                string periodo = filtro.PeriodoCodigo;
                query = query.Where(p => p.Codigo.EndsWith(periodo));
            }

            query = query.OrderByDescending(a => a.Codigo);
            int pagina = filtro.Pagina <= 0 ? 1 : filtro.Pagina;
            int cantidad = filtro.Cantidad <= 0 ? 10 : filtro.Cantidad;
            int skip = (pagina - 1) * cantidad;

            return await query
                .Skip(skip)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> ContarPorFiltroAsync(BuscarPeriodoEscolarDTO filtro)
        {
            // Crear una nueva consulta independiente (no usar la misma instancia de query)
            var query = _context.Periodo.AsQueryable();

            int.TryParse(filtro.Anio, out int anio);

            if (anio > 0)
            {
                string anioStr = anio.ToString();
                query = query.Where(p => p.Codigo.StartsWith(anioStr));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Periodo))
            {
                string periodo = filtro.PeriodoCodigo;
                query = query.Where(p => p.Codigo.EndsWith(periodo));
            }

            return await query.CountAsync();
        }

        public async Task<Periodo?> ObtenerPorIdAsync(int idPeriodoEscolar)
        {
            return await _context.Periodo.FindAsync(idPeriodoEscolar);

        }

        public async Task<List<Periodo>> ObtenerTodosAsync()
        {
            return await _context.Periodo
               .OrderByDescending(a => a.Codigo)
               .ToListAsync();
        }
    }
}
