namespace SGPla.Models.DTOs.Catalogos;

public enum TipoCatalogoNormalizado
{
    SistemaEducativo = 1,
    NivelFormacion = 2,
    AreaFormacion = 3,
    GradoAcademico = 4,
    TratamientoAcademico = 5,
    ModalidadRecepcion = 6,
    TipoPlaza = 7,
    TipoContratacion = 8,
    TipoDocumentoAspirante = 9
}

public sealed record CatalogoAdministracionMvcFila(
    TipoCatalogoNormalizado Tipo,
    string Categoria,
    int Id,
    string? Clave,
    string Nombre,
    int Referencias,
    bool Activo,
    string? Detalle,
    bool Clasificacion);

public sealed class CrearCatalogoClasificacionMvcDto
{
    public TipoCatalogoNormalizado Tipo { get; init; }
    public string? Clave { get; init; }
    public string Nombre { get; init; } = string.Empty;
}

public sealed class EditarNombreCatalogoMvcDto
{
    public TipoCatalogoNormalizado Tipo { get; init; }
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}
