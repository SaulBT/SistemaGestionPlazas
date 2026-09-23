namespace SGPla.Data.NewModel.Entities;

public sealed class AreaAcademica
{
    public int Id { get; set; }
    public int Clave { get; set; }
    public string Nombre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class AreaFormacion
{
    public int Id { get; set; }
    public string Clave { get; set; }
    public string Nombre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class AsignacionDocente
{
    public int Id { get; set; }
    public int ProgramacionAcademicaId { get; set; }
    public int DocenteId { get; set; }
    public string Origen { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? SincronizacionPlaneaId { get; set; }
    public int? ActaOfertaId { get; set; }
}

public sealed class Campus
{
    public int Id { get; set; }
    public string Clave { get; set; }
    public string Nombre { get; set; }
    public int RegionId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class Docente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string? NumPersonal { get; set; }
}

public sealed class EntidadAcademica
{
    public int Id { get; set; }
    public string Clave { get; set; }
    public string Nombre { get; set; }
    public string Calle { get; set; }
    public string? NumeroExterior { get; set; }
    public string Colonia { get; set; }
    public string CodigoPostal { get; set; }
    public string Telefono { get; set; }
    public string? Extension { get; set; }
    public int CampusId { get; set; }
    public int AreaAcademicaId { get; set; }
    public int MunicipioId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class ExperienciaEducativa
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string MateriaEe { get; set; }
    public string CursoEe { get; set; }
    public int HorasTeoricas { get; set; }
    public int HorasPracticas { get; set; }
    public int Creditos { get; set; }
    public string? PerfilDocente { get; set; }
    public int AreaFormacionId { get; set; }
    public int PlanEstudiosId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class HorarioProgramacion
{
    public int Id { get; set; }
    public int ProgramacionAcademicaId { get; set; }
    public int SincronizacionPlaneaId { get; set; }
    public byte DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Edificio { get; set; }
    public string? Aula { get; set; }
}

public sealed class Municipio
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class NivelFormacion
{
    public int Id { get; set; }
    public string Clave { get; set; }
    public string Nombre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class PeriodoEscolar
{
    public int Id { get; set; }
    public string Clave { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class PlanEstudios
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public int ProgramaEducativoId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class ProgramaEducativo
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int EntidadAcademicaId { get; set; }
    public int SistemaEducativoId { get; set; }
    public int NivelFormacionId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class ProgramacionAcademica
{
    public int Id { get; set; }
    public string Nrc { get; set; }
    public int PeriodoEscolarId { get; set; }
    public int ExperienciaEducativaId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class Region
{
    public int Id { get; set; }
    public int Clave { get; set; }
    public string Nombre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class SistemaEducativo
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class SincronizacionPlanea
{
    public int Id { get; set; }
    public int PeriodoEscolarId { get; set; }
    public string Estado { get; set; }
    public DateTime IniciadaEn { get; set; }
    public DateTime? FinalizadaEn { get; set; }
    public int RegistrosRecibidos { get; set; }
    public int RegistrosIgnorados { get; set; }
    public int SesionesGeneradas { get; set; }
    public int DuplicadosDescartados { get; set; }
    public int Advertencias { get; set; }
    public string? MensajeError { get; set; }
}

public sealed class ActaAsistencia
{
    public int Id { get; set; }
    public int ActaConsejoTecnicoId { get; set; }
    public int IntegranteConsejoTecnicoId { get; set; }
    public string Nombre { get; set; }
    public string Tratamiento { get; set; }
    public string Cargo { get; set; }
    public bool Asistio { get; set; }
    public bool Firmo { get; set; }
}

public sealed class ActaConsejoTecnico
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public int EntidadAcademicaId { get; set; }
    public string Folio { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Lugar { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? AsuntosGenerales { get; set; }
    public string Estado { get; set; }
    public DateTime? ArchivadaEn { get; set; }
    public int? ArchivadaPorUsuarioId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public int? EliminadaPorUsuarioId { get; set; }
}

public sealed class ActaOferta
{
    public int Id { get; set; }
    public int ActaConsejoTecnicoId { get; set; }
    public int AvisoOfertaId { get; set; }
    public string Resultado { get; set; }
    public string? Observaciones { get; set; }
    public int? SolicitudDesignadaId { get; set; }
    public int? DocenteAsignadoId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public int? EliminadoPorUsuarioId { get; set; }
}

public sealed class Articulo
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public string? Descripcion { get; set; }
}

public sealed class Aspirante
{
    public int Id { get; set; }
}

public sealed class Aviso
{
    public int Id { get; set; }
    public int EntidadAcademicaId { get; set; }
    public int PeriodoEscolarId { get; set; }
    public int SistemaEducativoId { get; set; }
    public int ArticuloId { get; set; }
    public string TipoComunicado { get; set; }
    public int? ModalidadRecepcionId { get; set; }
    public string? Requisitos { get; set; }
    public string? LugarRecepcion { get; set; }
    public string? CorreoContacto { get; set; }
    public string? NombreTitular { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateOnly? FechaPublicacion { get; set; }
    public DateOnly? FechaConsejoTecnico { get; set; }
    public DateOnly? FechaVacantes { get; set; }
    public string? UrlPublicacion { get; set; }
    public string Estado { get; set; }
    public DateTime? CanceladoEn { get; set; }
    public int? CanceladoPorUsuarioId { get; set; }
    public string? MotivoCancelacion { get; set; }
    public DateTime? ArchivadoEn { get; set; }
    public int? ArchivadoPorUsuarioId { get; set; }
}

public sealed class AvisoOferta
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public int OfertaId { get; set; }
    public DateTime IncorporadoEn { get; set; }
    public DateTime? CerradoEn { get; set; }
    public string? CausaCierre { get; set; }
}

public sealed class DocumentoActa
{
    public int Id { get; set; }
    public int ActaConsejoTecnicoId { get; set; }
    public string Tipo { get; set; }
    public string Nombre { get; set; }
    public string Mime { get; set; }
    public long Tamano { get; set; }
    public byte[] ChecksumSha256 { get; set; }
    public string ClaveAlmacenamiento { get; set; }
    public int NumeroVersion { get; set; }
    public bool EsVigente { get; set; }
    public DateTime CargadoEn { get; set; }
    public int CargadoPorUsuarioId { get; set; }
}

public sealed class DocumentoAspirante
{
    public int Id { get; set; }
    public int AspiranteId { get; set; }
    public int TipoDocumentoId { get; set; }
}

public sealed class DocumentoAviso
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public string Tipo { get; set; }
    public string Nombre { get; set; }
    public string Mime { get; set; }
    public long Tamano { get; set; }
    public byte[] ChecksumSha256 { get; set; }
    public string ClaveAlmacenamiento { get; set; }
    public int NumeroVersion { get; set; }
    public bool EsVigente { get; set; }
    public DateTime CargadoEn { get; set; }
    public int CargadoPorUsuarioId { get; set; }
}

public sealed class FormacionAspirante
{
    public int Id { get; set; }
    public int PerfilAspiranteId { get; set; }
    public int GradoAcademicoId { get; set; }
    public string Descripcion { get; set; }
}

public sealed class GradoAcademico
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class HorarioRecepcionRequisito
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}

public sealed class IntegranteConsejoTecnico
{
    public int Id { get; set; }
    public int EntidadAcademicaId { get; set; }
    public string Nombre { get; set; }
    public string Cargo { get; set; }
    public int TratamientoAcademicoId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
}

public sealed class ModalidadRecepcion
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public bool RequiereLugar { get; set; }
}

public sealed class Oferta
{
    public int Id { get; set; }
    public int ProgramacionAcademicaId { get; set; }
    public string ClavePlaza { get; set; }
    public int TipoPlazaId { get; set; }
    public int TipoContratacionId { get; set; }
    public string PerfilSolicitado { get; set; }
    public string? Justificacion { get; set; }
    public string Estado { get; set; }
    public DateTime? CerradaEn { get; set; }
}

public sealed class PerfilAspirante
{
    public int Id { get; set; }
    public int AspiranteId { get; set; }
    public int NumeroVersion { get; set; }
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string? PuestoActual { get; set; }
    public string DescripcionPerfil { get; set; }
    public bool EsVigente { get; set; }
    public DateTime CreadoEn { get; set; }
    public int CreadoPorUsuarioId { get; set; }
}

public sealed class RevisionActa
{
    public int Id { get; set; }
    public int ActaConsejoTecnicoId { get; set; }
    public int NumeroRevision { get; set; }
    public int DocumentoOriginalId { get; set; }
    public int EnviadoPorUsuarioId { get; set; }
    public DateTime EnviadoEn { get; set; }
    public int? ResueltoPorUsuarioId { get; set; }
    public DateTime? ResueltoEn { get; set; }
    public string? Resultado { get; set; }
    public string? Comentarios { get; set; }
}

public sealed class RevisionAviso
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public int NumeroRevision { get; set; }
    public int DocumentoOriginalId { get; set; }
    public int EnviadoPorUsuarioId { get; set; }
    public DateTime EnviadoEn { get; set; }
    public int? ResueltoPorUsuarioId { get; set; }
    public DateTime? ResueltoEn { get; set; }
    public string? Resultado { get; set; }
    public string? Comentarios { get; set; }
}

public sealed class Solicitud
{
    public int Id { get; set; }
    public int AvisoOfertaId { get; set; }
    public int AspiranteId { get; set; }
    public int PerfilAspiranteId { get; set; }
    public DateTime RegistradaEn { get; set; }
    public string Estado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? AdmitidaEn { get; set; }
    public DateTime? NoAdmitidaEn { get; set; }
    public DateTime? RetiradaEn { get; set; }
    public int? AdmitidaPorUsuarioId { get; set; }
    public int? NoAdmitidaPorUsuarioId { get; set; }
    public string? MotivoNoAdmision { get; set; }
    public string? MotivoRetiro { get; set; }
}

public sealed class SolicitudDocumento
{
    public int SolicitudId { get; set; }
    public int VersionDocumentoAspiranteId { get; set; }
}

public sealed class TipoContratacion
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class TipoDocumentoAspirante
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class TipoPlaza
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class TratamientoAcademico
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int GradoAcademicoId { get; set; }
}

public sealed class VersionDocumentoAspirante
{
    public int Id { get; set; }
    public int DocumentoAspiranteId { get; set; }
    public string Nombre { get; set; }
    public string Mime { get; set; }
    public long Tamano { get; set; }
    public byte[] ChecksumSha256 { get; set; }
    public string ClaveAlmacenamiento { get; set; }
    public int NumeroVersion { get; set; }
    public bool EsVigente { get; set; }
    public DateTime CargadoEn { get; set; }
    public int CargadoPorUsuarioId { get; set; }
}

public sealed class VotacionSolicitud
{
    public int ActaOfertaId { get; set; }
    public int SolicitudId { get; set; }
    public int Votos { get; set; }
}

public sealed class CredencialSuperusuario
{
    public int UsuarioId { get; set; }
    public string Contrasena { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class Rol
{
    public byte Id { get; set; }
    public string Nombre { get; set; }
}

public sealed class Usuario
{
    public int Id { get; set; }
    public string Correo { get; set; }
    public string Nombre { get; set; }
    public byte RolId { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

public sealed class UsuarioDgaa
{
    public int UsuarioId { get; set; }
    public int AreaAcademicaId { get; set; }
}

public sealed class UsuarioEntidadAcademica
{
    public int UsuarioId { get; set; }
    public int EntidadAcademicaId { get; set; }
}

