namespace SGPla.Models.ViewModels.Catalogos;

public sealed record CatalogoOpcion(int Id, string Clave, string Nombre);

public sealed class CatalogosEntidadAcademicaViewModel
{
    public IReadOnlyList<CatalogoOpcion> Regiones { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Campus { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> AreasAcademicas { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Municipios { get; init; } = [];
}

public sealed class CatalogosViewModel
{
    public IReadOnlyList<CatalogoOpcion> Regiones { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Campus { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> AreasAcademicas { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Municipios { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> SistemasEducativos { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> NivelesFormacion { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> AreasFormacion { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> GradosAcademicos { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> TratamientosAcademicos { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> ModalidadesRecepcion { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> TiposPlaza { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> TiposContratacion { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> TiposDocumentoAspirante { get; init; } = [];
    public IReadOnlyList<CatalogoOpcion> Articulos { get; init; } = [];
}

public sealed class CatalogosAdministracionIndexViewModel
{
    public CatalogosViewModel Catalogos { get; init; } = new();
    public IReadOnlyList<SGPla.Models.DTOs.Catalogos.CatalogoAdministracionMvcFila> Administrables { get; init; } = [];
}
