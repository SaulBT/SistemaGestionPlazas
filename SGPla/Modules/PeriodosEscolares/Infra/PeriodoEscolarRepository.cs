using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Models;
using SGPla.Modules.PeriodosEscolares.Application.Models;
using SGPla.Modules.PeriodosEscolares.Application.Ports;

namespace SGPla.Modules.PeriodosEscolares.Infra;

public sealed class PeriodoEscolarRepository : IPeriodoEscolarRepository
{
    private readonly GestionDePlazasDbContext _context;

    public PeriodoEscolarRepository(GestionDePlazasDbContext context)
    {
        _context = context;
    }

    public async Task<PeriodosEscolaresPagina> ObtenerPorFiltroAsync(
        PeriodoEscolarFiltro filtro,
        CancellationToken cancellationToken)
    {
        var query = _context.Periodo
            .AsNoTracking()
            .AsQueryable();

        if (filtro.Anio.HasValue)
        {
            var anio = filtro.Anio.Value.ToString();
            query = query.Where(periodo => periodo.Codigo.StartsWith(anio));
        }

        if (!string.IsNullOrWhiteSpace(filtro.CodigoPeriodo))
        {
            query = query.Where(periodo =>
                periodo.Codigo.EndsWith(filtro.CodigoPeriodo));
        }

        var total = await query.CountAsync(cancellationToken);
        var saltar = (filtro.Pagina - 1) * filtro.Cantidad;

        var items = await query
            .OrderByDescending(periodo => periodo.Codigo)
            .Skip(saltar)
            .Take(filtro.Cantidad)
            .Select(periodo => new PeriodoEscolarRegistro(
                periodo.IdPeriodo,
                periodo.Codigo))
            .ToListAsync(cancellationToken);

        return new PeriodosEscolaresPagina(items, total);
    }

    public async Task<PeriodoEscolarRegistro?> ObtenerPorIdAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken)
    {
        return await _context.Periodo
            .AsNoTracking()
            .Where(periodo => periodo.IdPeriodo == idPeriodoEscolar)
            .Select(periodo => new PeriodoEscolarRegistro(
                periodo.IdPeriodo,
                periodo.Codigo))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistePorCodigoAsync(
        string codigo,
        int? idPeriodoEscolarExcluido,
        CancellationToken cancellationToken)
    {
        return _context.Periodo
            .AsNoTracking()
            .AnyAsync(periodo =>
                periodo.Codigo == codigo
                && (!idPeriodoEscolarExcluido.HasValue
                    || periodo.IdPeriodo != idPeriodoEscolarExcluido.Value),
                cancellationToken);
    }

    public async Task<PeriodoEscolarRegistro> CrearAsync(
        PeriodoEscolarParaCrear periodo,
        CancellationToken cancellationToken)
    {
        var entidad = new Periodo
        {
            Codigo = periodo.Codigo
        };

        _context.Periodo.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        return new PeriodoEscolarRegistro(
            entidad.IdPeriodo,
            entidad.Codigo);
    }

    public async Task<PeriodoEscolarRegistro?> ActualizarAsync(
        PeriodoEscolarParaActualizar periodo,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.Periodo
            .FirstOrDefaultAsync(
                elemento => elemento.IdPeriodo == periodo.IdPeriodoEscolar,
                cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        entidad.Codigo = periodo.Codigo;
        await _context.SaveChangesAsync(cancellationToken);

        return new PeriodoEscolarRegistro(
            entidad.IdPeriodo,
            entidad.Codigo);
    }

    public async Task<bool> TieneRelacionesAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken)
    {
        if (await _context.Aviso.AnyAsync(
                aviso => aviso.IdPeriodo == idPeriodoEscolar,
                cancellationToken))
        {
            return true;
        }

        if (await _context.CargaAcademica.AnyAsync(
                carga => carga.IdPeriodo == idPeriodoEscolar,
                cancellationToken))
        {
            return true;
        }

        if (await _context.Oferta.AnyAsync(
                oferta => oferta.IdPeriodo == idPeriodoEscolar,
                cancellationToken))
        {
            return true;
        }

        return await _context.SolicitudApertura.AnyAsync(
            solicitud => solicitud.IdPeriodo == idPeriodoEscolar,
            cancellationToken);
    }

    public async Task<bool> EliminarAsync(
        int idPeriodoEscolar,
        CancellationToken cancellationToken)
    {
        var entidad = await _context.Periodo
            .FirstOrDefaultAsync(
                periodo => periodo.IdPeriodo == idPeriodoEscolar,
                cancellationToken);

        if (entidad is null)
        {
            return false;
        }

        _context.Periodo.Remove(entidad);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
