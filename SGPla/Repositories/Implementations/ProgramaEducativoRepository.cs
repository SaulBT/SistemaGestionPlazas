using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace SGPla.Repositories.Implementations
{
    public class ProgramaEducativoRepository : IProgramaEducativoRepository
    {

        private readonly GestionDePlazasDbContext _context;

        public ProgramaEducativoRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<ProgramaEducativo?> ActualizarAsync(ProgramaEducativo programaEducativo)
        {
            var actualizado = await _context.ProgramaEducativo.FindAsync(programaEducativo.IdProgramaEducativo);

            if (actualizado is null)
                return null;

            actualizado.Nombre = programaEducativo.Nombre;
            actualizado.IdEntidadAcademica = programaEducativo.IdEntidadAcademica;
            await _context.SaveChangesAsync();

            return actualizado;
        }

        public async Task<ProgramaEducativo> CrearAsync(ProgramaEducativo programaEducativo)
        {
            _context.ProgramaEducativo.Add(programaEducativo);
            await _context.SaveChangesAsync();
            return programaEducativo;
        }

        public async Task<ProgramaEducativo?> ExisteAsync(ProgramaEducativo programaEducativo)
        {
            return await _context.ProgramaEducativo.FirstOrDefaultAsync(a =>
                a.Nombre == programaEducativo.Nombre &&
                a.IdEntidadAcademica == programaEducativo.IdEntidadAcademica
            );

        }

        public async Task<List<ProgramaEducativo>> ObtenerPorFiltrosAsync(string busqueda)
        {
            var texto = busqueda.Trim();

            var query = _context.ProgramaEducativo.AsQueryable();

            if (!string.IsNullOrEmpty(texto))
            {
                query = query.Where(a =>
                    a.Nombre.Contains(texto)
                );
            }

            var resultados = await query.ToListAsync();

            return resultados;
        }

        public Task<ProgramaEducativo?> ObtenerPorIdAsync(int id)
        {
            return _context.ProgramaEducativo.FindAsync(id).AsTask();
        }

        public async Task<List<ProgramaEducativo>> ObtenerTodosAsync()
        {
            return await _context.ProgramaEducativo.OrderBy(a => a.Nombre).ToListAsync();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var programaEducativo = await ObtenerPorIdAsync(id);

            if (programaEducativo is not null)
            {
                _context.ProgramaEducativo.Remove(programaEducativo);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}





