namespace SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Contracts;

public sealed record ActualizarAreaAcademicaCommand(
    int IdAreaAcademica,
    string? Nombre,
    string? Telefono,
    string? Extension);
