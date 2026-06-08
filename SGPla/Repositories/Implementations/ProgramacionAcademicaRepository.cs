using SGPla.Data;
using SGPla.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.Oferta;
using SGPla.Models;


namespace SGPla.Repositories.Implementations
{
    public class ProgramacionAcademicaRepository : IProgramacionAcademicaRepository
    {
        private readonly GestionDePlazasDbContext _context;

        public ProgramacionAcademicaRepository(GestionDePlazasDbContext context)
        {
            _context = context;
        }

        public async Task<bool> GuardarOfertas(List<Oferta> ofertas)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
               
                await _context.Oferta.AddRangeAsync(ofertas);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                //cambiar tipo que regresa

                throw e;

            }
        }

        

        public async Task<List<string>> ObtenerRelacionesValidasAsync(List<OfertaDTO> ofertas)
        {
            var relaciones = ofertas
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Programa) &&
                    !string.IsNullOrWhiteSpace(x.ExperienciaEducativa))
                .Select(x => $"{x.Programa}|{x.ExperienciaEducativa}")
                .Distinct()
                .ToList();

            var relacionesValidas = await _context.ExperienciaEducativa
                .Select(ee =>
                    ee.IdPlanEstudiosNavigation
                        .IdProgramaEducativoNavigation.Nombre
                    + "|" +
                    ee.Nombre)
                .Where(relacion => relaciones.Contains(relacion))
                .Distinct()
                .ToListAsync();

            return relacionesValidas;
        }
    }
}