SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'usuarios.usuario', N'U') IS NULL
BEGIN
    CREATE TABLE [usuarios].[usuario]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [correo] varchar(254) COLLATE Latin1_General_100_CI_AI NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [rol_id] tinyint NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_usuario] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_usuario__rol] FOREIGN KEY ([rol_id]) REFERENCES [usuarios].[rol] ([id]),
        CONSTRAINT [ck_usuario__correo_formato] CHECK
            (([correo] COLLATE Latin1_General_100_BIN2) = (LOWER(LTRIM(RTRIM([correo]))) COLLATE Latin1_General_100_BIN2) AND [correo] NOT LIKE '% %' AND [correo] LIKE '%_@_%._%' AND LEN(LTRIM(RTRIM([correo]))) BETWEEN 3 AND 254),
        CONSTRAINT [ck_usuario__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'usuarios.usuario_dgaa', N'U') IS NULL
BEGIN
    CREATE TABLE [usuarios].[usuario_dgaa]
    (
        [usuario_id] int NOT NULL,
        [area_academica_id] int NOT NULL,
        CONSTRAINT [pk_usuario_dgaa] PRIMARY KEY CLUSTERED ([usuario_id]),
        CONSTRAINT [fk_usuario_dgaa__usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_usuario_dgaa__area_academica] FOREIGN KEY ([area_academica_id]) REFERENCES [academico].[area_academica] ([id])
    );
END
GO

IF OBJECT_ID(N'usuarios.usuario_entidad_academica', N'U') IS NULL
BEGIN
    CREATE TABLE [usuarios].[usuario_entidad_academica]
    (
        [usuario_id] int NOT NULL,
        [entidad_academica_id] int NOT NULL,
        CONSTRAINT [pk_usuario_entidad_academica] PRIMARY KEY CLUSTERED ([usuario_id]),
        CONSTRAINT [fk_usuario_entidad_academica__usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [fk_usuario_entidad_academica__entidad] FOREIGN KEY ([entidad_academica_id]) REFERENCES [academico].[entidad_academica] ([id])
    );
END
GO

IF OBJECT_ID(N'usuarios.credencial_superusuario', N'U') IS NULL
BEGIN
    CREATE TABLE [usuarios].[credencial_superusuario]
    (
        [usuario_id] int NOT NULL,
        [contrasena] varchar(500) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [fecha_actualizacion] datetime2(0) NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_credencial_superusuario] PRIMARY KEY CLUSTERED ([usuario_id]),
        CONSTRAINT [fk_credencial_superusuario__usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuarios].[usuario] ([id]),
        CONSTRAINT [ck_credencial_superusuario__contrasena_no_vacia] CHECK (LEN(LTRIM(RTRIM([contrasena]))) > 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_usuario__correo_activo' AND object_id = OBJECT_ID(N'usuarios.usuario'))
    CREATE UNIQUE INDEX [ux_usuario__correo_activo] ON [usuarios].[usuario] ([correo]) WHERE [fecha_eliminacion] IS NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_usuario__rol_id' AND object_id = OBJECT_ID(N'usuarios.usuario'))
    CREATE INDEX [ix_usuario__rol_id] ON [usuarios].[usuario] ([rol_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_usuario_dgaa__area_academica_id' AND object_id = OBJECT_ID(N'usuarios.usuario_dgaa'))
    CREATE INDEX [ix_usuario_dgaa__area_academica_id] ON [usuarios].[usuario_dgaa] ([area_academica_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_usuario_entidad_academica__entidad_academica_id' AND object_id = OBJECT_ID(N'usuarios.usuario_entidad_academica'))
    CREATE INDEX [ix_usuario_entidad_academica__entidad_academica_id] ON [usuarios].[usuario_entidad_academica] ([entidad_academica_id]);
GO
