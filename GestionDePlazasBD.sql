/*
    Script depurado y corregido para SQL Server
    Proyecto: Sistema de Gestión de Plazas

    Notas:
    - Se mantiene todo en un solo script.
    - Se conservan los campos de dirección repetidos en AreaAcademica y EntidadAcademica.
    - Se conservan los tipos varchar por decisión del equipo.
    - Se eliminaron dependencias del entorno original (rutas físicas, opciones avanzadas y usuario ligado a un login).
    - Este script está pensado para crear una base nueva.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'GestionDePlazasBD') IS NULL
BEGIN
    CREATE DATABASE [GestionDePlazasBD];
END
GO

USE [GestionDePlazasBD];
GO

/*==============================================================*/
/* TABLAS                                                       */
/*==============================================================*/

CREATE TABLE [dbo].[Acta](
    [idActa] [int] IDENTITY(1,1) NOT NULL,
    [idAviso] [int] NOT NULL,
    [folio] [varchar](10) NOT NULL,
    [lugar] [varchar](100) NOT NULL,
    [fecha] [date] NOT NULL,
    [horaInicio] [time](7) NOT NULL,
    [horaConclusion] [time](7) NOT NULL,
    [asuntosGenerales] [varchar](max) NOT NULL,
    [rutaDocumentoOriginal] [varchar](255) NULL,
    [rutaDocumentoFirmado] [varchar](255) NULL,
    [archivado] [bit] NULL,
    CONSTRAINT [PK_Acta] PRIMARY KEY CLUSTERED ([idActa] ASC),
    CONSTRAINT [CK_Acta_horas] CHECK ([horaInicio] < [horaConclusion])
);
GO

CREATE TABLE [dbo].[AreaAcademica](
    [idAreaAcademica] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    [calleNumero] [varchar](150) NOT NULL,
    [colonia] [varchar](100) NOT NULL,
    [cp] [varchar](5) NOT NULL,
    [municipio] [varchar](100) NOT NULL,
    [telefono] [varchar](30) NOT NULL,
    [conmutador] [varchar](20) NOT NULL,
    [extension] [varchar](5) NOT NULL,
    [fax] [varchar](20) NOT NULL,
    CONSTRAINT [PK_AreaAcademica] PRIMARY KEY CLUSTERED ([idAreaAcademica] ASC)
);
GO

CREATE TABLE [dbo].[Articulo](
    [idArticulo] [int] IDENTITY(1,1) NOT NULL,
    [numero] [varchar](10) NOT NULL,
    [descripcion] [varchar](max) NOT NULL,
    CONSTRAINT [PK_Articulo] PRIMARY KEY CLUSTERED ([idArticulo] ASC),
    CONSTRAINT [UQ_Articulo_numero] UNIQUE ([numero])
);
GO

CREATE TABLE [dbo].[Asistencia](
    [idAsistencia] [int] IDENTITY(1,1) NOT NULL,
    [idActa] [int] NOT NULL,
    [idIntegranteCT] [int] NOT NULL,
    [asistio] [bit] NOT NULL,
    CONSTRAINT [PK_Asistencia] PRIMARY KEY CLUSTERED ([idAsistencia] ASC),
    CONSTRAINT [UQ_Asistencia_Acta_IntegranteCT] UNIQUE ([idActa], [idIntegranteCT])
);
GO

CREATE TABLE [dbo].[Aviso](
    [idAviso] [int] IDENTITY(1,1) NOT NULL,
    [idEntidadAcademica] [int] NOT NULL,
    [idPeriodo] [int] NOT NULL,
    [idArticulo] [int] NOT NULL,
    [folio] [varchar](50) NOT NULL,
    [fechaInicio] [date] NOT NULL,
    [fechaCreacion] [date] NOT NULL,
    [requisitos] [varchar](max) NOT NULL,
    [lugar] [varchar](100) NULL,
    [correo] [varchar](255) NULL,
    [modalidad] [varchar](10) NOT NULL,
    [rutaDocumentoOriginal] [varchar](255) NULL,
    [rutaDocumentoFirmado] [varchar](255) NULL,
    [archivado] [bit] NULL,
    CONSTRAINT [PK_Aviso] PRIMARY KEY CLUSTERED ([idAviso] ASC),
    CONSTRAINT [CK_modalidad_aviso] CHECK ([modalidad] IN ('Virtual', 'Presencial')),
    CONSTRAINT [CK_Aviso_fechas] CHECK ([fechaCreacion] <= [fechaInicio])
);
GO

