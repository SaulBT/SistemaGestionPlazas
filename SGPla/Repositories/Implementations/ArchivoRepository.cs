using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public class ArchivoRepository : IArchivoRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ArchivoRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<Archivo> CrearAsync(Archivo archivo)
        {
            await _context.Archivo.AddAsync(archivo);
            await _context.SaveChangesAsync();
            return archivo;
        }

        public async Task<Archivo?> ObtenerPorIdAsync(int idArchivo)
        {
            return await _context.Archivo
                .FirstOrDefaultAsync(a => a.IdArchivo == idArchivo);
        }

        public async Task ActualizarAsync(Archivo archivo)
        {
            _context.Archivo.Update(archivo);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Archivo archivo)
        {
            ArgumentNullException.ThrowIfNull(archivo);

            _context.Archivo.Remove(archivo);
            await _context.SaveChangesAsync();
        }
    }
}