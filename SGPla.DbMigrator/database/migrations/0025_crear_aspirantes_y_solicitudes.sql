SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'plazas.aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        CONSTRAINT [pk_aspirante] PRIMARY KEY CLUSTERED ([id])
    );
END
GO

IF OBJECT_ID(N'plazas.perfil_aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[perfil_aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aspirante_id] int NOT NULL,
        [numero_version] int NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [correo] varchar(254) COLLATE Latin1_General_100_CI_AI NOT NULL,
        [puesto_actual] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NULL,
        [descripcion_perfil] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [es_vigente] bit NOT NULL,
        [creado_en] datetime2(0) NOT NULL CONSTRAINT [df_perfil_aspirante__creado_en] DEFAULT (SYSUTCDATETIME()),
        [creado_por_usuario_id] int NOT NULL,
        CONSTRAINT [pk_perfil_aspirante] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_perfil_aspirante__aspirante] FOREIGN KEY ([aspirante_id]) REFERENCES [plazas].[aspirante] ([id]),
        CONSTRAINT [fk_perfil_aspirante__usuario] FOREIGN KEY ([creado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_perfil_aspirante__version] UNIQUE ([aspirante_id], [numero_version]),
        CONSTRAINT [ck_perfil_aspirante__version] CHECK ([numero_version] > 0),
        CONSTRAINT [ck_perfil_aspirante__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_perfil_aspirante__correo] CHECK (([correo] COLLATE Latin1_General_100_BIN2) = (LOWER(LTRIM(RTRIM([correo]))) COLLATE Latin1_General_100_BIN2) AND [correo] NOT LIKE '% %' AND [correo] LIKE '%_@_%._%' AND LEN(LTRIM(RTRIM([correo]))) BETWEEN 3 AND 254),
        CONSTRAINT [ck_perfil_aspirante__puesto] CHECK ([puesto_actual] IS NULL OR LEN(LTRIM(RTRIM([puesto_actual]))) > 0),
        CONSTRAINT [ck_perfil_aspirante__descripcion] CHECK (LEN(LTRIM(RTRIM([descripcion_perfil]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.formacion_aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[formacion_aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [perfil_aspirante_id] int NOT NULL,
        [grado_academico_id] int NOT NULL,
        [descripcion] nvarchar(500) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_formacion_aspirante] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_formacion_aspirante__perfil] FOREIGN KEY ([perfil_aspirante_id]) REFERENCES [plazas].[perfil_aspirante] ([id]),
        CONSTRAINT [fk_formacion_aspirante__grado] FOREIGN KEY ([grado_academico_id]) REFERENCES [plazas].[grado_academico] ([id]),
        CONSTRAINT [ck_formacion_aspirante__descripcion] CHECK (LEN(LTRIM(RTRIM([descripcion]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.documento_aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[documento_aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aspirante_id] int NOT NULL,
        [tipo_documento_id] int NOT NULL,
        CONSTRAINT [pk_documento_aspirante] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_documento_aspirante__aspirante] FOREIGN KEY ([aspirante_id]) REFERENCES [plazas].[aspirante] ([id]),
        CONSTRAINT [fk_documento_aspirante__tipo] FOREIGN KEY ([tipo_documento_id]) REFERENCES [plazas].[tipo_documento_aspirante] ([id])
    );
END
GO

IF OBJECT_ID(N'plazas.version_documento_aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[version_documento_aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [documento_aspirante_id] int NOT NULL,
        [nombre] nvarchar(260) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [mime] varchar(255) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [tamano] bigint NOT NULL,
        [checksum_sha256] binary(32) NOT NULL,
        [clave_almacenamiento] nvarchar(500) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [numero_version] int NOT NULL,
        [es_vigente] bit NOT NULL,
        [cargado_en] datetime2(0) NOT NULL CONSTRAINT [df_version_documento_aspirante__cargado_en] DEFAULT (SYSUTCDATETIME()),
        [cargado_por_usuario_id] int NOT NULL,
        CONSTRAINT [pk_version_documento_aspirante] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_version_documento_aspirante__documento] FOREIGN KEY ([documento_aspirante_id]) REFERENCES [plazas].[documento_aspirante] ([id]),
        CONSTRAINT [fk_version_documento_aspirante__usuario] FOREIGN KEY ([cargado_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_version_documento_aspirante__version] UNIQUE ([documento_aspirante_id], [numero_version]),
        CONSTRAINT [ck_version_documento_aspirante__nombre] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_version_documento_aspirante__tamano] CHECK ([tamano] > 0),
        CONSTRAINT [ck_version_documento_aspirante__version] CHECK ([numero_version] > 0),
        CONSTRAINT [ck_version_documento_aspirante__clave] CHECK (LEN(LTRIM(RTRIM([clave_almacenamiento]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.solicitud', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[solicitud]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [aviso_oferta_id] int NOT NULL,
        [aspirante_id] int NOT NULL,
        [perfil_aspirante_id] int NOT NULL,
        [registrada_en] datetime2(0) NOT NULL CONSTRAINT [df_solicitud__registrada_en] DEFAULT (SYSUTCDATETIME()),
        [estado] varchar(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [observaciones] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        [admitida_en] datetime2(0) NULL,
        [no_admitida_en] datetime2(0) NULL,
        [retirada_en] datetime2(0) NULL,
        [admitida_por_usuario_id] int NULL,
        [no_admitida_por_usuario_id] int NULL,
        [motivo_no_admision] nvarchar(1000) COLLATE Modern_Spanish_100_CI_AI NULL,
        [motivo_retiro] nvarchar(1000) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_solicitud] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_solicitud__aviso_oferta] FOREIGN KEY ([aviso_oferta_id]) REFERENCES [plazas].[aviso_oferta] ([id]),
        CONSTRAINT [fk_solicitud__aspirante] FOREIGN KEY ([aspirante_id]) REFERENCES [plazas].[aspirante] ([id]),
        CONSTRAINT [fk_solicitud__perfil] FOREIGN KEY ([perfil_aspirante_id]) REFERENCES [plazas].[perfil_aspirante] ([id]),
        CONSTRAINT [fk_solicitud__admitida_por] FOREIGN KEY ([admitida_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_solicitud__no_admitida_por] FOREIGN KEY ([no_admitida_por_usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [uq_solicitud__aviso_oferta_aspirante] UNIQUE ([aviso_oferta_id], [aspirante_id]),
        CONSTRAINT [ck_solicitud__estado] CHECK ([estado] IN ('REGISTRADA', 'ADMITIDA', 'NO_ADMITIDA', 'RETIRADA')),
        CONSTRAINT [ck_solicitud__observaciones] CHECK ([observaciones] IS NULL OR LEN(LTRIM(RTRIM([observaciones]))) > 0),
        CONSTRAINT [ck_solicitud__resultado] CHECK
            (([estado] = 'ADMITIDA' AND [admitida_en] IS NOT NULL AND [admitida_por_usuario_id] IS NOT NULL AND [no_admitida_en] IS NULL AND [no_admitida_por_usuario_id] IS NULL AND [retirada_en] IS NULL AND [motivo_no_admision] IS NULL AND [motivo_retiro] IS NULL)
             OR ([estado] = 'NO_ADMITIDA' AND [no_admitida_en] IS NOT NULL AND [no_admitida_por_usuario_id] IS NOT NULL AND [motivo_no_admision] IS NOT NULL AND [admitida_en] IS NULL AND [admitida_por_usuario_id] IS NULL AND [retirada_en] IS NULL AND [motivo_retiro] IS NULL)
             OR ([estado] = 'RETIRADA' AND [retirada_en] IS NOT NULL AND [no_admitida_en] IS NULL AND [no_admitida_por_usuario_id] IS NULL AND [motivo_no_admision] IS NULL AND (([admitida_en] IS NULL AND [admitida_por_usuario_id] IS NULL) OR ([admitida_en] IS NOT NULL AND [admitida_por_usuario_id] IS NOT NULL)))
             OR ([estado] = 'REGISTRADA' AND [admitida_en] IS NULL AND [admitida_por_usuario_id] IS NULL AND [no_admitida_en] IS NULL AND [no_admitida_por_usuario_id] IS NULL AND [retirada_en] IS NULL AND [motivo_no_admision] IS NULL AND [motivo_retiro] IS NULL)),
        CONSTRAINT [ck_solicitud__motivo] CHECK ([motivo_no_admision] IS NULL OR LEN(LTRIM(RTRIM([motivo_no_admision]))) > 0),
        CONSTRAINT [ck_solicitud__motivo_retiro] CHECK ([motivo_retiro] IS NULL OR LEN(LTRIM(RTRIM([motivo_retiro]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.solicitud_documento', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[solicitud_documento]
    (
        [solicitud_id] int NOT NULL,
        [version_documento_aspirante_id] int NOT NULL,
        CONSTRAINT [pk_solicitud_documento] PRIMARY KEY CLUSTERED ([solicitud_id], [version_documento_aspirante_id]),
        CONSTRAINT [fk_solicitud_documento__solicitud] FOREIGN KEY ([solicitud_id]) REFERENCES [plazas].[solicitud] ([id]),
        CONSTRAINT [fk_solicitud_documento__version] FOREIGN KEY ([version_documento_aspirante_id]) REFERENCES [plazas].[version_documento_aspirante] ([id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_perfil_aspirante__vigente' AND object_id = OBJECT_ID(N'plazas.perfil_aspirante'))
    CREATE UNIQUE INDEX [ux_perfil_aspirante__vigente] ON [plazas].[perfil_aspirante] ([aspirante_id]) WHERE [es_vigente] = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_perfil_aspirante__correo' AND object_id = OBJECT_ID(N'plazas.perfil_aspirante'))
    CREATE INDEX [ix_perfil_aspirante__correo] ON [plazas].[perfil_aspirante] ([correo]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_perfil_aspirante__correo_vigente' AND object_id = OBJECT_ID(N'plazas.perfil_aspirante'))
    CREATE UNIQUE INDEX [ux_perfil_aspirante__correo_vigente] ON [plazas].[perfil_aspirante] ([correo]) WHERE [es_vigente] = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_formacion_aspirante__perfil' AND object_id = OBJECT_ID(N'plazas.formacion_aspirante'))
    CREATE INDEX [ix_formacion_aspirante__perfil] ON [plazas].[formacion_aspirante] ([perfil_aspirante_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_documento_aspirante__aspirante' AND object_id = OBJECT_ID(N'plazas.documento_aspirante'))
    CREATE INDEX [ix_documento_aspirante__aspirante] ON [plazas].[documento_aspirante] ([aspirante_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_version_documento_aspirante__vigente' AND object_id = OBJECT_ID(N'plazas.version_documento_aspirante'))
    CREATE UNIQUE INDEX [ux_version_documento_aspirante__vigente] ON [plazas].[version_documento_aspirante] ([documento_aspirante_id]) WHERE [es_vigente] = 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_version_documento_aspirante__clave_almacenamiento' AND object_id = OBJECT_ID(N'plazas.version_documento_aspirante'))
    CREATE UNIQUE INDEX [ux_version_documento_aspirante__clave_almacenamiento] ON [plazas].[version_documento_aspirante] ([clave_almacenamiento]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_solicitud__aspirante_estado' AND object_id = OBJECT_ID(N'plazas.solicitud'))
    CREATE INDEX [ix_solicitud__aspirante_estado] ON [plazas].[solicitud] ([aspirante_id], [estado]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_solicitud__aviso_estado' AND object_id = OBJECT_ID(N'plazas.solicitud'))
    CREATE INDEX [ix_solicitud__aviso_estado] ON [plazas].[solicitud] ([aviso_oferta_id], [estado]);
GO