CREATE TABLE [dbo].[CoordinadorDGAA](
    [idCoordinadorDGAA] [int] IDENTITY(1,1) NOT NULL,
    [idAreaAcademica] [int] NOT NULL,
    [nombre] [varchar](255) NOT NULL,
    [correo] [varchar](255) NOT NULL,
    [cargo] [varchar](255) NOT NULL,
    CONSTRAINT [PK_CoordinadorDGAA] PRIMARY KEY CLUSTERED ([idCoordinadorDGAA] ASC)
);
GO

CREATE TABLE [dbo].[CoordinadorEA](
    [idCoordinadorEA] [int] IDENTITY(1,1) NOT NULL,
    [idEntidadAcademica] [int] NOT NULL,
    [nombre] [varchar](255) NOT NULL,
    [correo] [varchar](255) NOT NULL,
    [cargo] [varchar](255) NOT NULL,
    CONSTRAINT [PK_CoordinadorEA] PRIMARY KEY CLUSTERED ([idCoordinadorEA] ASC)
);
GO

CREATE TABLE [dbo].[Dictamen](
    [idDictamen] [int] IDENTITY(1,1) NOT NULL,
    [idActa] [int] NOT NULL,
    [idDocente] [int] NOT NULL,
    [rutaDocumentoOriginal] [varchar](255) NOT NULL,
    [rutaDocumentoFirmado] [varchar](255) NULL,
    CONSTRAINT [PK_Dictamen] PRIMARY KEY CLUSTERED ([idDictamen] ASC),
    CONSTRAINT [UQ_Dictamen_Acta_Docente] UNIQUE ([idActa], [idDocente])
);
GO

CREATE TABLE [dbo].[Docente](
    [idDocente] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](255) NOT NULL,
    [descripcionPerfil] [varchar](max) NULL,
    [rutaDocumentosGenerales] [varchar](255) NULL,
    [numeroPersonal] [varchar](10) NOT NULL,
    [puesto] [varchar](25) NOT NULL,
    CONSTRAINT [PK_Docente] PRIMARY KEY CLUSTERED ([idDocente] ASC),
    CONSTRAINT [UQ_Docente_numeroPersonal] UNIQUE ([numeroPersonal]),
    CONSTRAINT [CK_puesto_docente] CHECK ([puesto] IN ('Docente por Asignatura', 'Técnico Académico', 'Docente', 'Investigador'))
);
GO

CREATE TABLE [dbo].[EntidadAcademica](
    [idEntidadAcademica] [int] IDENTITY(1,1) NOT NULL,
    [idAreaAcademica] [int] NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    [calleNumero] [varchar](150) NOT NULL,
    [colonia] [varchar](100) NOT NULL,
    [cp] [varchar](5) NOT NULL,
    [municipio] [varchar](100) NOT NULL,
    [telefono] [varchar](30) NOT NULL,
    [conmutador] [varchar](20) NOT NULL,
    [extension] [varchar](5) NOT NULL,
    [fax] [varchar](20) NOT NULL,
    [region] [varchar](30) NOT NULL,
    [campus] [varchar](255) NOT NULL,
    CONSTRAINT [PK_EntidadAcademica] PRIMARY KEY CLUSTERED ([idEntidadAcademica] ASC),
    CONSTRAINT [CK_region_entidadAcademica] CHECK ([region] IN (
        '1-Xalapa',
        '2-Veracruz',
        '3-Orizaba-Córdoba',
        '4-Poza Rica-Túxpan',
        '5-Coatzacoalcos-Minatitlán'
    ))
);
GO

CREATE TABLE [dbo].[ExperienciaEducativa](
    [idExperienciaEducativa] [int] IDENTITY(1,1) NOT NULL,
    [idPlanEstudios] [int] NOT NULL,
    [codigo] [varchar](10) NOT NULL,
    [nombre] [varchar](150) NOT NULL,
    [perfilDocente] [varchar](max) NOT NULL,
    CONSTRAINT [PK_ExperienciaEducativa] PRIMARY KEY CLUSTERED ([idExperienciaEducativa] ASC)
);
GO

