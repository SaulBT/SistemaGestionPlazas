SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Modelo normalizado. La migración es aditiva: dbo es el modelo legacy. */
IF SCHEMA_ID(N'academico') IS NULL EXEC(N'CREATE SCHEMA [academico]');
IF SCHEMA_ID(N'integracion') IS NULL EXEC(N'CREATE SCHEMA [integracion]');
IF SCHEMA_ID(N'usuarios') IS NULL EXEC(N'CREATE SCHEMA [usuarios]');
IF SCHEMA_ID(N'plazas') IS NULL EXEC(N'CREATE SCHEMA [plazas]');
GO

IF OBJECT_ID(N'academico.region', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[region]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] int NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_region] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_region__clave] UNIQUE ([clave]),
        CONSTRAINT [ck_region__clave_positiva] CHECK ([clave] > 0),
        CONSTRAINT [ck_region__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.campus', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[campus]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [region_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_campus] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_campus__clave] UNIQUE ([clave]),
        CONSTRAINT [fk_campus__region] FOREIGN KEY ([region_id]) REFERENCES [academico].[region] ([id]),
        CONSTRAINT [ck_campus__clave_formato] CHECK
            ([clave] = UPPER(LTRIM(RTRIM([clave]))) AND [clave] NOT LIKE '%[^A-Z0-9]%' AND LEN(LTRIM(RTRIM([clave]))) > 0),
        CONSTRAINT [ck_campus__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.area_academica', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[area_academica]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] int NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_area_academica] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_area_academica__clave] UNIQUE ([clave]),
        CONSTRAINT [ck_area_academica__clave_positiva] CHECK ([clave] > 0),
        CONSTRAINT [ck_area_academica__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.municipio', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[municipio]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_municipio] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_municipio__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_municipio__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.sistema_educativo', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[sistema_educativo]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_sistema_educativo] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_sistema_educativo__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_sistema_educativo__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.nivel_formacion', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[nivel_formacion]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_nivel_formacion] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_nivel_formacion__clave] UNIQUE ([clave]),
        CONSTRAINT [uq_nivel_formacion__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_nivel_formacion__clave_formato] CHECK
            ([clave] = UPPER(LTRIM(RTRIM([clave]))) AND [clave] NOT LIKE '%[^A-Z0-9]%' AND LEN(LTRIM(RTRIM([clave]))) > 0),
        CONSTRAINT [ck_nivel_formacion__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.area_formacion', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[area_formacion]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_area_formacion] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_area_formacion__clave] UNIQUE ([clave]),
        CONSTRAINT [ck_area_formacion__clave_formato] CHECK
            ([clave] = UPPER(LTRIM(RTRIM([clave]))) AND [clave] NOT LIKE '%[^A-Z0-9]%' AND LEN(LTRIM(RTRIM([clave]))) > 0),
        CONSTRAINT [ck_area_formacion__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'usuarios.rol', N'U') IS NULL
BEGIN
    CREATE TABLE [usuarios].[rol]
    (
        [id] tinyint NOT NULL,
        [nombre] nvarchar(100) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_rol] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_rol__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_rol__catalogo_fijo] CHECK
            (([id] = 1 AND [nombre] = N'Superusuario')
             OR ([id] = 2 AND [nombre] = N'DGAA')
             OR ([id] = 3 AND [nombre] = N'Entidad Académica'))
    );
END
GO

IF OBJECT_ID(N'plazas.grado_academico', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[grado_academico]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_grado_academico] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_grado_academico__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_grado_academico__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.tratamiento_academico', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[tratamiento_academico]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(30) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [grado_academico_id] int NOT NULL,
        CONSTRAINT [pk_tratamiento_academico] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_tratamiento_academico__nombre] UNIQUE ([nombre]),
        CONSTRAINT [fk_tratamiento_academico__grado_academico]
            FOREIGN KEY ([grado_academico_id]) REFERENCES [plazas].[grado_academico] ([id]),
        CONSTRAINT [ck_tratamiento_academico__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.articulo', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[articulo]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [numero] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [descripcion] nvarchar(1000) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_articulo] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_articulo__numero] UNIQUE ([numero]),
        CONSTRAINT [ck_articulo__numero_no_vacio] CHECK (LEN(LTRIM(RTRIM([numero]))) > 0),
        CONSTRAINT [ck_articulo__numero_normalizado] CHECK ([numero] = LTRIM(RTRIM([numero])))
    );
END
GO

IF OBJECT_ID(N'plazas.modalidad_recepcion', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[modalidad_recepcion]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(100) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [requiere_lugar] bit NOT NULL,
        CONSTRAINT [pk_modalidad_recepcion] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_modalidad_recepcion__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_modalidad_recepcion__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.tipo_plaza', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[tipo_plaza]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_tipo_plaza] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_tipo_plaza__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_tipo_plaza__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.tipo_contratacion', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[tipo_contratacion]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_tipo_contratacion] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_tipo_contratacion__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_tipo_contratacion__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'plazas.tipo_documento_aspirante', N'U') IS NULL
BEGIN
    CREATE TABLE [plazas].[tipo_documento_aspirante]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        CONSTRAINT [pk_tipo_documento_aspirante] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_tipo_documento_aspirante__nombre] UNIQUE ([nombre]),
        CONSTRAINT [ck_tipo_documento_aspirante__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_campus__region_id' AND object_id = OBJECT_ID(N'academico.campus'))
    CREATE INDEX [ix_campus__region_id] ON [academico].[campus] ([region_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_tratamiento_academico__grado_academico_id' AND object_id = OBJECT_ID(N'plazas.tratamiento_academico'))
    CREATE INDEX [ix_tratamiento_academico__grado_academico_id] ON [plazas].[tratamiento_academico] ([grado_academico_id]);
GO
