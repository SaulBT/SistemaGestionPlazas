namespace SGPla.Models.DTOs.Solicitudes;

public sealed record FormacionSolicitudMvcDatos(int GradoAcademicoId, string Descripcion);

public sealed record RegistrarSolicitudMvcDatos(
    int AvisoOfertaId,
    string Nombre,
    string Correo,
    string? PuestoActual,
    string DescripcionPerfil,
    IReadOnlyList<FormacionSolicitudMvcDatos> Formaciones,
    string? Observaciones);

public sealed record EditarPerfilSolicitudMvcDatos(
    string Nombre,
    string Correo,
    string? PuestoActual,
    string DescripcionPerfil,
    IReadOnlyList<FormacionSolicitudMvcDatos> Formaciones);

public sealed record ResolverSolicitudMvcDatos(bool Admitir, string? MotivoNoAdmision);

public sealed record GradoAcademicoSolicitudMvcOpcion(int Id, string Nombre);
public sealed record TipoDocumentoAspiranteSolicitudMvcOpcion(int Id, string Nombre);

public sealed record VacanteSolicitudMvcOpcion(int AvisoOfertaId, string ClavePlaza, string Nrc,
    string Materia, string Curso);

public sealed record FormacionSolicitudMvcItem(int Id, int GradoAcademicoId, string Grado, string Descripcion);

public sealed record DocumentoSolicitudMvcItem(int DocumentoAspiranteId, int VersionId, int TipoDocumentoId, string TipoDocumento,
    string Nombre, int NumeroVersion, bool EsVigente);

public sealed record DocumentoAspiranteMvcDatos(int TipoDocumentoId, int? DocumentoAspiranteId, string Nombre, string Mime,
    long Tamano, byte[] ChecksumSha256, string ClaveRelativa);

public sealed record DescargaDocumentoSolicitudMvc(Stream Contenido, string Nombre, string Mime);

public sealed record SolicitudMvcItem(
    int Id,
    int AvisoOfertaId,
    string Estado,
    string Nombre,
    string Correo,
    DateTime RegistradaEn,
    int PerfilAspiranteId);

public sealed record SolicitudMvcDetalle(
    int Id,
    int AvisoId,
    int AvisoOfertaId,
    int AspiranteId,
    int PerfilAspiranteId,
    string Estado,
    string Nombre,
    string Correo,
    string? PuestoActual,
    string DescripcionPerfil,
    string? Observaciones,
    DateTime RegistradaEn,
    DateTime? AdmitidaEn,
    DateTime? NoAdmitidaEn,
    string? MotivoNoAdmision,
    IReadOnlyList<FormacionSolicitudMvcItem> Formaciones,
    IReadOnlyList<DocumentoSolicitudMvcItem> Documentos);

public sealed record SolicitudMvcListado(
    int AvisoId,
    string EstadoAviso,
    IReadOnlyList<SolicitudMvcItem> Solicitudes,
    IReadOnlyList<GradoAcademicoSolicitudMvcOpcion> GradosAcademicos,
    IReadOnlyList<VacanteSolicitudMvcOpcion> VacantesDisponibles,
    IReadOnlyList<TipoDocumentoAspiranteSolicitudMvcOpcion> TiposDocumento);
