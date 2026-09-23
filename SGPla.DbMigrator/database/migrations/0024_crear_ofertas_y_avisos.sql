SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'plazas.integrante_consejo_tecnico', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[integrante_consejo_tecnico]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [entidad_academica_id] int NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [cargo] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [tratamiento_academico_id] int NOT NULL,
        [fecha_inicio] date NOT NULL,
        [fecha_fin] date NULL,
        CONSTRAINT [pk_integrante_consejo_tecnico] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_integrante_consejo_tecnico__entidad] FOREIGN KEY ([entidad_academica_id]) REFERENCES [academico].[entidad_academica] ([id]),
        CONSTRAINT [fk_integrante_consejo_tecnico__tratamiento] FOREIGN KEY ([tratamiento_academico_id]) REFERENCES [plazas].[tratamiento_academico] ([id]),
        CONSTRAINT [ck_integrante_consejo_tecnico__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_integrante_consejo_tecnico__cargo] CHECK (LEN(LTRIM(RTRIM([cargo]))) > 0),
        CONSTRAINT [ck_integrante_consejo_tecnico__fechas] CHECK ([fecha_fin] IS NULL OR [fecha_inicio] <= [fecha_fin])
    );
END
GO

IF OBJECT_ID(N'plazas.oferta', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[oferta]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [programacion_academica_id] int NOT NULL,
        [clave_plaza] varchar(100) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [tipo_plaza_id] int NOT NULL,
        [tipo_contratacion_id] int NOT NULL,
        [perfil_solicitado] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [justificacion] nvarchar(1000) COLLATE Modern_Spanish_100_CI_AI NULL,
        [estado] varchar(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [cerrada_en] datetime2(0) NULL,
        CONSTRAINT [pk_oferta] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_oferta__programacion] FOREIGN KEY ([programacion_academica_id]) REFERENCES [academico].[programacion_academica] ([id]),
        CONSTRAINT [fk_oferta__tipo_plaza] FOREIGN KEY ([tipo_plaza_id]) REFERENCES [plazas].[tipo_plaza] ([id]),
        CONSTRAINT [fk_oferta__tipo_contratacion] FOREIGN KEY ([tipo_contratacion_id]) REFERENCES [plazas].[tipo_contratacion] ([id]),
        CONSTRAINT [ck_oferta__clave_formato] CHECK ([clave_plaza] = UPPER(LTRIM(RTRIM([clave_plaza]))) AND [clave_plaza] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([clave_plaza]))) > 0),
        CONSTRAINT [ck_oferta__perfil_no_vacio] CHECK (LEN(LTRIM(RTRIM([perfil_solicitado]))) > 0),
        CONSTRAINT [ck_oferta__justificacion] CHECK ([justificacion] IS NULL OR LEN(LTRIM(RTRIM([justificacion]))) > 0),
        CONSTRAINT [ck_oferta__estado] CHECK ([estado] IN ('DISPONIBLE', 'EN_PUBLICACION', 'CUBIERTA')),
        CONSTRAINT [ck_oferta__cierre] CHECK (([estado] = 'CUBIERTA' AND [cerrada_en] IS NOT NULL) OR ([estado] <> 'CUBIERTA' AND [cerrada_en] IS NULL))
    );
END
GO