CREATE TABLE [dbo].[Grado](
    [idGrado] [int] IDENTITY(1,1) NOT NULL,
    [idDocente] [int] NOT NULL,
    [grado] [varchar](15) NOT NULL,
    [titulo] [varchar](100) NOT NULL,
    [ultimo] [bit] NOT NULL,
    CONSTRAINT [PK_Grado] PRIMARY KEY CLUSTERED ([idGrado] ASC),
    CONSTRAINT [CK_grado_grado] CHECK ([grado] IN ('Doctorado', 'Maestría', 'Especialidad', 'Licenciatura'))
);
GO

CREATE TABLE [dbo].[Horario](
    [idHorario] [int] IDENTITY(1,1) NOT NULL,
    [idOferta] [int] NULL,
    [idAviso] [int] NULL,
    [dia] [varchar](10) NOT NULL,
    [horaInicio] [time](7) NOT NULL,
    [horaFin] [time](7) NOT NULL,
    [salon] [varchar](100) NULL,
    CONSTRAINT [PK_Horario] PRIMARY KEY CLUSTERED ([idHorario] ASC),
    CONSTRAINT [CK_dia_horario] CHECK ([dia] IN ('Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado')),
    CONSTRAINT [CK_Horario_horas] CHECK ([horaInicio] < [horaFin]),
    CONSTRAINT [CK_Horario_unSoloOrigen] CHECK (
        ([idOferta] IS NOT NULL AND [idAviso] IS NULL) OR
        ([idOferta] IS NULL AND [idAviso] IS NOT NULL)
    )
);
GO

CREATE TABLE [dbo].[IntegranteCT](
    [idIntegranteCT] [int] IDENTITY(1,1) NOT NULL,
    [idEntidadAcademica] [int] NOT NULL,
    [nombre] [varchar](255) NOT NULL,
    [cargo] [varchar](100) NOT NULL,
    CONSTRAINT [PK_IntegranteCT] PRIMARY KEY CLUSTERED ([idIntegranteCT] ASC)
);
GO

CREATE TABLE [dbo].[Log](
    [idLog] [int] IDENTITY(1,1) NOT NULL,
    [idOferta] [int] NOT NULL,
    [mensaje] [varchar](max) NOT NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([idLog] ASC)
);
GO

CREATE TABLE [dbo].[Notificacion](
    [idNotificacion] [int] IDENTITY(1,1) NOT NULL,
    [idActa] [int] NOT NULL,
    [rutaDocumentoOriginal] [varchar](255) NOT NULL,
    [rutaDocumentoFirmado] [varchar](255) NULL,
    CONSTRAINT [PK_Notificacion] PRIMARY KEY CLUSTERED ([idNotificacion] ASC)
);
GO

CREATE TABLE [dbo].[Oferta](
    [idOferta] [int] IDENTITY(1,1) NOT NULL,
    [idDocente] [int] NOT NULL,
    [idExperienciaEducativa] [int] NOT NULL,
    [idArticulo] [int] NOT NULL,
    [idPeriodo] [int] NOT NULL,
    [plaza] [varchar](4) NULL,
    [tipoContratacion] [varchar](3) NOT NULL,
    [nrc] [varchar](5) NOT NULL,
    [vacante] [bit] NOT NULL,
    [incluida] [bit] NOT NULL,
    [justificacion] [varchar](50) NOT NULL,
    [rutaArchivoSolicitudApertura] [varchar](255) NULL,
    [estadoSolicitudApertura] [varchar](50) NOT NULL,
    [hsm] [int] NOT NULL,
    [justificacionApertura] [varchar](255) NULL,
    CONSTRAINT [PK_Oferta] PRIMARY KEY CLUSTERED ([idOferta] ASC),
    CONSTRAINT [CK_estadoSolicitudOferta_Oferta] CHECK ([estadoSolicitudApertura] IN ('Pendiente', 'Aceptada', 'Rechazada')),
    CONSTRAINT [CK_justificacion_oferta] CHECK ([justificacion] IN ('Demanda insuficiente', 'Reservada Para Otra Plaza')),
    CONSTRAINT [CK_tipoContratacion_oferta] CHECK ([tipoContratacion] IN ('IOD', 'IPP')),
    CONSTRAINT [CK_Oferta_hsm] CHECK ([hsm] > 0)
);
GO

