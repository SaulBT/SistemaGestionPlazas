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
            actualizado.Campus = programaEducativo.Campus;
            actualizado.IdEntidadAcademica = programaEducativo.IdEntidadAcademica;
            await _context.SaveChangesAsync();

            return programaEducativo;
        }

        public async Task<ProgramaEducativo> CrearAsync(ProgramaEducativo programaEducativo)
        {
            _context.ProgramaEducativo.Add(programaEducativo);
            await _context.SaveChangesAsync();
            return programaEducativo;
        }

        public async Task<ProgramaEducativo?> ExisteAsync(ProgramaEducativo programaEducativo)
        {
            string clave = programaEducativo.Nombre.Split('-', 2)[0].Trim();

            string prefijoClave = $"{clave}-";

            return await _context.ProgramaEducativo
                .FirstOrDefaultAsync(a =>
                    a.Nombre != null &&
                    a.Nombre.StartsWith(prefijoClave) 
                );
        }

        public async Task<List<ProgramaEducativo>> ObtenerPorFiltroAsync(BuscarProgramaEducativoDTO filtro)
        {
            var lista = _context.ProgramaEducativo
                .AsNoTracking()
                .Include(p => p.IdEntidadAcademicaNavigation)
                    .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .AsQueryable();

            if (filtro.IdAreaAcademica.HasValue && filtro.IdAreaAcademica.Value > 0)
                lista = lista.Where(a => a.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica.Value);

            if (filtro.IdEntidadAcademica.HasValue && filtro.IdEntidadAcademica.Value > 0)
                lista = lista.Where(a => a.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                lista = lista.Where(a => a.Nombre.Contains(filtro.Nombre.Trim()));

            if (!string.IsNullOrWhiteSpace(filtro.Region))
                lista = lista.Where(a => a.IdEntidadAcademicaNavigation.Region == filtro.Region);

            return await lista
                .OrderBy(a => a.Nombre)
                .Skip((filtro.Pagina - 1) * filtro.Cantidad)
                .Take(filtro.Cantidad)
                .ToListAsync();
        }

        public async Task<ProgramaEducativo?> ObtenerPorIdAsync(int id)
        {
            return await _context.ProgramaEducativo
                .AsNoTracking()
                .Include(p => p.IdEntidadAcademicaNavigation)
                    .ThenInclude(e => e.IdAreaAcademicaNavigation)
                .FirstOrDefaultAsync(p => p.IdProgramaEducativo == id);
        }

        public async Task<List<ProgramaEducativo>> ObtenerTodosAsync()
        {
            return await _context.ProgramaEducativo
                .Include(p => p.IdEntidadAcademicaNavigation)
                .Include(aa => aa.IdEntidadAcademicaNavigation.IdAreaAcademicaNavigation)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<bool> EstaAsociadoAPlan(int id)
        {
            return await _context.PlanEstudios.AnyAsync(plan => plan.IdProgramaEducativo == id);

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

        public async Task<int> ContarPorFiltroAsync(BuscarProgramaEducativoDTO filtro)
        {
            // Crear una nueva consulta independiente (no usar la misma instancia de query)
            var query = _context.ProgramaEducativo.AsQueryable();

         

            if (filtro.IdAreaAcademica.HasValue && filtro.IdAreaAcademica.Value > 0)
                query = query.Where(a => a.IdEntidadAcademicaNavigation.IdAreaAcademica == filtro.IdAreaAcademica.Value);
            if (filtro.IdEntidadAcademica.HasValue && filtro.IdEntidadAcademica.Value > 0)
                query = query.Where(a => a.IdEntidadAcademica == filtro.IdEntidadAcademica.Value);
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query = query.Where(a => a.Nombre.Contains(filtro.Nombre.Trim()));
            if (!string.IsNullOrWhiteSpace(filtro.Region))
                query = query.Where(a => a.IdEntidadAcademicaNavigation.Region == filtro.Region);

           

            return await query.CountAsync();
        }

        public async Task<List<string>> ObtenerNombresProgramasRegistradosAsync(
        List<string> programas)
        {
            return await _context.ProgramaEducativo
                .Where(p => programas.Contains(p.Nombre))
                .Select(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> ObtenerIdsProgramasAsync(
     List<string> programas)
        {
            return await _context.ProgramaEducativo
                .Where(p => programas.Contains(p.Nombre))
                .ToDictionaryAsync(
                    p => p.Nombre,
                    p => p.IdProgramaEducativo);
        }
    }
}





