namespace SGPla.Models.DTOs.Ofertas;

public sealed record OfertaMvcOpcion(int Id, string Nombre);
public sealed record ProgramacionOfertaMvc(int ProgramacionAcademicaId, string Nrc, int EntidadAcademicaId,
    string EntidadAcademica, string ProgramaEducativo, string ExperienciaEducativa, bool TieneDocenteVigente);
public sealed record OfertaMvcNuevaDatos(ProgramacionOfertaMvc Programacion,
    IReadOnlyList<OfertaMvcOpcion> TiposPlaza, IReadOnlyList<OfertaMvcOpcion> TiposContratacion);
public sealed record CrearOfertaMvcDatos(int ProgramacionAcademicaId, string ClavePlaza, int TipoPlazaId,
    int TipoContratacionId, string PerfilSolicitado, string? Justificacion);
