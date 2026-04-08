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

    public virtual DbSet<CoordinadorDgaa> CoordinadorDgaa { get; set; }

    public virtual DbSet<CoordinadorEa> CoordinadorEa { get; set; }

    public virtual DbSet<Dictamen> Dictamen { get; set; }

    public virtual DbSet<Docente> Docente { get; set; }

    public virtual DbSet<EntidadAcademica> EntidadAcademica { get; set; }

    public virtual DbSet<ExperienciaEducativa> ExperienciaEducativa { get; set; }

    public virtual DbSet<Grado> Grado { get; set; }

    public virtual DbSet<Horario> Horario { get; set; }

    public virtual DbSet<IntegranteCt> IntegranteCt { get; set; }

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
            entity.HasKey(e => e.IdActa);

            entity.HasIndex(e => e.IdAviso, "IX_Acta_idAviso");

            entity.Property(e => e.IdActa).HasColumnName("idActa");
            entity.Property(e => e.Archivado).HasColumnName("archivado");
            entity.Property(e => e.AsuntosGenerales)
                .IsUnicode(false)
                .HasColumnName("asuntosGenerales");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Folio)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("folio");
            entity.Property(e => e.HoraConclusion).HasColumnName("horaConclusion");
            entity.Property(e => e.HoraInicio).HasColumnName("horaInicio");
            entity.Property(e => e.IdAviso).HasColumnName("idAviso");
            entity.Property(e => e.Lugar)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lugar");
            entity.Property(e => e.RutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoFirmado");
            entity.Property(e => e.RutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoOriginal");

            entity.HasOne(d => d.IdAvisoNavigation).WithMany(p => p.Acta)
                .HasForeignKey(d => d.IdAviso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Acta_Aviso");
        });

        modelBuilder.Entity<AreaAcademica>(entity =>
        {
            entity.HasKey(e => e.IdAreaAcademica);

            entity.Property(e => e.IdAreaAcademica).HasColumnName("idAreaAcademica");
            entity.Property(e => e.CalleNumero)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("calleNumero");
            entity.Property(e => e.Colonia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("colonia");
            entity.Property(e => e.Conmutador)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("conmutador");
            entity.Property(e => e.Cp)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("cp");
            entity.Property(e => e.Extension)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("extension");
            entity.Property(e => e.Fax)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("fax");
            entity.Property(e => e.Municipio)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("municipio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Articulo>(entity =>
        {
            entity.HasKey(e => e.IdArticulo);

            entity.HasIndex(e => e.Numero, "UQ_Articulo_numero").IsUnique();

            entity.Property(e => e.IdArticulo).HasColumnName("idArticulo");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("numero");
        });

        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.IdAsistencia);

            entity.HasIndex(e => e.IdIntegranteCt, "IX_Asistencia_idIntegranteCT");

            entity.HasIndex(e => new { e.IdActa, e.IdIntegranteCt }, "UQ_Asistencia_Acta_IntegranteCT").IsUnique();

            entity.Property(e => e.IdAsistencia).HasColumnName("idAsistencia");
            entity.Property(e => e.Asistio).HasColumnName("asistio");
            entity.Property(e => e.IdActa).HasColumnName("idActa");
            entity.Property(e => e.IdIntegranteCt).HasColumnName("idIntegranteCT");

            entity.HasOne(d => d.IdActaNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Acta");

            entity.HasOne(d => d.IdIntegranteCtNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdIntegranteCt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_IntegranteCT");
        });

        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.HasKey(e => e.IdAviso);

            entity.HasIndex(e => e.IdArticulo, "IX_Aviso_idArticulo");

            entity.HasIndex(e => e.IdEntidadAcademica, "IX_Aviso_idEntidadAcademica");

            entity.HasIndex(e => e.IdPeriodo, "IX_Aviso_idPeriodo");

            entity.Property(e => e.IdAviso).HasColumnName("idAviso");
            entity.Property(e => e.Archivado).HasColumnName("archivado");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.FechaCreacion).HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.Folio)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("folio");
            entity.Property(e => e.IdArticulo).HasColumnName("idArticulo");
            entity.Property(e => e.IdEntidadAcademica).HasColumnName("idEntidadAcademica");
            entity.Property(e => e.IdPeriodo).HasColumnName("idPeriodo");
            entity.Property(e => e.Lugar)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lugar");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("modalidad");
            entity.Property(e => e.Requisitos)
                .IsUnicode(false)
                .HasColumnName("requisitos");
            entity.Property(e => e.RutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoFirmado");
            entity.Property(e => e.RutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoOriginal");

            entity.HasOne(d => d.IdArticuloNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.IdArticulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_Articulo");

            entity.HasOne(d => d.IdEntidadAcademicaNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.IdEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_EntidadAcademica");

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.Aviso)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aviso_Periodo");
        });

        modelBuilder.Entity<CoordinadorDgaa>(entity =>
        {
            entity.HasKey(e => e.IdCoordinadorDgaa);

            entity.ToTable("CoordinadorDGAA");

            entity.HasIndex(e => e.IdAreaAcademica, "IX_CoordinadorDGAA_idAreaAcademica");

            entity.Property(e => e.IdCoordinadorDgaa).HasColumnName("idCoordinadorDGAA");
            entity.Property(e => e.Cargo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("cargo");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.IdAreaAcademica).HasColumnName("idAreaAcademica");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdAreaAcademicaNavigation).WithMany(p => p.CoordinadorDgaa)
                .HasForeignKey(d => d.IdAreaAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CoordinadorDGAA_AreaAcademica");
        });

        modelBuilder.Entity<CoordinadorEa>(entity =>
        {
            entity.HasKey(e => e.IdCoordinadorEa);

            entity.ToTable("CoordinadorEA");

            entity.HasIndex(e => e.IdEntidadAcademica, "IX_CoordinadorEA_idEntidadAcademica");

            entity.Property(e => e.IdCoordinadorEa).HasColumnName("idCoordinadorEA");
            entity.Property(e => e.Cargo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("cargo");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.IdEntidadAcademica).HasColumnName("idEntidadAcademica");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdEntidadAcademicaNavigation).WithMany(p => p.CoordinadorEa)
                .HasForeignKey(d => d.IdEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CoordinadorEA_EntidadAcademica");
        });

        modelBuilder.Entity<Dictamen>(entity =>
        {
            entity.HasKey(e => e.IdDictamen);

            entity.HasIndex(e => e.IdDocente, "IX_Dictamen_idDocente");

            entity.HasIndex(e => new { e.IdActa, e.IdDocente }, "UQ_Dictamen_Acta_Docente").IsUnique();

            entity.Property(e => e.IdDictamen).HasColumnName("idDictamen");
            entity.Property(e => e.IdActa).HasColumnName("idActa");
            entity.Property(e => e.IdDocente).HasColumnName("idDocente");
            entity.Property(e => e.RutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoFirmado");
            entity.Property(e => e.RutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoOriginal");

            entity.HasOne(d => d.IdActaNavigation).WithMany(p => p.Dictamen)
                .HasForeignKey(d => d.IdActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dictamen_Acta");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.Dictamen)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dictamen_Docente");
        });

        modelBuilder.Entity<Docente>(entity =>
        {
            entity.HasKey(e => e.IdDocente);

            entity.HasIndex(e => e.NumeroPersonal, "UQ_Docente_numeroPersonal").IsUnique();

            entity.Property(e => e.IdDocente).HasColumnName("idDocente");
            entity.Property(e => e.DescripcionPerfil)
                .IsUnicode(false)
                .HasColumnName("descripcionPerfil");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroPersonal)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("numeroPersonal");
            entity.Property(e => e.Puesto)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("puesto");
            entity.Property(e => e.RutaDocumentosGenerales)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentosGenerales");
        });

        modelBuilder.Entity<EntidadAcademica>(entity =>
        {
            entity.HasKey(e => e.IdEntidadAcademica);

            entity.HasIndex(e => e.IdAreaAcademica, "IX_EntidadAcademica_idAreaAcademica");

            entity.Property(e => e.IdEntidadAcademica).HasColumnName("idEntidadAcademica");
            entity.Property(e => e.CalleNumero)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("calleNumero");
            entity.Property(e => e.Campus)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("campus");
            entity.Property(e => e.Colonia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("colonia");
            entity.Property(e => e.Conmutador)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("conmutador");
            entity.Property(e => e.Cp)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("cp");
            entity.Property(e => e.Extension)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("extension");
            entity.Property(e => e.Fax)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("fax");
            entity.Property(e => e.IdAreaAcademica).HasColumnName("idAreaAcademica");
            entity.Property(e => e.Municipio)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("municipio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Region)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("region");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdAreaAcademicaNavigation).WithMany(p => p.EntidadAcademica)
                .HasForeignKey(d => d.IdAreaAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EntidadAcademica_AreaAcademica");
        });

        modelBuilder.Entity<ExperienciaEducativa>(entity =>
        {
            entity.HasKey(e => e.IdExperienciaEducativa);

            entity.HasIndex(e => e.IdPlanEstudios, "IX_ExperienciaEducativa_idPlanEstudios");

            entity.Property(e => e.IdExperienciaEducativa).HasColumnName("idExperienciaEducativa");
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("codigo");
            entity.Property(e => e.IdPlanEstudios).HasColumnName("idPlanEstudios");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PerfilDocente)
                .IsUnicode(false)
                .HasColumnName("perfilDocente");

            entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany(p => p.ExperienciaEducativa)
                .HasForeignKey(d => d.IdPlanEstudios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperienciaEducativa_PlanEstudios");
        });

        modelBuilder.Entity<Grado>(entity =>
        {
            entity.HasKey(e => e.IdGrado);

            entity.HasIndex(e => e.IdDocente, "IX_Grado_idDocente");

            entity.HasIndex(e => e.IdDocente, "UX_Grado_docente_ultimo")
                .IsUnique()
                .HasFilter("([ultimo]=(1))");

            entity.Property(e => e.IdGrado).HasColumnName("idGrado");
            entity.Property(e => e.Grado1)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("grado");
            entity.Property(e => e.IdDocente).HasColumnName("idDocente");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("titulo");
            entity.Property(e => e.Ultimo).HasColumnName("ultimo");

            entity.HasOne(d => d.IdDocenteNavigation).WithOne(p => p.Grado)
                .HasForeignKey<Grado>(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grado_Docente");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.IdHorario);

            entity.HasIndex(e => e.IdAviso, "IX_Horario_idAviso");

            entity.HasIndex(e => e.IdOferta, "IX_Horario_idOferta");

            entity.Property(e => e.IdHorario).HasColumnName("idHorario");
            entity.Property(e => e.Dia)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("dia");
            entity.Property(e => e.HoraFin).HasColumnName("horaFin");
            entity.Property(e => e.HoraInicio).HasColumnName("horaInicio");
            entity.Property(e => e.IdAviso).HasColumnName("idAviso");
            entity.Property(e => e.IdOferta).HasColumnName("idOferta");
            entity.Property(e => e.Salon)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("salon");

            entity.HasOne(d => d.IdAvisoNavigation).WithMany(p => p.Horario)
                .HasForeignKey(d => d.IdAviso)
                .HasConstraintName("FK_Horario_Aviso");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Horario)
                .HasForeignKey(d => d.IdOferta)
                .HasConstraintName("FK_Horario_Oferta");
        });

        modelBuilder.Entity<IntegranteCt>(entity =>
        {
            entity.HasKey(e => e.IdIntegranteCt);

            entity.ToTable("IntegranteCT");

            entity.HasIndex(e => e.IdEntidadAcademica, "IX_IntegranteCT_idEntidadAcademica");

            entity.Property(e => e.IdIntegranteCt).HasColumnName("idIntegranteCT");
            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("cargo");
            entity.Property(e => e.IdEntidadAcademica).HasColumnName("idEntidadAcademica");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdEntidadAcademicaNavigation).WithMany(p => p.IntegranteCt)
                .HasForeignKey(d => d.IdEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IntegranteCT_EntidadAcademica");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.IdLog);

            entity.HasIndex(e => e.IdOferta, "IX_Log_idOferta");

            entity.Property(e => e.IdLog).HasColumnName("idLog");
            entity.Property(e => e.IdOferta).HasColumnName("idOferta");
            entity.Property(e => e.Mensaje)
                .IsUnicode(false)
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Log)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_Oferta");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion);

            entity.HasIndex(e => e.IdActa, "IX_Notificacion_idActa");

            entity.Property(e => e.IdNotificacion).HasColumnName("idNotificacion");
            entity.Property(e => e.IdActa).HasColumnName("idActa");
            entity.Property(e => e.RutaDocumentoFirmado)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoFirmado");
            entity.Property(e => e.RutaDocumentoOriginal)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentoOriginal");

            entity.HasOne(d => d.IdActaNavigation).WithMany(p => p.Notificacion)
                .HasForeignKey(d => d.IdActa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Acta");
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.HasKey(e => e.IdOferta);

            entity.HasIndex(e => e.IdArticulo, "IX_Oferta_idArticulo");

            entity.HasIndex(e => e.IdDocente, "IX_Oferta_idDocente");

            entity.HasIndex(e => e.IdExperienciaEducativa, "IX_Oferta_idExperienciaEducativa");

            entity.HasIndex(e => e.IdPeriodo, "IX_Oferta_idPeriodo");

            entity.Property(e => e.IdOferta).HasColumnName("idOferta");
            entity.Property(e => e.EstadoSolicitudApertura)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoSolicitudApertura");
            entity.Property(e => e.Hsm).HasColumnName("hsm");
            entity.Property(e => e.IdArticulo).HasColumnName("idArticulo");
            entity.Property(e => e.IdDocente).HasColumnName("idDocente");
            entity.Property(e => e.IdExperienciaEducativa).HasColumnName("idExperienciaEducativa");
            entity.Property(e => e.IdPeriodo).HasColumnName("idPeriodo");
            entity.Property(e => e.Incluida).HasColumnName("incluida");
            entity.Property(e => e.Justificacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("justificacion");
            entity.Property(e => e.JustificacionApertura)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("justificacionApertura");
            entity.Property(e => e.Nrc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("nrc");
            entity.Property(e => e.Plaza)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("plaza");
            entity.Property(e => e.RutaArchivoSolicitudApertura)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaArchivoSolicitudApertura");
            entity.Property(e => e.TipoContratacion)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("tipoContratacion");
            entity.Property(e => e.Vacante).HasColumnName("vacante");

            entity.HasOne(d => d.IdArticuloNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdArticulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Articulo");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Docente");

            entity.HasOne(d => d.IdExperienciaEducativaNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdExperienciaEducativa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_ExperienciaEducativa");

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.Oferta)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oferta_Periodo");
        });

        modelBuilder.Entity<OfertaAviso>(entity =>
        {
            entity.HasKey(e => e.IdOfertaAviso);

            entity.HasIndex(e => e.IdAviso, "IX_OfertaAviso_idAviso");

            entity.HasIndex(e => new { e.IdOferta, e.IdAviso }, "UQ_OfertaAviso_Oferta_Aviso").IsUnique();

            entity.Property(e => e.IdOfertaAviso).HasColumnName("idOfertaAviso");
            entity.Property(e => e.IdAviso).HasColumnName("idAviso");
            entity.Property(e => e.IdOferta).HasColumnName("idOferta");

            entity.HasOne(d => d.IdAvisoNavigation).WithMany(p => p.OfertaAviso)
                .HasForeignKey(d => d.IdAviso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfertaAviso_Aviso");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.OfertaAviso)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfertaAviso_Oferta");
        });

        modelBuilder.Entity<Periodo>(entity =>
        {
            entity.HasKey(e => e.IdPeriodo);

            entity.HasIndex(e => new { e.Nombre, e.AnioInicio }, "UQ_Periodo_nombre_anioInicio").IsUnique();

            entity.Property(e => e.IdPeriodo)
                .ValueGeneratedNever()
                .HasColumnName("idPeriodo");
            entity.Property(e => e.AnioInicio).HasColumnName("anioInicio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<PlanEstudios>(entity =>
        {
            entity.HasKey(e => e.IdPlanEstudios);

            entity.HasIndex(e => e.IdProgramaEducativo, "IX_PlanEstudios_idProgramaEducativo");

            entity.Property(e => e.IdPlanEstudios).HasColumnName("idPlanEstudios");
            entity.Property(e => e.IdProgramaEducativo).HasColumnName("idProgramaEducativo");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("modalidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProgramaEducativoNavigation).WithMany(p => p.PlanEstudios)
                .HasForeignKey(d => d.IdProgramaEducativo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanEstudios_ProgramaEducativo");
        });

        modelBuilder.Entity<ProgramaEducativo>(entity =>
        {
            entity.HasKey(e => e.IdProgramaEducativo);

            entity.HasIndex(e => e.IdEntidadAcademica, "IX_ProgramaEducativo_idEntidadAcademica");

            entity.Property(e => e.IdProgramaEducativo).HasColumnName("idProgramaEducativo");
            entity.Property(e => e.IdEntidadAcademica).HasColumnName("idEntidadAcademica");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdEntidadAcademicaNavigation).WithMany(p => p.ProgramaEducativo)
                .HasForeignKey(d => d.IdEntidadAcademica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramaEducativo_EntidadAcademica");
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud);

            entity.HasIndex(e => e.IdDocente, "IX_Solicitud_idDocente");

            entity.HasIndex(e => e.IdOferta, "IX_Solicitud_idOferta");

            entity.Property(e => e.IdSolicitud).HasColumnName("idSolicitud");
            entity.Property(e => e.Designado).HasColumnName("designado");
            entity.Property(e => e.IdDocente).HasColumnName("idDocente");
            entity.Property(e => e.IdOferta).HasColumnName("idOferta");
            entity.Property(e => e.Justificacion)
                .IsUnicode(false)
                .HasColumnName("justificacion");
            entity.Property(e => e.Modalidad)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("modalidad");
            entity.Property(e => e.RutaDocumentosSolicitud)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumentosSolicitud");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.Solicitud)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Docente");

            entity.HasOne(d => d.IdOfertaNavigation).WithMany(p => p.Solicitud)
                .HasForeignKey(d => d.IdOferta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitud_Oferta");
        });

        modelBuilder.Entity<SuperUsuario>(entity =>
        {
            entity.HasKey(e => e.IdSuperUsuario);

            entity.Property(e => e.IdSuperUsuario).HasColumnName("idSuperUsuario");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