IF OBJECT_ID(N'plazas.aviso', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[aviso]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [entidad_academica_id] int NOT NULL,
        [periodo_escolar_id] int NOT NULL,
        [sistema_educativo_id] int NOT NULL,
        [articulo_id] int NOT NULL,
        [tipo_comunicado] varchar(15) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [modalidad_recepcion_id] int NULL,
        [requisitos] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        [lugar_recepcion] nvarchar(500) COLLATE Modern_Spanish_100_CI_AI NULL,
        [correo_contacto] varchar(254) COLLATE Latin1_General_100_CI_AI NULL,
        [nombre_titular] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NULL,
        [creado_en] datetime2(0) NOT NULL CONSTRAINT [df_aviso__creado_en] DEFAULT (SYSUTCDATETIME()),
        [fecha_publicacion] date NULL,
        [fecha_consejo_tecnico] date NULL,
        [fecha_vacantes] date NULL,
        [url_publicacion] varchar(2048) COLLATE Latin1_General_100_BIN2 NULL,
        [estado] varchar(30) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [cancelado_en] datetime2(0) NULL,
        [cancelado_por_usuario_id] int NULL,
        [motivo_cancelacion] nvarchar(1000) COLLATE Modern_Spanish_100_CI_AI NULL,
        [archivado_en] datetime2(0) NULL,
        [archivado_por_usuario_id] int NULL,
        CONSTRAINT [pk_aviso] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_aviso__entidad] FOREIGN KEY ([entidad_academica_id]) REFERENCES [academico].[entidad_academica] ([id]),
        CONSTRAINT [fk_aviso__periodo] FOREIGN KEY ([periodo_escolar_id]) REFERENCES [academico].[periodo_escolar] ([id]),
        CONSTRAINT [fk_aviso__sistema] FOREIGN KEY ([sistema_educativo_id]) REFERENCES [academico].[sistema_educativo] ([id]),
        CONSTRAINT [fk_aviso__articulo] FOREIGN KEY ([articulo_id]) REFERENCES [plazas].[articulo] ([id]),
        CONSTRAINT [fk_aviso__modalidad] FOREIGN KEY ([modalidad_recepcion_id]) REFERENCES [plazas].[modalidad_recepcion] ([id]),
        CONSTRAINT [fk_aviso__cancelado_por] FOREIGN KEY ([cancelado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_aviso__archivado_por] FOREIGN KEY ([archivado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [ck_aviso__tipo_comunicado] CHECK ([tipo_comunicado] IN ('AVISO', 'CONVOCATORIA')),
        CONSTRAINT [ck_aviso__estado] CHECK ([estado] IN ('CREADO', 'EN_REVISION_DGAA', 'DEVUELTO_DGAA', 'AVALADO_DGAA', 'FIRMADO', 'PUBLICADO', 'CANCELADO')),
        CONSTRAINT [ck_aviso__requisitos] CHECK ([requisitos] IS NULL OR LEN(LTRIM(RTRIM([requisitos]))) > 0),
        CONSTRAINT [ck_aviso__lugar_recepcion] CHECK ([lugar_recepcion] IS NULL OR LEN(LTRIM(RTRIM([lugar_recepcion]))) > 0),
        CONSTRAINT [ck_aviso__correo_contacto] CHECK ([correo_contacto] IS NULL OR ([correo_contacto] LIKE '%_@_%._%' AND [correo_contacto] NOT LIKE '% %')),
        CONSTRAINT [ck_aviso__nombre_titular] CHECK ([nombre_titular] IS NULL OR LEN(LTRIM(RTRIM([nombre_titular]))) > 0),
        CONSTRAINT [ck_aviso__url] CHECK ([url_publicacion] IS NULL OR [url_publicacion] LIKE 'http://%' OR [url_publicacion] LIKE 'https://%'),
        CONSTRAINT [ck_aviso__cancelacion] CHECK (([estado] = 'CANCELADO' AND [cancelado_en] IS NOT NULL AND [cancelado_por_usuario_id] IS NOT NULL AND [motivo_cancelacion] IS NOT NULL AND LEN(LTRIM(RTRIM([motivo_cancelacion]))) > 0) OR ([estado] <> 'CANCELADO' AND [cancelado_en] IS NULL AND [cancelado_por_usuario_id] IS NULL AND [motivo_cancelacion] IS NULL)),
        CONSTRAINT [ck_aviso__archivo] CHECK (([archivado_en] IS NULL AND [archivado_por_usuario_id] IS NULL) OR ([archivado_en] IS NOT NULL AND [archivado_por_usuario_id] IS NOT NULL))
    );
END
GO

IF OBJECT_ID(N'plazas.horario_recepcion_requisito', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[horario_recepcion_requisito]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_id] int NOT NULL,
        [fecha] date NOT NULL,
        [hora_inicio] time(0) NOT NULL,
        [hora_fin] time(0) NOT NULL,
        CONSTRAINT [pk_horario_recepcion_requisito] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_horario_recepcion_requisito__aviso] FOREIGN KEY ([aviso_id]) REFERENCES [plazas].[aviso] ([id]),
        CONSTRAINT [uq_horario_recepcion_requisito__clave] UNIQUE ([aviso_id], [fecha], [hora_inicio], [hora_fin]),
        CONSTRAINT [ck_horario_recepcion_requisito__horas] CHECK ([hora_inicio] < [hora_fin])
    );