CREATE TABLE [dbo].[OfertaAviso](
    [idOfertaAviso] [int] IDENTITY(1,1) NOT NULL,
    [idOferta] [int] NOT NULL,
    [idAviso] [int] NOT NULL,
    CONSTRAINT [PK_OfertaAviso] PRIMARY KEY CLUSTERED ([idOfertaAviso] ASC),
    CONSTRAINT [UQ_OfertaAviso_Oferta_Aviso] UNIQUE ([idOferta], [idAviso])
);
GO

CREATE TABLE [dbo].[Periodo](
    [idPeriodo] [int] NOT NULL,
    [nombre] [varchar](15) NOT NULL,
    [anioInicio] [int] NOT NULL,
    CONSTRAINT [PK_Periodo] PRIMARY KEY CLUSTERED ([idPeriodo] ASC),
    CONSTRAINT [UQ_Periodo_nombre_anioInicio] UNIQUE ([nombre], [anioInicio]),
    CONSTRAINT [CK_nombre_periodo] CHECK ([nombre] IN ('Agosto-Enero', 'Febrero-Julio')),
    CONSTRAINT [CK_Periodo_anioInicio] CHECK ([anioInicio] >= 2000)
);
GO

CREATE TABLE [dbo].[PlanEstudios](
    [idPlanEstudios] [int] IDENTITY(1,1) NOT NULL,
    [idProgramaEducativo] [int] NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    [modalidad] [varchar](100) NOT NULL,
    CONSTRAINT [PK_PlanEstudios] PRIMARY KEY CLUSTERED ([idPlanEstudios] ASC)
);
GO

CREATE TABLE [dbo].[ProgramaEducativo](
    [idProgramaEducativo] [int] IDENTITY(1,1) NOT NULL,
    [idEntidadAcademica] [int] NOT NULL,
    [nombre] [varchar](100) NOT NULL,
    CONSTRAINT [PK_ProgramaEducativo] PRIMARY KEY CLUSTERED ([idProgramaEducativo] ASC)
);
GO

CREATE TABLE [dbo].[Solicitud](
    [idSolicitud] [int] IDENTITY(1,1) NOT NULL,
    [idDocente] [int] NOT NULL,
    [idOferta] [int] NOT NULL,
    [designado] [bit] NOT NULL,
    [modalidad] [varchar](10) NOT NULL,
    [justificacion] [varchar](max) NOT NULL,
    [rutaDocumentosSolicitud] [varchar](255) NOT NULL,
    CONSTRAINT [PK_Solicitud] PRIMARY KEY CLUSTERED ([idSolicitud] ASC),
    CONSTRAINT [CK_modalidad_solicitud] CHECK ([modalidad] IN ('Mayoría', 'Unánime'))
);
GO

CREATE TABLE [dbo].[SuperUsuario](
    [idSuperUsuario] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](255) NOT NULL,
    [correo] [varchar](255) NOT NULL,
    CONSTRAINT [PK_SuperUsuario] PRIMARY KEY CLUSTERED ([idSuperUsuario] ASC)
);
GO

/*==============================================================*/
/* CLAVES FORÁNEAS                                              */
/*==============================================================*/

ALTER TABLE [dbo].[Acta]
ADD CONSTRAINT [FK_Acta_Aviso]
    FOREIGN KEY ([idAviso]) REFERENCES [dbo].[Aviso] ([idAviso]);
GO

ALTER TABLE [dbo].[Asistencia]
ADD CONSTRAINT [FK_Asistencia_Acta]
    FOREIGN KEY ([idActa]) REFERENCES [dbo].[Acta] ([idActa]);
GO

ALTER TABLE [dbo].[Asistencia]
ADD CONSTRAINT [FK_Asistencia_IntegranteCT]
    FOREIGN KEY ([idIntegranteCT]) REFERENCES [dbo].[IntegranteCT] ([idIntegranteCT]);
GO

ALTER TABLE [dbo].[Aviso]
ADD CONSTRAINT [FK_Aviso_Articulo]
    FOREIGN KEY ([idArticulo]) REFERENCES [dbo].[Articulo] ([idArticulo]);
GO

ALTER TABLE [dbo].[Aviso]
ADD CONSTRAINT [FK_Aviso_EntidadAcademica]
    FOREIGN KEY ([idEntidadAcademica]) REFERENCES [dbo].[EntidadAcademica] ([idEntidadAcademica]);
GO

ALTER TABLE [dbo].[Aviso]
ADD CONSTRAINT [FK_Aviso_Periodo]
    FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]);
