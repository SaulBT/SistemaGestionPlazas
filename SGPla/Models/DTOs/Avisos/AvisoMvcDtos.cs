namespace SGPla.Models.DTOs.Avisos;

public sealed record AvisoMvcOpcion(int Id, string Texto);

public sealed record AvisoMvcItem(int Id, string EntidadAcademica, string PeriodoEscolar,
    string SistemaEducativo, string Articulo, string TipoComunicado, string Estado,
    DateTime CreadoEn, int Ofertas);

public sealed record AvisoMvcFiltro(int Pagina = 1, int TamanoPagina = 20,
    int? PeriodoEscolarId = null, int? EntidadAcademicaId = null, string? Estado = null);

public sealed record AvisoMvcPagina(IReadOnlyList<AvisoMvcItem> Items, int Total,
    int Pagina, int TamanoPagina, IReadOnlyList<AvisoMvcOpcion> Periodos,
    IReadOnlyList<AvisoMvcOpcion> Entidades);

public sealed record AvisoMvcNuevoDatos(IReadOnlyList<AvisoMvcOpcion> Periodos,
    IReadOnlyList<AvisoMvcOpcion> SistemasEducativos, IReadOnlyList<AvisoMvcOpcion> Articulos,
    IReadOnlyList<AvisoMvcOpcion> OfertasDisponibles,
    IReadOnlyList<AvisoMvcOpcion> ModalidadesRecepcion);

public sealed record AvisoMvcHorario(DateOnly Fecha, TimeOnly HoraInicio, TimeOnly HoraFin);

public sealed record ConfigurarYEnviarAvisoMvcDatos(int ModalidadRecepcionId, string Requisitos,
    string? LugarRecepcion, string CorreoContacto, string NombreTitular, DateOnly FechaConsejoTecnico,
    DateOnly FechaVacantes, IReadOnlyList<AvisoMvcHorario> Horarios);

public sealed record PublicarAvisoMvcDatos(DateOnly FechaPublicacion, string UrlPublicacion);

public sealed record CancelarAvisoMvcDatos(string Motivo);

public sealed record AvisoMvcOfertaItem(int AvisoOfertaId, int OfertaId, string ClavePlaza,
    string Nrc, string ProgramaEducativo, string ExperienciaEducativa, string EstadoOferta);

public sealed record AvisoMvcDocumentoItem(int Id, string Tipo, string Nombre, string Mime,
    long Tamano, int NumeroVersion, bool EsVigente, DateTime CargadoEn);

public sealed record DocumentoAvisoDescargaMvc(int AvisoId, int DocumentoId, string Nombre,
    string Mime, string ClaveAlmacenamiento, byte[] ChecksumSha256);

public sealed record DescargaDocumentoAvisoMvc(Stream Contenido, string Nombre, string Mime);

public sealed record AvisoMvcDetalle(int Id, int PeriodoEscolarId, int SistemaEducativoId,
    string EntidadAcademica, string PeriodoEscolar, string SistemaEducativo, string Articulo,
    string TipoComunicado, string Estado, DateTime CreadoEn, DateOnly? FechaPublicacion,
    string? UrlPublicacion, DateTime? CanceladoEn, string? MotivoCancelacion, DateTime? ArchivadoEn,
    IReadOnlyList<AvisoMvcOfertaItem> Ofertas,
    IReadOnlyList<AvisoMvcDocumentoItem> Documentos, int? ModalidadRecepcionId, string? ModalidadRecepcion,
    string? Requisitos, string? LugarRecepcion, string? CorreoContacto, string? NombreTitular,
    DateOnly? FechaConsejoTecnico, DateOnly? FechaVacantes,
    IReadOnlyList<AvisoMvcHorario> Horarios, IReadOnlyList<AvisoMvcRevisionItem> Revisiones);

public sealed record AvisoMvcRevisionItem(int Numero, DateTime EnviadoEn, string? Resultado,
    DateTime? ResueltoEn, string? Comentarios);

public sealed record CrearAvisoMvcDatos(int PeriodoEscolarId, int SistemaEducativoId,
    int ArticuloId, string TipoComunicado, IReadOnlyList<int> OfertaIds);