END
GO

IF OBJECT_ID(N'plazas.aviso_oferta', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[aviso_oferta]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_id] int NOT NULL,
        [oferta_id] int NOT NULL,
        [incorporado_en] datetime2(0) NOT NULL CONSTRAINT [df_aviso_oferta__incorporado_en] DEFAULT (SYSUTCDATETIME()),
        [cerrado_en] datetime2(0) NULL,
        [causa_cierre] varchar(20) COLLATE Latin1_General_100_BIN2 NULL,
        CONSTRAINT [pk_aviso_oferta] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_aviso_oferta__aviso] FOREIGN KEY ([aviso_id]) REFERENCES [plazas].[aviso] ([id]),
        CONSTRAINT [fk_aviso_oferta__oferta] FOREIGN KEY ([oferta_id]) REFERENCES [plazas].[oferta] ([id]),
        CONSTRAINT [uq_aviso_oferta__aviso_oferta] UNIQUE ([aviso_id], [oferta_id]),
        CONSTRAINT [ck_aviso_oferta__cierre] CHECK (([cerrado_en] IS NULL AND [causa_cierre] IS NULL) OR ([cerrado_en] IS NOT NULL AND [causa_cierre] IN ('DESIGNADA', 'DESIERTA', 'SIN_ASPIRANTES', 'CANCELADO')))
    );
END
GO

