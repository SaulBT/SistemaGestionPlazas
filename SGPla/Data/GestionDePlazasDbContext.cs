using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SGPla.Models;

namespace SGPla.Data;

public partial class GestionDePlazasDbContext : DbContext
{
    public GestionDePlazasDbContext(DbContextOptions<GestionDePlazasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Acta> Acta { get; set; }

    public virtual DbSet<AreaAcademica> AreaAcademica { get; set; }

    public virtual DbSet<Articulo> Articulo { get; set; }

    public virtual DbSet<Asistencia> Asistencia { get; set; }

    public virtual DbSet<Aviso> Aviso { get; set; }

    public virtual DbSet<CoordinadorDGAA> CoordinadorDGAA { get; set; }

    public virtual DbSet<CoordinadorEA> CoordinadorEA { get; set; }

    public virtual DbSet<Dictamen> Dictamen { get; set; }

    public virtual DbSet<Docente> Docente { get; set; }

    public virtual DbSet<EntidadAcademica> EntidadAcademica { get; set; }

    public virtual DbSet<ExperienciaEducativa> ExperienciaEducativa { get; set; }

    public virtual DbSet<Grado> Grado { get; set; }

    public virtual DbSet<Horario> Horario { get; set; }

    public virtual DbSet<IntegranteCT> IntegranteCT { get; set; }

    public virtual DbSet<Log> Log { get; set; }

    public virtual DbSet<Notificacion> Notificacion { get; set; }

    public virtual DbSet<Oferta> Oferta { get; set; }

    public virtual DbSet<OfertaAviso> OfertaAviso { get; set; }

    public virtual DbSet<Periodo> Periodo { get; set; }

    public virtual DbSet<PlanEstudios> PlanEstudios { get; set; }

    public virtual DbSet<ProgramaEducativo> ProgramaEducativo { get; set; }

    public virtual DbSet<Solicitud> Solicitud { get; set; }

    public virtual DbSet<SuperUsuario> SuperUsuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Acta>(entity =>
        {
            entity.HasKey(e => e.idActa);

            entity.HasIndex(e => e.idAviso, "IX_Acta_idAviso");

            entity.Property(e => e.asuntosGenerales).IsUnicode(false);
            entity.Property(e => e.folio)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.lugar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idAvisoNavigation).WithMany(p => p.Acta)
                .HasForeignKey(d => d.idAviso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Acta_Aviso");
        });

        modelBuilder.Entity<AreaAcademica>(entity =>
        {
            entity.HasKey(e => e.idAreaAcademica);

            entity.Property(e => e.calleNumero)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.colonia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.conmutador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.cp)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.extension)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.fax)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.municipio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.telefono)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Articulo>(entity =>
        {
            entity.HasKey(e => e.IdArticulo);

            entity.HasIndex(e => e.Numero, "UQ_Articulo_numero").IsUnique();

            entity.Property(e => e.Descripcion).IsUnicode(false);
            entity.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.idAsistencia);

            entity.HasIndex(e => e.idIntegranteCT, "IX_Asistencia_idIntegranteCT");

            entity.HasIndex(e => new { e.idActa, e.idIntegranteCT }, "UQ_Asistencia_Acta_IntegranteCT").IsUnique();

            entity.HasOne(d => d.idActaNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.idActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Acta");

            entity.HasOne(d => d.idIntegranteCTNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.idIntegranteCT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_IntegranteCT");
        });

        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.HasKey(e => e.idAviso);

            entity.HasIndex(e => e.idArticulo, "IX_Aviso_idArticulo");

            entity.HasIndex(e => e.idEntidadAcademica, "IX_Aviso_idEntidadAcademica");

            entity.HasIndex(e => e.idPeriodo, "IX_Aviso_idPeriodo");

            entity.Property(e => e.correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.folio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.lugar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.modalidad)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.requisitos).IsUnicode(false);
            entity.Property(e => e.rutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idArticuloNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.idArticulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_Articulo");

            entity.HasOne(d => d.idEntidadAcademicaNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.idEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_EntidadAcademica");

            entity.HasOne(d => d.idPeriodoNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.idPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_Periodo");
        });

        modelBuilder.Entity<CoordinadorDGAA>(entity =>
        {
            entity.HasKey(e => e.idCoordinadorDGAA);

            entity.HasIndex(e => e.idAreaAcademica, "IX_CoordinadorDGAA_idAreaAcademica");

            entity.Property(e => e.cargo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idAreaAcademicaNavigation).WithMany(p => p.CoordinadorDGAA)
                .HasForeignKey(d => d.idAreaAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CoordinadorDGAA_AreaAcademica");
        });

        modelBuilder.Entity<CoordinadorEA>(entity =>
        {
            entity.HasKey(e => e.idCoordinadorEA);

            entity.HasIndex(e => e.idEntidadAcademica, "IX_CoordinadorEA_idEntidadAcademica");

            entity.Property(e => e.cargo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idEntidadAcademicaNavigation).WithMany(p => p.CoordinadorEA)
                .HasForeignKey(d => d.idEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CoordinadorEA_EntidadAcademica");
        });

        modelBuilder.Entity<Dictamen>(entity =>
        {
            entity.HasKey(e => e.idDictamen);

            entity.HasIndex(e => e.idDocente, "IX_Dictamen_idDocente");

            entity.HasIndex(e => new { e.idActa, e.idDocente }, "UQ_Dictamen_Acta_Docente").IsUnique();

            entity.Property(e => e.rutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idActaNavigation).WithMany(p => p.Dictamen)
                .HasForeignKey(d => d.idActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dictamen_Acta");

            entity.HasOne(d => d.idDocenteNavigation).WithMany(p => p.Dictamen)
                .HasForeignKey(d => d.idDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dictamen_Docente");
        });

        modelBuilder.Entity<Docente>(entity =>
        {
            entity.HasKey(e => e.idDocente);

            entity.HasIndex(e => e.numeroPersonal, "UQ_Docente_numeroPersonal").IsUnique();

            entity.Property(e => e.descripcionPerfil).IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.numeroPersonal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.puesto)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentosGenerales)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EntidadAcademica>(entity =>
        {
            entity.HasKey(e => e.idEntidadAcademica);

            entity.HasIndex(e => e.idAreaAcademica, "IX_EntidadAcademica_idAreaAcademica");

            entity.Property(e => e.calleNumero)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.campus)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.colonia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.conmutador)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.cp)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.extension)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.fax)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.municipio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.region)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.telefono)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.idAreaAcademicaNavigation).WithMany(p => p.EntidadAcademica)
                .HasForeignKey(d => d.idAreaAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EntidadAcademica_AreaAcademica");
        });

        modelBuilder.Entity<ExperienciaEducativa>(entity =>
        {
            entity.HasKey(e => e.idExperienciaEducativa);

            entity.HasIndex(e => e.idPlanEstudios, "IX_ExperienciaEducativa_idPlanEstudios");

            entity.Property(e => e.codigo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.perfilDocente).IsUnicode(false);

            entity.HasOne(d => d.idPlanEstudiosNavigation).WithMany(p => p.ExperienciaEducativa)
                .HasForeignKey(d => d.idPlanEstudios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperienciaEducativa_PlanEstudios");
        });

        modelBuilder.Entity<Grado>(entity =>
        {
            entity.HasKey(e => e.idGrado);

            entity.HasIndex(e => e.idDocente, "IX_Grado_idDocente");

            entity.HasIndex(e => e.idDocente, "UX_Grado_docente_ultimo")
                .IsUnique()
                .HasFilter("([ultimo]=(1))");

            entity.Property(e => e.grado)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.titulo)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.idDocenteNavigation).WithOne(p => p.Grado)
                .HasForeignKey<Grado>(d => d.idDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grado_Docente");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.idHorario);

            entity.HasIndex(e => e.idAviso, "IX_Horario_idAviso");

            entity.HasIndex(e => e.idOferta, "IX_Horario_idOferta");

            entity.Property(e => e.dia)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.salon)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.idAvisoNavigation).WithMany(p => p.Horario)
                .HasForeignKey(d => d.idAviso)
                .HasConstraintName("FK_Horario_Aviso");

            entity.HasOne(d => d.idOfertaNavigation).WithMany(p => p.Horario)
                .HasForeignKey(d => d.idOferta)
                .HasConstraintName("FK_Horario_Oferta");
        });

        modelBuilder.Entity<IntegranteCT>(entity =>
        {
            entity.HasKey(e => e.idIntegranteCT);

            entity.HasIndex(e => e.idEntidadAcademica, "IX_IntegranteCT_idEntidadAcademica");

            entity.Property(e => e.cargo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idEntidadAcademicaNavigation).WithMany(p => p.IntegranteCT)
                .HasForeignKey(d => d.idEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IntegranteCT_EntidadAcademica");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.idLog);

            entity.HasIndex(e => e.idOferta, "IX_Log_idOferta");

            entity.Property(e => e.mensaje).IsUnicode(false);

            entity.HasOne(d => d.idOfertaNavigation).WithMany(p => p.Log)
                .HasForeignKey(d => d.idOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_Oferta");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.idNotificacion);

            entity.HasIndex(e => e.idActa, "IX_Notificacion_idActa");

            entity.Property(e => e.rutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idActaNavigation).WithMany(p => p.Notificacion)
                .HasForeignKey(d => d.idActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Acta");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.idOferta);

            entity.HasIndex(e => e.idArticulo, "IX_Oferta_idArticulo");

            entity.HasIndex(e => e.idDocente, "IX_Oferta_idDocente");

            entity.HasIndex(e => e.idExperienciaEducativa, "IX_Oferta_idExperienciaEducativa");

            entity.HasIndex(e => e.idPeriodo, "IX_Oferta_idPeriodo");

            entity.Property(e => e.estadoSolicitudApertura)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.justificacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.justificacionApertura)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.nrc)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.plaza)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.rutaArchivoSolicitudApertura)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.tipoContratacion)
                .HasMaxLength(3)
                .IsUnicode(false);

            entity.HasOne(d => d.idArticuloNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.idArticulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Articulo");

            entity.HasOne(d => d.idDocenteNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.idDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Docente");

            entity.HasOne(d => d.idExperienciaEducativaNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.idExperienciaEducativa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_ExperienciaEducativa");

            entity.HasOne(d => d.idPeriodoNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.idPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Periodo");
        });

        modelBuilder.Entity<OfertaAviso>(entity =>
        {
            entity.HasKey(e => e.idOfertaAviso);

            entity.HasIndex(e => e.idAviso, "IX_OfertaAviso_idAviso");

            entity.HasIndex(e => new { e.idOferta, e.idAviso }, "UQ_OfertaAviso_Oferta_Aviso").IsUnique();

            entity.HasOne(d => d.idAvisoNavigation).WithMany(p => p.OfertaAviso)
                .HasForeignKey(d => d.idAviso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfertaAviso_Aviso");

            entity.HasOne(d => d.idOfertaNavigation).WithMany(p => p.OfertaAviso)
                .HasForeignKey(d => d.idOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfertaAviso_Oferta");
        });

        modelBuilder.Entity<Periodo>(entity =>
        {
            entity.HasKey(e => e.idPeriodo);

            entity.HasIndex(e => new { e.nombre, e.anioInicio }, "UQ_Periodo_nombre_anioInicio").IsUnique();

            entity.Property(e => e.idPeriodo).ValueGeneratedNever();
            entity.Property(e => e.nombre)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlanEstudios>(entity =>
        {
            entity.HasKey(e => e.idPlanEstudios);

            entity.HasIndex(e => e.idProgramaEducativo, "IX_PlanEstudios_idProgramaEducativo");

            entity.Property(e => e.modalidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.idProgramaEducativoNavigation).WithMany(p => p.PlanEstudios)
                .HasForeignKey(d => d.idProgramaEducativo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanEstudios_ProgramaEducativo");
        });

        modelBuilder.Entity<ProgramaEducativo>(entity =>
        {
            entity.HasKey(e => e.idProgramaEducativo);

            entity.HasIndex(e => e.idEntidadAcademica, "IX_ProgramaEducativo_idEntidadAcademica");

            entity.Property(e => e.nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.idEntidadAcademicaNavigation).WithMany(p => p.ProgramaEducativo)
                .HasForeignKey(d => d.idEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramaEducativo_EntidadAcademica");
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.idSolicitud);

            entity.HasIndex(e => e.idDocente, "IX_Solicitud_idDocente");

            entity.HasIndex(e => e.idOferta, "IX_Solicitud_idOferta");

            entity.Property(e => e.justificacion).IsUnicode(false);
            entity.Property(e => e.modalidad)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.rutaDocumentosSolicitud)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.idDocenteNavigation).WithMany(p => p.Solicitud)
                .HasForeignKey(d => d.idDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Docente");

            entity.HasOne(d => d.idOfertaNavigation).WithMany(p => p.Solicitud)
                .HasForeignKey(d => d.idOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Oferta");
        });

        modelBuilder.Entity<SuperUsuario>(entity =>
        {
            entity.HasKey(e => e.idSuperUsuario);

            entity.Property(e => e.correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.nombre)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
