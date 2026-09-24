using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SGPla.Models.DTOs.PlanEstudios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlanEstudiosMvcService : IPlanEstudiosMvcService
{
    private static readonly Regex CodigoRegex = new("^[A-Z0-9._-]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private readonly IPlanEstudiosMvcRepository _repository;
    private readonly TimeProvider _timeProvider;

    public PlanEstudiosMvcService(IPlanEstudiosMvcRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public Task<PaginaPlanesEstudiosMvc> BuscarAsync(PlanEstudiosMvcFiltro filtro, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (filtro.Pagina < 1 || filtro.TamanoPagina is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(filtro), "La página o tamaño solicitados no son válidos.");
        return _repository.BuscarAsync(filtro, cancellationToken);
    }

    public Task<PlanEstudiosMvcDetalle?> ObtenerAsync(int id, CancellationToken cancellationToken = default) =>
        id < 1 ? Task.FromResult<PlanEstudiosMvcDetalle?>(null) : _repository.ObtenerAsync(id, cancellationToken);

    public async Task<int> CrearAsync(GuardarPlanEstudiosMvcDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        if (codigo.Length is < 1 or > 50 || !CodigoRegex.IsMatch(codigo))
            throw new ArgumentException("El código debe contener entre 1 y 50 caracteres alfanuméricos, punto, guion o guion bajo.");
        if (dto.ProgramaEducativoId < 1 || !await _repository.ProgramaActivoAsync(dto.ProgramaEducativoId, cancellationToken))
            throw new ArgumentException("Selecciona un Programa Educativo vigente.");
        try
        {
            return await _repository.CrearAsync(new GuardarPlanEstudiosMvcDto { Codigo = codigo, ProgramaEducativoId = dto.ProgramaEducativoId }, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sqlException)
        {
            throw new InvalidOperationException("Ya existe ese código de Plan para el Programa Educativo seleccionado.", sqlException);
        }
    }

    public Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.EliminarAsync(id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    public Task<PaginaExperienciasEducativasMvc> BuscarExperienciasAsync(
        int planId, string? busqueda, int pagina, int tamanoPagina, CancellationToken cancellationToken = default)
    {
        if (planId < 1) throw new ArgumentOutOfRangeException(nameof(planId));
        if (pagina < 1 || tamanoPagina is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pagina));
        return _repository.BuscarExperienciasAsync(planId, busqueda, pagina, tamanoPagina, cancellationToken);
    }

    public Task<ExperienciaEducativaMvcDto?> ObtenerExperienciaAsync(int planId, int id, CancellationToken cancellationToken = default) =>
        planId < 1 || id < 1 ? Task.FromResult<ExperienciaEducativaMvcDto?>(null) : _repository.ObtenerExperienciaAsync(planId, id, cancellationToken);

    public Task<bool> TieneProgramacionAsync(int experienciaEducativaId, CancellationToken cancellationToken = default) =>
        experienciaEducativaId > 0 ? _repository.TieneProgramacionAsync(experienciaEducativaId, cancellationToken) : Task.FromResult(false);

    public async Task<int> CrearExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default)
    {
        ValidarExperiencia(dto);
        if (await _repository.ObtenerAsync(dto.PlanEstudiosId, cancellationToken) is null)
            throw new ArgumentException("El Plan de Estudios no existe o está inactivo.");
        if (!await _repository.AreaFormacionActivaAsync(dto.AreaFormacionId, cancellationToken))
            throw new ArgumentException("Selecciona un Área de Formación vigente.");
        try
        {
            return await _repository.CrearExperienciaAsync(dto, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sqlException)
        {
            throw new InvalidOperationException("Ya existe una Experiencia Educativa con esa materia y curso en este Plan.", sqlException);
        }
    }

    public async Task<int> ImportarExperienciasAsync(
        int planId, int areaFormacionId, IReadOnlyList<GuardarExperienciaEducativaMvcDto> experiencias,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(experiencias);
        if (planId < 1 || experiencias.Count is < 1 or > 10000)
            throw new ArgumentException("El archivo debe contener entre 1 y 10,000 Experiencias Educativas válidas.");
        if (await _repository.ObtenerAsync(planId, cancellationToken) is null)
            throw new ArgumentException("El Plan de Estudios no existe o está inactivo.");
        if (!await _repository.AreaFormacionActivaAsync(areaFormacionId, cancellationToken))
            throw new ArgumentException("Selecciona un Área de Formación vigente.");

        var claves = new HashSet<string>(StringComparer.Ordinal);
        var filas = new List<GuardarExperienciaEducativaMvcDto>(experiencias.Count);
        foreach (var experiencia in experiencias)
        {
            var fila = new GuardarExperienciaEducativaMvcDto
            {
                PlanEstudiosId = planId,
                Nombre = experiencia.Nombre,
                MateriaEe = experiencia.MateriaEe,
                CursoEe = experiencia.CursoEe,
                HorasTeoricas = experiencia.HorasTeoricas,
                HorasPracticas = experiencia.HorasPracticas,
                Creditos = experiencia.Creditos,
                PerfilDocente = experiencia.PerfilDocente,
                AreaFormacionId = areaFormacionId
            };
            ValidarExperiencia(fila);
            var clave = $"{fila.MateriaEe.Trim().ToUpperInvariant()}\u001f{fila.CursoEe.Trim().ToUpperInvariant()}";
            if (!claves.Add(clave)) throw new ArgumentException("El archivo contiene materia y curso duplicados.");
            filas.Add(fila);
        }
        try
        {
            return await _repository.CrearExperienciasAsync(filas, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sqlException)
        {
            throw new InvalidOperationException("Una o más Experiencias ya existen en este Plan.", sqlException);
        }
    }

    public async Task<bool> ActualizarExperienciaAsync(GuardarExperienciaEducativaMvcDto dto, CancellationToken cancellationToken = default)
    {
        ValidarExperiencia(dto);
        if (!await _repository.AreaFormacionActivaAsync(dto.AreaFormacionId, cancellationToken))
            throw new ArgumentException("Selecciona un Área de Formación vigente.");
        return await _repository.ActualizarExperienciaAsync(dto, cancellationToken);
    }

    public Task<bool> EliminarExperienciaAsync(int planId, int id, CancellationToken cancellationToken = default) =>
        _repository.EliminarExperienciaAsync(planId, id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    private static void ValidarExperiencia(GuardarExperienciaEducativaMvcDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.PlanEstudiosId < 1 || (dto.Id < 0)) throw new ArgumentException("La referencia de la Experiencia Educativa no es válida.");
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre es obligatorio y no debe exceder 200 caracteres.");
        ValidarCodigo(dto.MateriaEe, "materia");
        ValidarCodigo(dto.CursoEe, "curso");
        if (dto.HorasTeoricas < 0 || dto.HorasPracticas < 0)
            throw new ArgumentException("Las horas teóricas y prácticas deben ser enteros no negativos.");
        if (dto.Creditos <= 0) throw new ArgumentException("Los créditos deben ser un entero positivo.");
        if (dto.PerfilDocente?.Trim().Length == 0)
            throw new ArgumentException("El perfil docente debe omitirse o contener texto.");
    }

    private static void ValidarCodigo(string? value, string field)
    {
        var codigo = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(codigo) || codigo.Length > 50 || !CodigoRegex.IsMatch(codigo))
            throw new ArgumentException($"El código de {field} debe contener entre 1 y 50 caracteres alfanuméricos, punto, guion o guion bajo.");
    }
}