GO

ALTER TABLE [dbo].[CoordinadorDGAA]
ADD CONSTRAINT [FK_CoordinadorDGAA_AreaAcademica]
    FOREIGN KEY ([idAreaAcademica]) REFERENCES [dbo].[AreaAcademica] ([idAreaAcademica]);
GO

ALTER TABLE [dbo].[CoordinadorEA]
ADD CONSTRAINT [FK_CoordinadorEA_EntidadAcademica]
    FOREIGN KEY ([idEntidadAcademica]) REFERENCES [dbo].[EntidadAcademica] ([idEntidadAcademica]);
GO

ALTER TABLE [dbo].[Dictamen]
ADD CONSTRAINT [FK_Dictamen_Acta]
    FOREIGN KEY ([idActa]) REFERENCES [dbo].[Acta] ([idActa]);
GO

ALTER TABLE [dbo].[Dictamen]
ADD CONSTRAINT [FK_Dictamen_Docente]
    FOREIGN KEY ([idDocente]) REFERENCES [dbo].[Docente] ([idDocente]);
GO

ALTER TABLE [dbo].[EntidadAcademica]
ADD CONSTRAINT [FK_EntidadAcademica_AreaAcademica]
    FOREIGN KEY ([idAreaAcademica]) REFERENCES [dbo].[AreaAcademica] ([idAreaAcademica]);
GO

ALTER TABLE [dbo].[ExperienciaEducativa]
ADD CONSTRAINT [FK_ExperienciaEducativa_PlanEstudios]
    FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]);
GO

ALTER TABLE [dbo].[Grado]
ADD CONSTRAINT [FK_Grado_Docente]
    FOREIGN KEY ([idDocente]) REFERENCES [dbo].[Docente] ([idDocente]);
GO

ALTER TABLE [dbo].[Horario]
ADD CONSTRAINT [FK_Horario_Aviso]
    FOREIGN KEY ([idAviso]) REFERENCES [dbo].[Aviso] ([idAviso]);
GO

ALTER TABLE [dbo].[Horario]
ADD CONSTRAINT [FK_Horario_Oferta]
    FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]);
GO

ALTER TABLE [dbo].[IntegranteCT]
ADD CONSTRAINT [FK_IntegranteCT_EntidadAcademica]
    FOREIGN KEY ([idEntidadAcademica]) REFERENCES [dbo].[EntidadAcademica] ([idEntidadAcademica]);
GO

ALTER TABLE [dbo].[Log]
ADD CONSTRAINT [FK_Log_Oferta]
    FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]);
GO

ALTER TABLE [dbo].[Notificacion]
ADD CONSTRAINT [FK_Notificacion_Acta]
    FOREIGN KEY ([idActa]) REFERENCES [dbo].[Acta] ([idActa]);
GO

ALTER TABLE [dbo].[Oferta]
ADD CONSTRAINT [FK_Oferta_Articulo]
    FOREIGN KEY ([idArticulo]) REFERENCES [dbo].[Articulo] ([idArticulo]);
GO

ALTER TABLE [dbo].[Oferta]
ADD CONSTRAINT [FK_Oferta_Docente]
    FOREIGN KEY ([idDocente]) REFERENCES [dbo].[Docente] ([idDocente]);
GO

ALTER TABLE [dbo].[Oferta]
ADD CONSTRAINT [FK_Oferta_ExperienciaEducativa]
    FOREIGN KEY ([idExperienciaEducativa]) REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa]);
GO

ALTER TABLE [dbo].[Oferta]
ADD CONSTRAINT [FK_Oferta_Periodo]
    FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]);
GO

ALTER TABLE [dbo].[OfertaAviso]
ADD CONSTRAINT [FK_OfertaAviso_Aviso]
    FOREIGN KEY ([idAviso]) REFERENCES [dbo].[Aviso] ([idAviso]);
GO

ALTER TABLE [dbo].[OfertaAviso]
ADD CONSTRAINT [FK_OfertaAviso_Oferta]
    FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]);
GO

ALTER TABLE [dbo].[PlanEstudios]
ADD CONSTRAINT [FK_PlanEstudios_ProgramaEducativo]
    FOREIGN KEY ([idProgramaEducativo]) REFERENCES [dbo].[ProgramaEducativo] ([idProgramaEducativo]);
GO

