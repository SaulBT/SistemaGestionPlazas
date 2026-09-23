SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'plazas.acta_consejo_tecnico', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[acta_consejo_tecnico]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_id] int NOT NULL,
        [entidad_academica_id] int NOT NULL,
        [folio] varchar(100) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [fecha] date NOT NULL,
        [lugar] nvarchar(300) COLLATE Modern_Spanish_100_CI_AI NULL,
        [hora_inicio] time(0) NULL,
        [hora_fin] time(0) NULL,
        [asuntos_generales] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        [estado] varchar(30) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [archivada_en] datetime2(0) NULL,
        [archivada_por_usuario_id] int NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        [eliminada_por_usuario_id] int NULL,
        CONSTRAINT [pk_acta_consejo_tecnico] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_acta_consejo_tecnico__aviso] FOREIGN KEY ([aviso_id]) REFERENCES [plazas].[aviso] ([id]),
        CONSTRAINT [fk_acta_consejo_tecnico__entidad] FOREIGN KEY ([entidad_academica_id]) REFERENCES [academico].[entidad_academica] ([id]),
        CONSTRAINT [fk_acta_consejo_tecnico__archivada_por] FOREIGN KEY ([archivada_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_acta_consejo_tecnico__eliminada_por] FOREIGN KEY ([eliminada_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [ck_acta_consejo_tecnico__folio] CHECK ([folio] = UPPER(LTRIM(RTRIM([folio]))) AND [folio] NOT LIKE '%[^A-Z0-9/-]%' AND LEN(LTRIM(RTRIM([folio]))) > 0),
        CONSTRAINT [ck_acta_consejo_tecnico__lugar] CHECK ([lugar] IS NULL OR LEN(LTRIM(RTRIM([lugar]))) > 0),
        CONSTRAINT [ck_acta_consejo_tecnico__horas] CHECK (([hora_inicio] IS NULL AND [hora_fin] IS NULL) OR ([hora_inicio] IS NOT NULL AND [hora_fin] IS NOT NULL AND [hora_inicio] < [hora_fin])),
        CONSTRAINT [ck_acta_consejo_tecnico__estado] CHECK ([estado] IN ('CREADA', 'EN_REVISION_DGAA', 'DEVUELTA_DGAA', 'AVALADA_DGAA', 'FIRMADA')),
        CONSTRAINT [ck_acta_consejo_tecnico__archivo] CHECK (([archivada_en] IS NULL AND [archivada_por_usuario_id] IS NULL) OR ([archivada_en] IS NOT NULL AND [archivada_por_usuario_id] IS NOT NULL)),
        CONSTRAINT [ck_acta_consejo_tecnico__eliminacion] CHECK (([fecha_eliminacion] IS NULL AND [eliminada_por_usuario_id] IS NULL) OR ([fecha_eliminacion] IS NOT NULL AND [eliminada_por_usuario_id] IS NOT NULL))
    );
END
GO

IF OBJECT_ID(N'plazas.acta_oferta', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[acta_oferta]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [acta_consejo_tecnico_id] int NOT NULL,
        [aviso_oferta_id] int NOT NULL,
        [resultado] varchar(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [observaciones] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        [solicitud_designada_id] int NULL,
        [docente_asignado_id] int NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        [eliminado_por_usuario_id] int NULL,
        CONSTRAINT [pk_acta_oferta] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_acta_oferta__acta] FOREIGN KEY ([acta_consejo_tecnico_id]) REFERENCES [plazas].[acta_consejo_tecnico] ([id]),
        CONSTRAINT [fk_acta_oferta__aviso_oferta] FOREIGN KEY ([aviso_oferta_id]) REFERENCES [plazas].[aviso_oferta] ([id]),
        CONSTRAINT [fk_acta_oferta__solicitud] FOREIGN KEY ([solicitud_designada_id]) REFERENCES [plazas].[solicitud] ([id]),
        CONSTRAINT [fk_acta_oferta__docente] FOREIGN KEY ([docente_asignado_id]) REFERENCES [academico].[docente] ([id]),
        CONSTRAINT [fk_acta_oferta__eliminado_por] FOREIGN KEY ([eliminado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [ck_acta_oferta__resultado] CHECK ([resultado] IN ('PENDIENTE', 'DESIGNADA', 'DESIERTA', 'SIN_ASPIRANTES')),
        CONSTRAINT [ck_acta_oferta__observaciones] CHECK ([observaciones] IS NULL OR LEN(LTRIM(RTRIM([observaciones]))) > 0),
        CONSTRAINT [ck_acta_oferta__eliminacion] CHECK (([fecha_eliminacion] IS NULL AND [eliminado_por_usuario_id] IS NULL) OR ([fecha_eliminacion] IS NOT NULL AND [eliminado_por_usuario_id] IS NOT NULL)),
        CONSTRAINT [ck_acta_oferta__designacion] CHECK (([resultado] = 'DESIGNADA' AND [solicitud_designada_id] IS NOT NULL) OR ([resultado] <> 'DESIGNADA' AND [solicitud_designada_id] IS NULL AND [docente_asignado_id] IS NULL))
    );
END
GO

IF OBJECT_ID(N'plazas.votacion_solicitud', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[votacion_solicitud]
    (
        [acta_oferta_id] int NOT NULL,
        [solicitud_id] int NOT NULL,
        [votos] int NOT NULL,
        CONSTRAINT [pk_votacion_solicitud] PRIMARY KEY CLUSTERED ([acta_oferta_id], [solicitud_id]),
        CONSTRAINT [fk_votacion_solicitud__acta_oferta] FOREIGN KEY ([acta_oferta_id]) REFERENCES [plazas].[acta_oferta] ([id]),
        CONSTRAINT [fk_votacion_solicitud__solicitud] FOREIGN KEY ([solicitud_id]) REFERENCES [plazas].[solicitud] ([id]),
        CONSTRAINT [ck_votacion_solicitud__votos] CHECK ([votos] >= 0)
    );
END
GO

IF OBJECT_ID(N'plazas.acta_asistencia', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[acta_asistencia]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [acta_consejo_tecnico_id] int NOT NULL,
        [integrante_consejo_tecnico_id] int NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [tratamiento] nvarchar(30) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [cargo] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [asistio] bit NOT NULL,
        [firmo] bit NOT NULL,
        CONSTRAINT [pk_acta_asistencia] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_acta_asistencia__acta] FOREIGN KEY ([acta_consejo_tecnico_id]) REFERENCES [plazas].[acta_consejo_tecnico] ([id]),
        CONSTRAINT [fk_acta_asistencia__integrante] FOREIGN KEY ([integrante_consejo_tecnico_id]) REFERENCES [plazas].[integrante_consejo_tecnico] ([id]),
        CONSTRAINT [uq_acta_asistencia__acta_integrante] UNIQUE ([acta_consejo_tecnico_id], [integrante_consejo_tecnico_id]),
        CONSTRAINT [ck_acta_asistencia__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_acta_asistencia__tratamiento] CHECK (LEN(LTRIM(RTRIM([tratamiento]))) > 0),
        CONSTRAINT [ck_acta_asistencia__cargo] CHECK (LEN(LTRIM(RTRIM([cargo]))) > 0),
        CONSTRAINT [ck_acta_asistencia__firma] CHECK ([firmo] = 0 OR [asistio] = 1)
    );
END
GO

IF OBJECT_ID(N'plazas.documento_acta', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[documento_acta]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [acta_consejo_tecnico_id] int NOT NULL,
        [tipo] varchar(10) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(260) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [mime] varchar(255) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [tamano] bigint NOT NULL,
        [checksum_sha256] binary(32) NOT NULL,
        [clave_almacenamiento] nvarchar(500) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [numero_version] int NOT NULL,
        [es_vigente] bit NOT NULL,
        [cargado_en] datetime2(0) NOT NULL CONSTRAINT [df_documento_acta__cargado_en] DEFAULT (SYSUTCDATETIME()),
        [cargado_por_usuario_id] int NOT NULL,
        CONSTRAINT [pk_documento_acta] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_documento_acta__acta] FOREIGN KEY ([acta_consejo_tecnico_id]) REFERENCES [plazas].[acta_consejo_tecnico] ([id]),
        CONSTRAINT [fk_documento_acta__usuario] FOREIGN KEY ([cargado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_documento_acta__version] UNIQUE ([acta_consejo_tecnico_id], [tipo], [numero_version]),
        CONSTRAINT [ck_documento_acta__tipo] CHECK ([tipo] IN ('ORIGINAL', 'FIRMADO')),
        CONSTRAINT [ck_documento_acta__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_documento_acta__tamano] CHECK ([tamano] > 0),
        CONSTRAINT [ck_documento_acta__version] CHECK ([numero_version] > 0),
        CONSTRAINT [ck_documento_acta__clave] CHECK (LEN(LTRIM(RTRIM([clave_almacenamiento]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.revision_acta', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[revision_acta]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [acta_consejo_tecnico_id] int NOT NULL,
        [numero_revision] int NOT NULL,
        [documento_original_id] int NOT NULL,
        [enviado_por_usuario_id] int NOT NULL,
        [enviado_en] datetime2(0) NOT NULL CONSTRAINT [df_revision_acta__enviado_en] DEFAULT (SYSUTCDATETIME()),
        [resuelto_por_usuario_id] int NULL,
        [resuelto_en] datetime2(0) NULL,
        [resultado] varchar(10) COLLATE Latin1_General_100_BIN2 NULL,
        [comentarios] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_revision_acta] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_revision_acta__acta] FOREIGN KEY ([acta_consejo_tecnico_id]) REFERENCES [plazas].[acta_consejo_tecnico] ([id]),
        CONSTRAINT [fk_revision_acta__documento] FOREIGN KEY ([documento_original_id]) REFERENCES [plazas].[documento_acta] ([id]),
        CONSTRAINT [fk_revision_acta__enviado_por] FOREIGN KEY ([enviado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_revision_acta__resuelto_por] FOREIGN KEY ([resuelto_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_revision_acta__numero] UNIQUE ([acta_consejo_tecnico_id], [numero_revision]),
        CONSTRAINT [ck_revision_acta__numero] CHECK ([numero_revision] > 0),
        CONSTRAINT [ck_revision_acta__resultado] CHECK ([resultado] IS NULL OR [resultado] IN ('AVALADA', 'DEVUELTA')),
        CONSTRAINT [ck_revision_acta__resolucion] CHECK (([resuelto_en] IS NULL AND [resuelto_por_usuario_id] IS NULL AND [resultado] IS NULL) OR ([resuelto_en] IS NOT NULL AND [resuelto_por_usuario_id] IS NOT NULL AND [resultado] IS NOT NULL)),
        CONSTRAINT [ck_revision_acta__devolucion] CHECK ([resultado] <> 'DEVUELTA' OR ([comentarios] IS NOT NULL AND LEN(LTRIM(RTRIM([comentarios]))) > 0))
    );
END
GO

IF OBJECT_ID(N'academico.asignacion_docente', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[asignacion_docente]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [programacion_academica_id] int NOT NULL,
        [docente_id] int NOT NULL,
        [origen] varchar(10) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [fecha_inicio] date NOT NULL,
        [fecha_fin] date NULL,
        [sincronizacion_planea_id] int NULL,
        [acta_oferta_id] int NULL,
        CONSTRAINT [pk_asignacion_docente] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_asignacion_docente__programacion] FOREIGN KEY ([programacion_academica_id]) REFERENCES [academico].[programacion_academica] ([id]),
        CONSTRAINT [fk_asignacion_docente__docente] FOREIGN KEY ([docente_id]) REFERENCES [academico].[docente] ([id]),
        CONSTRAINT [fk_asignacion_docente__sincronizacion] FOREIGN KEY ([sincronizacion_planea_id]) REFERENCES [integracion].[sincronizacion_planea] ([id]),
        CONSTRAINT [fk_asignacion_docente__acta_oferta] FOREIGN KEY ([acta_oferta_id]) REFERENCES [plazas].[acta_oferta] ([id]),
        CONSTRAINT [ck_asignacion_docente__origen] CHECK ([origen] IN ('PLANEA', 'SGPLA')),
        CONSTRAINT [ck_asignacion_docente__fechas] CHECK ([fecha_fin] IS NULL OR [fecha_inicio] <= [fecha_fin]),
        CONSTRAINT [ck_asignacion_docente__referencia_origen] CHECK (([origen] = 'PLANEA' AND [sincronizacion_planea_id] IS NOT NULL AND [acta_oferta_id] IS NULL) OR ([origen] = 'SGPLA' AND [acta_oferta_id] IS NOT NULL AND [sincronizacion_planea_id] IS NULL))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_acta_consejo_tecnico__entidad_folio_activa' AND object_id = OBJECT_ID(N'plazas.acta_consejo_tecnico'))
    CREATE UNIQUE INDEX [ux_acta_consejo_tecnico__entidad_folio_activa] ON [plazas].[acta_consejo_tecnico] ([entidad_academica_id], [folio]) WHERE [fecha_eliminacion] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_acta_consejo_tecnico__aviso_estado' AND object_id = OBJECT_ID(N'plazas.acta_consejo_tecnico'))
    CREATE INDEX [ix_acta_consejo_tecnico__aviso_estado] ON [plazas].[acta_consejo_tecnico] ([aviso_id], [estado]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_acta_consejo_tecnico__entidad_fecha_estado' AND object_id = OBJECT_ID(N'plazas.acta_consejo_tecnico'))
    CREATE INDEX [ix_acta_consejo_tecnico__entidad_fecha_estado] ON [plazas].[acta_consejo_tecnico] ([entidad_academica_id], [fecha], [estado]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_acta_oferta__aviso_oferta' AND object_id = OBJECT_ID(N'plazas.acta_oferta'))
    CREATE INDEX [ix_acta_oferta__aviso_oferta] ON [plazas].[acta_oferta] ([aviso_oferta_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_acta_oferta__aviso_oferta_activa' AND object_id = OBJECT_ID(N'plazas.acta_oferta'))
    CREATE UNIQUE INDEX [ux_acta_oferta__aviso_oferta_activa] ON [plazas].[acta_oferta] ([aviso_oferta_id]) WHERE [fecha_eliminacion] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_votacion_solicitud__solicitud' AND object_id = OBJECT_ID(N'plazas.votacion_solicitud'))
    CREATE INDEX [ix_votacion_solicitud__solicitud] ON [plazas].[votacion_solicitud] ([solicitud_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_acta_asistencia__acta' AND object_id = OBJECT_ID(N'plazas.acta_asistencia'))
    CREATE INDEX [ix_acta_asistencia__acta] ON [plazas].[acta_asistencia] ([acta_consejo_tecnico_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_documento_acta__acta' AND object_id = OBJECT_ID(N'plazas.documento_acta'))
    CREATE INDEX [ix_documento_acta__acta] ON [plazas].[documento_acta] ([acta_consejo_tecnico_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_documento_acta__vigente' AND object_id = OBJECT_ID(N'plazas.documento_acta'))
    CREATE UNIQUE INDEX [ux_documento_acta__vigente] ON [plazas].[documento_acta] ([acta_consejo_tecnico_id], [tipo]) WHERE [es_vigente] = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_documento_acta__clave_almacenamiento' AND object_id = OBJECT_ID(N'plazas.documento_acta'))
    CREATE UNIQUE INDEX [ux_documento_acta__clave_almacenamiento] ON [plazas].[documento_acta] ([clave_almacenamiento]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_revision_acta__acta' AND object_id = OBJECT_ID(N'plazas.revision_acta'))
    CREATE INDEX [ix_revision_acta__acta] ON [plazas].[revision_acta] ([acta_consejo_tecnico_id], [numero_revision]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_revision_acta__abierta' AND object_id = OBJECT_ID(N'plazas.revision_acta'))
    CREATE UNIQUE INDEX [ux_revision_acta__abierta] ON [plazas].[revision_acta] ([acta_consejo_tecnico_id]) WHERE [resuelto_en] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_asignacion_docente__programacion_fecha' AND object_id = OBJECT_ID(N'academico.asignacion_docente'))
    CREATE INDEX [ix_asignacion_docente__programacion_fecha] ON [academico].[asignacion_docente] ([programacion_academica_id], [fecha_inicio], [fecha_fin]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_asignacion_docente__docente_fecha' AND object_id = OBJECT_ID(N'academico.asignacion_docente'))
    CREATE INDEX [ix_asignacion_docente__docente_fecha] ON [academico].[asignacion_docente] ([docente_id], [fecha_inicio], [fecha_fin]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_asignacion_docente__origen_referencia' AND object_id = OBJECT_ID(N'academico.asignacion_docente'))
    CREATE INDEX [ix_asignacion_docente__origen_referencia] ON [academico].[asignacion_docente] ([origen], [sincronizacion_planea_id], [acta_oferta_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_asignacion_docente__sincronizacion_planea_id' AND object_id = OBJECT_ID(N'academico.asignacion_docente'))
    CREATE INDEX [ix_asignacion_docente__sincronizacion_planea_id] ON [academico].[asignacion_docente] ([sincronizacion_planea_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_asignacion_docente__acta_oferta_id' AND object_id = OBJECT_ID(N'academico.asignacion_docente'))
    CREATE INDEX [ix_asignacion_docente__acta_oferta_id] ON [academico].[asignacion_docente] ([acta_oferta_id]);
GO
