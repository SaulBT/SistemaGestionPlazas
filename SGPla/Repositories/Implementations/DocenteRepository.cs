using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace SGPla.Repositories.Implementations
{
    public class DocenteRepository : IDocenteRepository
    {

        private readonly GestionDePlazasDbContext _context;

        public DocenteRepository (GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> ObtenerNumerosPersonalRegistradosAsync(
        List<string> numerosPersonal)
        {
            return await _context.Docente
                .Where(d => numerosPersonal.Contains(d.NumeroPersonal))
                .Select(d => d.NumeroPersonal)
                .ToListAsync();
        }
    }
}
