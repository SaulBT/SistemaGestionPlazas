namespace SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Contracts;

public sealed record CrearAreaAcademicaCommand(
    string? Nombre,
    string? Telefono,
    string? Extension);
