using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class HorarioRepository : IHorarioRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public HorarioRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Horario>> ObtenerPorIdOferta(int idOferta)
        {
            return await _context.Horario
                .AsNoTracking()
                .Where(horario => horario.IdOferta == idOferta)
                .ToListAsync();
        }

        public async Task<List<Horario>> ObtenerPorIdAviso(int idAviso)
        {
            return await _context.Horario
                .AsNoTracking()
                .Where(horario => horario.IdAviso == idAviso)
                .ToListAsync();
        }

        public async Task CrearHorarios(List<Horario> horarios)
        {
            if (horarios.Count == 0)
                return;
            await _context.Horario.AddRangeAsync(horarios);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarHorariosPorId(List<int> idsHorarios)
        {
            if (idsHorarios.Count == 0)
                return;
            var horarios = await _context.Horario
                .Where(horario => idsHorarios.Contains(horario.IdHorario))
                .ToListAsync();
            _context.Horario.RemoveRange(horarios);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarHorariosPorId(List<Horario> horarios)
        {
            if (horarios.Count == 0)
                return;
            _context.Horario.UpdateRange(horarios);
            await _context.SaveChangesAsync();
        }


    }
}
