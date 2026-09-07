namespace SGPla.Commons;

/// <summary>
/// Nombres centralizados de las políticas de autorización de la aplicación.
/// Una sesión representa un único rol efectivo.
/// </summary>
public static class PoliticasAutorizacion
{
    public const string SuperUsuario = nameof(SuperUsuario);
    public const string Dgaa = nameof(Dgaa);
    public const string EntidadAcademica = nameof(EntidadAcademica);
    public const string OperadorAcademico = nameof(OperadorAcademico);
}
