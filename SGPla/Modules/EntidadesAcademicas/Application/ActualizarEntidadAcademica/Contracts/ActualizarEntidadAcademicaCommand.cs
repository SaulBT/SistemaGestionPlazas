namespace SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Contracts;

public sealed record ActualizarEntidadAcademicaCommand(
    int IdEntidadAcademica,
    string? Clave,
    string? Nombre,
    string? CalleNumero,
    string? Colonia,
    string? Cp,
    string? Municipio,
    string? Telefono,
    string? Extension,
    int? IdAreaAcademica,
    string? Region);