ALTER TABLE [dbo].[ProgramaEducativo]
ADD CONSTRAINT [FK_ProgramaEducativo_EntidadAcademica]
    FOREIGN KEY ([idEntidadAcademica]) REFERENCES [dbo].[EntidadAcademica] ([idEntidadAcademica]);
GO

ALTER TABLE [dbo].[Solicitud]
ADD CONSTRAINT [FK_Solicitud_Docente]
    FOREIGN KEY ([idDocente]) REFERENCES [dbo].[Docente] ([idDocente]);
GO

ALTER TABLE [dbo].[Solicitud]
ADD CONSTRAINT [FK_Solicitud_Oferta]
    FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]);
GO

/*==============================================================*/
/* ÍNDICES RECOMENDADOS PARA CLAVES FORÁNEAS                    */
/*==============================================================*/

CREATE INDEX [IX_Acta_idAviso] ON [dbo].[Acta] ([idAviso]);
GO

CREATE INDEX [IX_Aviso_idArticulo] ON [dbo].[Aviso] ([idArticulo]);
GO
CREATE INDEX [IX_Aviso_idEntidadAcademica] ON [dbo].[Aviso] ([idEntidadAcademica]);
GO
CREATE INDEX [IX_Aviso_idPeriodo] ON [dbo].[Aviso] ([idPeriodo]);
GO

CREATE INDEX [IX_Asistencia_idIntegranteCT] ON [dbo].[Asistencia] ([idIntegranteCT]);
GO

CREATE INDEX [IX_CoordinadorDGAA_idAreaAcademica] ON [dbo].[CoordinadorDGAA] ([idAreaAcademica]);
GO

CREATE INDEX [IX_CoordinadorEA_idEntidadAcademica] ON [dbo].[CoordinadorEA] ([idEntidadAcademica]);
GO

CREATE INDEX [IX_Dictamen_idDocente] ON [dbo].[Dictamen] ([idDocente]);
GO

CREATE INDEX [IX_EntidadAcademica_idAreaAcademica] ON [dbo].[EntidadAcademica] ([idAreaAcademica]);
GO

CREATE INDEX [IX_ExperienciaEducativa_idPlanEstudios] ON [dbo].[ExperienciaEducativa] ([idPlanEstudios]);
GO

CREATE INDEX [IX_Grado_idDocente] ON [dbo].[Grado] ([idDocente]);
GO
CREATE UNIQUE INDEX [UX_Grado_docente_ultimo]
    ON [dbo].[Grado] ([idDocente])
    WHERE [ultimo] = 1;
GO

CREATE INDEX [IX_Horario_idAviso] ON [dbo].[Horario] ([idAviso]);
GO
CREATE INDEX [IX_Horario_idOferta] ON [dbo].[Horario] ([idOferta]);
GO

CREATE INDEX [IX_IntegranteCT_idEntidadAcademica] ON [dbo].[IntegranteCT] ([idEntidadAcademica]);
GO

CREATE INDEX [IX_Log_idOferta] ON [dbo].[Log] ([idOferta]);
GO

CREATE INDEX [IX_Notificacion_idActa] ON [dbo].[Notificacion] ([idActa]);
GO

CREATE INDEX [IX_Oferta_idArticulo] ON [dbo].[Oferta] ([idArticulo]);
GO
CREATE INDEX [IX_Oferta_idDocente] ON [dbo].[Oferta] ([idDocente]);
GO
CREATE INDEX [IX_Oferta_idExperienciaEducativa] ON [dbo].[Oferta] ([idExperienciaEducativa]);
GO
CREATE INDEX [IX_Oferta_idPeriodo] ON [dbo].[Oferta] ([idPeriodo]);
GO

CREATE INDEX [IX_OfertaAviso_idAviso] ON [dbo].[OfertaAviso] ([idAviso]);
GO

CREATE INDEX [IX_PlanEstudios_idProgramaEducativo] ON [dbo].[PlanEstudios] ([idProgramaEducativo]);
GO

CREATE INDEX [IX_ProgramaEducativo_idEntidadAcademica] ON [dbo].[ProgramaEducativo] ([idEntidadAcademica]);
GO

CREATE INDEX [IX_Solicitud_idDocente] ON [dbo].[Solicitud] ([idDocente]);
GO
CREATE INDEX [IX_Solicitud_idOferta] ON [dbo].[Solicitud] ([idOferta]);
GO
