namespace SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Contracts;

public sealed record CrearEntidadAcademicaCommand(
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
