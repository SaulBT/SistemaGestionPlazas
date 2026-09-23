using Microsoft.EntityFrameworkCore;
using SGPla.Data.NewModel.Entities;

namespace SGPla.Data.NewModel;

public sealed class SgplaDbContext : DbContext
{
    public SgplaDbContext(DbContextOptions<SgplaDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<AreaAcademica> AreaAcademicas => Set<AreaAcademica>();
    public DbSet<AreaFormacion> AreaFormaciones => Set<AreaFormacion>();
    public DbSet<AsignacionDocente> AsignacionDocentes => Set<AsignacionDocente>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<EntidadAcademica> EntidadAcademicas => Set<EntidadAcademica>();
    public DbSet<ExperienciaEducativa> ExperienciasEducativas => Set<ExperienciaEducativa>();
    public DbSet<HorarioProgramacion> HorariosProgramacion => Set<HorarioProgramacion>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<NivelFormacion> NivelesFormacion => Set<NivelFormacion>();
    public DbSet<PeriodoEscolar> PeriodosEscolares => Set<PeriodoEscolar>();
    public DbSet<PlanEstudios> PlanesEstudios => Set<PlanEstudios>();
    public DbSet<ProgramaEducativo> ProgramasEducativos => Set<ProgramaEducativo>();
    public DbSet<ProgramacionAcademica> ProgramacionAcademicas => Set<ProgramacionAcademica>();
    public DbSet<Region> Regiones => Set<Region>();
    public DbSet<SistemaEducativo> SistemasEducativos => Set<SistemaEducativo>();
    public DbSet<SincronizacionPlanea> SincronizacionesPlanea => Set<SincronizacionPlanea>();
    public DbSet<ActaAsistencia> ActaAsistencias => Set<ActaAsistencia>();
    public DbSet<ActaConsejoTecnico> ActaConsejoTecnicos => Set<ActaConsejoTecnico>();
    public DbSet<ActaOferta> ActaOfertas => Set<ActaOferta>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Aspirante> Aspirantes => Set<Aspirante>();
    public DbSet<Aviso> Avisos => Set<Aviso>();
    public DbSet<AvisoOferta> AvisoOfertas => Set<AvisoOferta>();
    public DbSet<DocumentoActa> DocumentoActas => Set<DocumentoActa>();
    public DbSet<DocumentoAspirante> DocumentoAspirantes => Set<DocumentoAspirante>();
    public DbSet<DocumentoAviso> DocumentoAvisos => Set<DocumentoAviso>();
    public DbSet<FormacionAspirante> FormacionAspirantes => Set<FormacionAspirante>();
    public DbSet<GradoAcademico> GradoAcademicos => Set<GradoAcademico>();
    public DbSet<HorarioRecepcionRequisito> HorarioRecepcionRequisitos => Set<HorarioRecepcionRequisito>();
    public DbSet<IntegranteConsejoTecnico> IntegranteConsejoTecnicos => Set<IntegranteConsejoTecnico>();
    public DbSet<ModalidadRecepcion> ModalidadesRecepcion => Set<ModalidadRecepcion>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<PerfilAspirante> PerfilAspirantes => Set<PerfilAspirante>();
    public DbSet<RevisionActa> RevisionActas => Set<RevisionActa>();
    public DbSet<RevisionAviso> RevisionAvisos => Set<RevisionAviso>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<SolicitudDocumento> SolicitudDocumentos => Set<SolicitudDocumento>();
    public DbSet<TipoContratacion> TiposContratacion => Set<TipoContratacion>();
    public DbSet<TipoDocumentoAspirante> TipoDocumentoAspirantes => Set<TipoDocumentoAspirante>();
    public DbSet<TipoPlaza> TipoPlazas => Set<TipoPlaza>();
    public DbSet<TratamientoAcademico> TratamientosAcademicos => Set<TratamientoAcademico>();
    public DbSet<VersionDocumentoAspirante> VersionDocumentoAspirantes => Set<VersionDocumentoAspirante>();
    public DbSet<VotacionSolicitud> VotacionesSolicitud => Set<VotacionSolicitud>();
    public DbSet<CredencialSuperusuario> CredencialSuperusuarios => Set<CredencialSuperusuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioDgaa> UsuariosDgaa => Set<UsuarioDgaa>();
    public DbSet<UsuarioEntidadAcademica> UsuariosEntidadAcademica => Set<UsuarioEntidadAcademica>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AreaAcademica>(entity =>
        {
            entity.ToTable("area_academica", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<AreaFormacion>(entity =>
        {
            entity.ToTable("area_formacion", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<AsignacionDocente>(entity =>
        {
            entity.ToTable("asignacion_docente", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Origen).HasMaxLength(10);
        });
        modelBuilder.Entity<Campus>(entity =>
        {
            entity.ToTable("campus", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<Docente>(entity =>
        {
            entity.ToTable("docente", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.NumPersonal).HasMaxLength(50);
        });
        modelBuilder.Entity<EntidadAcademica>(entity =>
        {
            entity.ToTable("entidad_academica", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Calle).HasMaxLength(200);
            entity.Property(e => e.NumeroExterior).HasMaxLength(20);
            entity.Property(e => e.Colonia).HasMaxLength(150);
            entity.Property(e => e.CodigoPostal).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.Telefono).HasMaxLength(10).IsFixedLength();
            entity.Property(e => e.Extension).HasMaxLength(10);
        });
        modelBuilder.Entity<ExperienciaEducativa>(entity =>
        {
            entity.ToTable("experiencia_educativa", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.MateriaEe).HasMaxLength(50);
            entity.Property(e => e.CursoEe).HasMaxLength(50);
        });
        modelBuilder.Entity<HorarioProgramacion>(entity =>
        {
            entity.ToTable("horario_programacion", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Edificio).HasMaxLength(100);
            entity.Property(e => e.Aula).HasMaxLength(100);
        });
        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.ToTable("municipio", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });
        modelBuilder.Entity<NivelFormacion>(entity =>
        {
            entity.ToTable("nivel_formacion", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<PeriodoEscolar>(entity =>
        {
            entity.ToTable("periodo_escolar", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasMaxLength(6).IsFixedLength();
        });
        modelBuilder.Entity<PlanEstudios>(entity =>
        {
            entity.ToTable("plan_estudios", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Codigo).HasMaxLength(50);
        });
        modelBuilder.Entity<ProgramaEducativo>(entity =>
        {
            entity.ToTable("programa_educativo", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<ProgramacionAcademica>(entity =>
        {
            entity.ToTable("programacion_academica", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nrc).HasMaxLength(20);
        });
        modelBuilder.Entity<Region>(entity =>
        {
            entity.ToTable("region", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<SistemaEducativo>(entity =>
        {
            entity.ToTable("sistema_educativo", "academico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<SincronizacionPlanea>(entity =>
        {
            entity.ToTable("sincronizacion_planea", "integracion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.MensajeError).HasMaxLength(4000);
        });
        modelBuilder.Entity<ActaAsistencia>(entity =>
        {
            entity.ToTable("acta_asistencia", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Tratamiento).HasMaxLength(30);
            entity.Property(e => e.Cargo).HasMaxLength(200);
        });
        modelBuilder.Entity<ActaConsejoTecnico>(entity =>
        {
            entity.ToTable("acta_consejo_tecnico", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Folio).HasMaxLength(100);
            entity.Property(e => e.Lugar).HasMaxLength(300);
            entity.Property(e => e.Estado).HasMaxLength(30);
        });
        modelBuilder.Entity<ActaOferta>(entity =>
        {
            entity.ToTable("acta_oferta", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Resultado).HasMaxLength(20);
        });
        modelBuilder.Entity<Articulo>(entity =>
        {
            entity.ToTable("articulo", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Numero).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
        });
        modelBuilder.Entity<Aspirante>(entity =>
        {
            entity.ToTable("aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.ToTable("aviso", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TipoComunicado).HasMaxLength(15);
            entity.Property(e => e.LugarRecepcion).HasMaxLength(500);
            entity.Property(e => e.CorreoContacto).HasMaxLength(254);
            entity.Property(e => e.NombreTitular).HasMaxLength(200);
            entity.Property(e => e.UrlPublicacion).HasMaxLength(2048);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.MotivoCancelacion).HasMaxLength(1000);
        });
        modelBuilder.Entity<AvisoOferta>(entity =>
        {
            entity.ToTable("aviso_oferta", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CausaCierre).HasMaxLength(20);
        });
        modelBuilder.Entity<DocumentoActa>(entity =>
        {
            entity.ToTable("documento_acta", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Tipo).HasMaxLength(10);
            entity.Property(e => e.Nombre).HasMaxLength(260);
            entity.Property(e => e.Mime).HasMaxLength(255);
            entity.Property(e => e.ChecksumSha256).HasMaxLength(32);
            entity.Property(e => e.ClaveAlmacenamiento).HasMaxLength(500);
        });
        modelBuilder.Entity<DocumentoAspirante>(entity =>
        {
            entity.ToTable("documento_aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<DocumentoAviso>(entity =>
        {
            entity.ToTable("documento_aviso", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Tipo).HasMaxLength(10);
            entity.Property(e => e.Nombre).HasMaxLength(260);
            entity.Property(e => e.Mime).HasMaxLength(255);
            entity.Property(e => e.ChecksumSha256).HasMaxLength(32);
            entity.Property(e => e.ClaveAlmacenamiento).HasMaxLength(500);
        });
        modelBuilder.Entity<FormacionAspirante>(entity =>
        {
            entity.ToTable("formacion_aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
        });
        modelBuilder.Entity<GradoAcademico>(entity =>
        {
            entity.ToTable("grado_academico", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });
        modelBuilder.Entity<HorarioRecepcionRequisito>(entity =>
        {
            entity.ToTable("horario_recepcion_requisito", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<IntegranteConsejoTecnico>(entity =>
        {
            entity.ToTable("integrante_consejo_tecnico", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Cargo).HasMaxLength(200);
        });
        modelBuilder.Entity<ModalidadRecepcion>(entity =>
        {
            entity.ToTable("modalidad_recepcion", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });
        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.ToTable("oferta", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ClavePlaza).HasMaxLength(100);
            entity.Property(e => e.Justificacion).HasMaxLength(1000);
            entity.Property(e => e.Estado).HasMaxLength(20);
        });
        modelBuilder.Entity<PerfilAspirante>(entity =>
        {
            entity.ToTable("perfil_aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Correo).HasMaxLength(254);
            entity.Property(e => e.PuestoActual).HasMaxLength(200);
        });
        modelBuilder.Entity<RevisionActa>(entity =>
        {
            entity.ToTable("revision_acta", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Resultado).HasMaxLength(10);
        });
        modelBuilder.Entity<RevisionAviso>(entity =>
        {
            entity.ToTable("revision_aviso", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Resultado).HasMaxLength(10);
        });
        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.ToTable("solicitud", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.MotivoNoAdmision).HasMaxLength(1000);
            entity.Property(e => e.MotivoRetiro).HasMaxLength(1000);
        });
        modelBuilder.Entity<SolicitudDocumento>(entity =>
        {
            entity.ToTable("solicitud_documento", "plazas");
            entity.HasKey(e => new { e.SolicitudId, e.VersionDocumentoAspiranteId });
        });
        modelBuilder.Entity<TipoContratacion>(entity =>
        {
            entity.ToTable("tipo_contratacion", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });
        modelBuilder.Entity<TipoDocumentoAspirante>(entity =>
        {
            entity.ToTable("tipo_documento_aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });
        modelBuilder.Entity<TipoPlaza>(entity =>
        {
            entity.ToTable("tipo_plaza", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });
        modelBuilder.Entity<TratamientoAcademico>(entity =>
        {
            entity.ToTable("tratamiento_academico", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(30);
        });
        modelBuilder.Entity<VersionDocumentoAspirante>(entity =>
        {
            entity.ToTable("version_documento_aspirante", "plazas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(260);
            entity.Property(e => e.Mime).HasMaxLength(255);
            entity.Property(e => e.ChecksumSha256).HasMaxLength(32);
            entity.Property(e => e.ClaveAlmacenamiento).HasMaxLength(500);
        });
        modelBuilder.Entity<VotacionSolicitud>(entity =>
        {
            entity.ToTable("votacion_solicitud", "plazas");
            entity.HasKey(e => new { e.ActaOfertaId, e.SolicitudId });
        });
        modelBuilder.Entity<CredencialSuperusuario>(entity =>
        {
            entity.ToTable("credencial_superusuario", "usuarios");
            entity.HasKey(e => e.UsuarioId);
            entity.Property(e => e.Contrasena).HasMaxLength(500);
        });
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("rol", "usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario", "usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Correo).HasMaxLength(254);
            entity.Property(e => e.Nombre).HasMaxLength(200);
        });
        modelBuilder.Entity<UsuarioDgaa>(entity =>
        {
            entity.ToTable("usuario_dgaa", "usuarios");
            entity.HasKey(e => e.UsuarioId);
        });
        modelBuilder.Entity<UsuarioEntidadAcademica>(entity =>
        {
            entity.ToTable("usuario_entidad_academica", "usuarios");
            entity.HasKey(e => e.UsuarioId);
        });
    }
}
