using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.ProgramaEducativo;


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

        public async Task<List<ProgramaEducativo>> ObtenerPorFiltroAsync(BuscarProgramaEducativoDTO filtro)
        {

            var query = _context.ProgramaEducativo.AsQueryable();

            
                query = query.Where(a =>
    (string.IsNullOrEmpty(filtro.Nombre) || a.Nombre.Contains(filtro.Nombre)) &&
    (!filtro.IdEntidadAcademica.HasValue || a.IdEntidadAcademica == filtro.IdEntidadAcademica) &&
    (!filtro.IdAreaAcademica.HasValue ||
        (a.IdEntidadAcademicaNavigation != null &&
         a.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica)) &&
       (string.IsNullOrEmpty(filtro.Region) || a.IdEntidadAcademicaNavigation.Region.Contains(filtro.Region))
);
            

            var resultados = await query.Include(p => p.IdEntidadAcademicaNavigation)
                .Include(aa => aa.IdEntidadAcademicaNavigation.IdAreaAcademicaNavigation).ToListAsync();

            return resultados;
        }

        public async Task<ProgramaEducativo?> ObtenerPorIdAsync(int id)
        {
            return await _context.ProgramaEducativo.FindAsync(id);
        }

        public async Task<List<ProgramaEducativo>> ObtenerTodosAsync()
        {
            return await _context.ProgramaEducativo
                .Include(p => p.IdEntidadAcademicaNavigation)
                .Include(aa => aa.IdEntidadAcademicaNavigation.IdAreaAcademicaNavigation)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
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