IF OBJECT_ID(N'plazas.documento_aviso', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[documento_aviso]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_id] int NOT NULL,
        [tipo] varchar(10) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(260) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [mime] varchar(255) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [tamano] bigint NOT NULL,
        [checksum_sha256] binary(32) NOT NULL,
        [clave_almacenamiento] nvarchar(500) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [numero_version] int NOT NULL,
        [es_vigente] bit NOT NULL,
        [cargado_en] datetime2(0) NOT NULL CONSTRAINT [df_documento_aviso__cargado_en] DEFAULT (SYSUTCDATETIME()),
        [cargado_por_usuario_id] int NOT NULL,
        CONSTRAINT [pk_documento_aviso] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_documento_aviso__aviso] FOREIGN KEY ([aviso_id]) REFERENCES [plazas].[aviso] ([id]),
        CONSTRAINT [fk_documento_aviso__usuario] FOREIGN KEY ([cargado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_documento_aviso__version] UNIQUE ([aviso_id], [tipo], [numero_version]),
        CONSTRAINT [ck_documento_aviso__tipo] CHECK ([tipo] IN ('ORIGINAL', 'FIRMADO')),
        CONSTRAINT [ck_documento_aviso__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_documento_aviso__tamano] CHECK ([tamano] > 0),
        CONSTRAINT [ck_documento_aviso__version] CHECK ([numero_version] > 0),
        CONSTRAINT [ck_documento_aviso__clave] CHECK (LEN(LTRIM(RTRIM([clave_almacenamiento]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.revision_aviso', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[revision_aviso]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_id] int NOT NULL,
        [numero_revision] int NOT NULL,
        [documento_original_id] int NOT NULL,
        [enviado_por_usuario_id] int NOT NULL,
        [enviado_en] datetime2(0) NOT NULL CONSTRAINT [df_revision_aviso__enviado_en] DEFAULT (SYSUTCDATETIME()),
        [resuelto_por_usuario_id] int NULL,
        [resuelto_en] datetime2(0) NULL,
        [resultado] varchar(10) COLLATE Latin1_General_100_BIN2 NULL,
        [comentarios] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_revision_aviso] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_revision_aviso__aviso] FOREIGN KEY ([aviso_id]) REFERENCES [plazas].[aviso] ([id]),
        CONSTRAINT [fk_revision_aviso__documento] FOREIGN KEY ([documento_original_id]) REFERENCES [plazas].[documento_aviso] ([id]),
        CONSTRAINT [fk_revision_aviso__enviado_por] FOREIGN KEY ([enviado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_revision_aviso__resuelto_por] FOREIGN KEY ([resuelto_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_revision_aviso__numero] UNIQUE ([aviso_id], [numero_revision]),
        CONSTRAINT [ck_revision_aviso__numero] CHECK ([numero_revision] > 0),
        CONSTRAINT [ck_revision_aviso__resultado] CHECK ([resultado] IS NULL OR [resultado] IN ('AVALADA', 'DEVUELTA')),
        CONSTRAINT [ck_revision_aviso__resolucion] CHECK (([resuelto_en] IS NULL AND [resuelto_por_usuario_id] IS NULL AND [resultado] IS NULL) OR ([resuelto_en] IS NOT NULL AND [resuelto_por_usuario_id] IS NOT NULL AND [resultado] IS NOT NULL)),
        CONSTRAINT [ck_revision_aviso__devolucion] CHECK ([resultado] <> 'DEVUELTA' OR ([comentarios] IS NOT NULL AND LEN(LTRIM(RTRIM([comentarios]))) > 0))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_oferta__programacion_clave_activa' AND object_id = OBJECT_ID(N'plazas.oferta'))
    CREATE UNIQUE INDEX [ux_oferta__programacion_clave_activa] ON [plazas].[oferta] ([programacion_academica_id], [clave_plaza]) WHERE [cerrada_en] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_oferta__tipo' AND object_id = OBJECT_ID(N'plazas.oferta'))
    CREATE INDEX [ix_oferta__tipo] ON [plazas].[oferta] ([tipo_plaza_id], [tipo_contratacion_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_aviso__entidad_periodo_estado' AND object_id = OBJECT_ID(N'plazas.aviso'))
    CREATE INDEX [ix_aviso__entidad_periodo_estado] ON [plazas].[aviso] ([entidad_academica_id], [periodo_escolar_id], [estado]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_aviso__sistema_articulo' AND object_id = OBJECT_ID(N'plazas.aviso'))
    CREATE INDEX [ix_aviso__sistema_articulo] ON [plazas].[aviso] ([sistema_educativo_id], [articulo_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_horario_recepcion_requisito__aviso_fecha' AND object_id = OBJECT_ID(N'plazas.horario_recepcion_requisito'))
    CREATE INDEX [ix_horario_recepcion_requisito__aviso_fecha] ON [plazas].[horario_recepcion_requisito] ([aviso_id], [fecha]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_aviso_oferta__oferta_abierta' AND object_id = OBJECT_ID(N'plazas.aviso_oferta'))
    CREATE UNIQUE INDEX [ux_aviso_oferta__oferta_abierta] ON [plazas].[aviso_oferta] ([oferta_id]) WHERE [cerrado_en] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_documento_aviso__aviso' AND object_id = OBJECT_ID(N'plazas.documento_aviso'))
    CREATE INDEX [ix_documento_aviso__aviso] ON [plazas].[documento_aviso] ([aviso_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_documento_aviso__vigente' AND object_id = OBJECT_ID(N'plazas.documento_aviso'))
    CREATE UNIQUE INDEX [ux_documento_aviso__vigente] ON [plazas].[documento_aviso] ([aviso_id], [tipo]) WHERE [es_vigente] = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_documento_aviso__clave_almacenamiento' AND object_id = OBJECT_ID(N'plazas.documento_aviso'))
    CREATE UNIQUE INDEX [ux_documento_aviso__clave_almacenamiento] ON [plazas].[documento_aviso] ([clave_almacenamiento]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_revision_aviso__aviso' AND object_id = OBJECT_ID(N'plazas.revision_aviso'))
    CREATE INDEX [ix_revision_aviso__aviso] ON [plazas].[revision_aviso] ([aviso_id], [numero_revision]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_revision_aviso__abierta' AND object_id = OBJECT_ID(N'plazas.revision_aviso'))
    CREATE UNIQUE INDEX [ux_revision_aviso__abierta] ON [plazas].[revision_aviso] ([aviso_id]) WHERE [resuelto_en] IS NULL;
GO
