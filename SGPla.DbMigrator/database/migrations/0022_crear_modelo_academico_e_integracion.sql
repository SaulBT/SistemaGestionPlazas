SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'academico.entidad_academica', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[entidad_academica]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [calle] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [numero_exterior] nvarchar(20) COLLATE Modern_Spanish_100_CI_AI NULL,
        [colonia] nvarchar(150) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [codigo_postal] char(5) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [telefono] char(10) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [extension] varchar(10) COLLATE Latin1_General_100_BIN2 NULL,
        [campus_id] int NOT NULL,
        [area_academica_id] int NOT NULL,
        [municipio_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_entidad_academica] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_entidad_academica__clave] UNIQUE ([clave]),
        CONSTRAINT [fk_entidad_academica__campus] FOREIGN KEY ([campus_id]) REFERENCES [academico].[campus] ([id]),
        CONSTRAINT [fk_entidad_academica__area_academica] FOREIGN KEY ([area_academica_id]) REFERENCES [academico].[area_academica] ([id]),
        CONSTRAINT [fk_entidad_academica__municipio] FOREIGN KEY ([municipio_id]) REFERENCES [academico].[municipio] ([id]),
        CONSTRAINT [ck_entidad_academica__clave_formato] CHECK
            ([clave] = UPPER(LTRIM(RTRIM([clave]))) AND [clave] NOT LIKE '%[^A-Z0-9]%' AND LEN(LTRIM(RTRIM([clave]))) > 0),
        CONSTRAINT [ck_entidad_academica__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_entidad_academica__calle_no_vacia] CHECK (LEN(LTRIM(RTRIM([calle]))) > 0),
        CONSTRAINT [ck_entidad_academica__numero_exterior] CHECK ([numero_exterior] IS NULL OR LEN(LTRIM(RTRIM([numero_exterior]))) > 0),
        CONSTRAINT [ck_entidad_academica__colonia_no_vacia] CHECK (LEN(LTRIM(RTRIM([colonia]))) > 0),
        CONSTRAINT [ck_entidad_academica__codigo_postal_formato] CHECK ([codigo_postal] NOT LIKE '%[^0-9]%' AND LEN([codigo_postal]) = 5),
        CONSTRAINT [ck_entidad_academica__telefono_formato] CHECK ([telefono] NOT LIKE '%[^0-9]%' AND LEN([telefono]) = 10),
        CONSTRAINT [ck_entidad_academica__extension_formato] CHECK ([extension] IS NULL OR ([extension] NOT LIKE '%[^0-9]%' AND LEN([extension]) BETWEEN 1 AND 10)),
        CONSTRAINT [ck_entidad_academica__direccion_no_vacia] CHECK (LEN(LTRIM(RTRIM([calle]))) > 0 AND LEN(LTRIM(RTRIM([colonia]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.programa_educativo', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[programa_educativo]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [entidad_academica_id] int NOT NULL,
        [sistema_educativo_id] int NOT NULL,
        [nivel_formacion_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_programa_educativo] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_programa_educativo__entidad_nombre_sistema] UNIQUE ([entidad_academica_id], [nombre], [sistema_educativo_id]),
        CONSTRAINT [fk_programa_educativo__entidad_academica] FOREIGN KEY ([entidad_academica_id]) REFERENCES [academico].[entidad_academica] ([id]),
        CONSTRAINT [fk_programa_educativo__sistema_educativo] FOREIGN KEY ([sistema_educativo_id]) REFERENCES [academico].[sistema_educativo] ([id]),
        CONSTRAINT [fk_programa_educativo__nivel_formacion] FOREIGN KEY ([nivel_formacion_id]) REFERENCES [academico].[nivel_formacion] ([id]),
        CONSTRAINT [ck_programa_educativo__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.plan_estudios', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[plan_estudios]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [codigo] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [programa_educativo_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_plan_estudios] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_plan_estudios__programa_educativo_id_codigo] UNIQUE ([programa_educativo_id], [codigo]),
        CONSTRAINT [fk_plan_estudios__programa_educativo] FOREIGN KEY ([programa_educativo_id]) REFERENCES [academico].[programa_educativo] ([id]),
        CONSTRAINT [ck_plan_estudios__codigo_no_vacio] CHECK
            ([codigo] = UPPER(LTRIM(RTRIM([codigo]))) AND [codigo] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([codigo]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.experiencia_educativa', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[experiencia_educativa]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [materia_ee] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [curso_ee] varchar(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [horas_teoricas] int NOT NULL,
        [horas_practicas] int NOT NULL,
        [creditos] int NOT NULL,
        [perfil_docente] nvarchar(max) COLLATE Modern_Spanish_100_CI_AI NULL,
        [area_formacion_id] int NOT NULL,
        [plan_estudios_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_experiencia_educativa] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_experiencia_educativa__plan_materia_curso] UNIQUE ([plan_estudios_id], [materia_ee], [curso_ee]),
        CONSTRAINT [fk_experiencia_educativa__area_formacion] FOREIGN KEY ([area_formacion_id]) REFERENCES [academico].[area_formacion] ([id]),
        CONSTRAINT [fk_experiencia_educativa__plan_estudios] FOREIGN KEY ([plan_estudios_id]) REFERENCES [academico].[plan_estudios] ([id]),
        CONSTRAINT [ck_experiencia_educativa__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_experiencia_educativa__materia_formato] CHECK ([materia_ee] = UPPER(LTRIM(RTRIM([materia_ee]))) AND [materia_ee] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([materia_ee]))) > 0),
        CONSTRAINT [ck_experiencia_educativa__curso_formato] CHECK ([curso_ee] = UPPER(LTRIM(RTRIM([curso_ee]))) AND [curso_ee] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([curso_ee]))) > 0),
        CONSTRAINT [ck_experiencia_educativa__horas_no_negativas] CHECK ([horas_teoricas] >= 0 AND [horas_practicas] >= 0),
        CONSTRAINT [ck_experiencia_educativa__creditos_positivos] CHECK ([creditos] > 0),
        CONSTRAINT [ck_experiencia_educativa__perfil_no_vacio] CHECK ([perfil_docente] IS NULL OR LEN(LTRIM(RTRIM([perfil_docente]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.periodo_escolar', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[periodo_escolar]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [clave] char(6) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [fecha_inicio] date NOT NULL,
        [fecha_fin] date NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_periodo_escolar] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_periodo_escolar__clave] UNIQUE ([clave]),
        CONSTRAINT [ck_periodo_escolar__clave_formato] CHECK ([clave] NOT LIKE '%[^0-9]%' AND LEN([clave]) = 6),
        CONSTRAINT [ck_periodo_escolar__rango_fechas] CHECK ([fecha_inicio] <= [fecha_fin])
    );
END
GO

IF OBJECT_ID(N'academico.programacion_academica', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[programacion_academica]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nrc] varchar(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [periodo_escolar_id] int NOT NULL,
        [experiencia_educativa_id] int NOT NULL,
        [fecha_eliminacion] datetime2(0) NULL,
        CONSTRAINT [pk_programacion_academica] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [uq_programacion_academica__periodo_escolar_id_nrc] UNIQUE ([periodo_escolar_id], [nrc]),
        CONSTRAINT [fk_programacion_academica__periodo_escolar] FOREIGN KEY ([periodo_escolar_id]) REFERENCES [academico].[periodo_escolar] ([id]),
        CONSTRAINT [fk_programacion_academica__experiencia_educativa] FOREIGN KEY ([experiencia_educativa_id]) REFERENCES [academico].[experiencia_educativa] ([id]),
        CONSTRAINT [ck_programacion_academica__nrc_formato] CHECK ([nrc] = UPPER(LTRIM(RTRIM([nrc]))) AND [nrc] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([nrc]))) > 0)
    );
END
GO

IF OBJECT_ID(N'integracion.sincronizacion_planea', N'U') IS NULL
BEGIN
    CREATE TABLE [integracion].[sincronizacion_planea]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [periodo_escolar_id] int NOT NULL,
        [estado] varchar(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [iniciada_en] datetime2(0) NOT NULL,
        [finalizada_en] datetime2(0) NULL,
        [registros_recibidos] int NOT NULL CONSTRAINT [df_sincronizacion_planea__registros_recibidos] DEFAULT (0),
        [registros_ignorados] int NOT NULL CONSTRAINT [df_sincronizacion_planea__registros_ignorados] DEFAULT (0),
        [sesiones_generadas] int NOT NULL CONSTRAINT [df_sincronizacion_planea__sesiones_generadas] DEFAULT (0),
        [duplicados_descartados] int NOT NULL CONSTRAINT [df_sincronizacion_planea__duplicados_descartados] DEFAULT (0),
        [advertencias] int NOT NULL CONSTRAINT [df_sincronizacion_planea__advertencias] DEFAULT (0),
        [mensaje_error] nvarchar(4000) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_sincronizacion_planea] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_sincronizacion_planea__periodo_escolar] FOREIGN KEY ([periodo_escolar_id]) REFERENCES [academico].[periodo_escolar] ([id]),
        CONSTRAINT [ck_sincronizacion_planea__estado] CHECK ([estado] IN ('EN_PROCESO', 'EXITOSA', 'FALLIDA')),
        CONSTRAINT [ck_sincronizacion_planea__contadores] CHECK ([registros_recibidos] >= 0 AND [registros_ignorados] >= 0 AND [sesiones_generadas] >= 0 AND [duplicados_descartados] >= 0 AND [advertencias] >= 0),
        CONSTRAINT [ck_sincronizacion_planea__fechas_estado] CHECK (([estado] = 'EN_PROCESO' AND [finalizada_en] IS NULL) OR ([estado] IN ('EXITOSA', 'FALLIDA') AND [finalizada_en] IS NOT NULL AND [finalizada_en] >= [iniciada_en]))
    );
END
GO

IF OBJECT_ID(N'academico.horario_programacion', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[horario_programacion]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [programacion_academica_id] int NOT NULL,
        [sincronizacion_planea_id] int NOT NULL,
        [dia_semana] tinyint NOT NULL,
        [hora_inicio] time(0) NOT NULL,
        [hora_fin] time(0) NOT NULL,
        [fecha_inicio] date NOT NULL,
        [fecha_fin] date NOT NULL,
        [edificio] nvarchar(100) COLLATE Modern_Spanish_100_CI_AI NULL,
        [aula] nvarchar(100) COLLATE Modern_Spanish_100_CI_AI NULL,
        CONSTRAINT [pk_horario_programacion] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [fk_horario_programacion__programacion_academica] FOREIGN KEY ([programacion_academica_id]) REFERENCES [academico].[programacion_academica] ([id]),
        CONSTRAINT [fk_horario_programacion__sincronizacion_planea] FOREIGN KEY ([sincronizacion_planea_id]) REFERENCES [integracion].[sincronizacion_planea] ([id]),
        CONSTRAINT [uq_horario_programacion__sesion] UNIQUE ([programacion_academica_id], [dia_semana], [hora_inicio], [hora_fin], [fecha_inicio], [fecha_fin], [edificio], [aula]),
        CONSTRAINT [ck_horario_programacion__dia_semana] CHECK ([dia_semana] BETWEEN 1 AND 6),
        CONSTRAINT [ck_horario_programacion__rango_horas] CHECK ([hora_inicio] < [hora_fin]),
        CONSTRAINT [ck_horario_programacion__rango_fechas] CHECK ([fecha_inicio] <= [fecha_fin]),
        CONSTRAINT [ck_horario_programacion__edificio] CHECK ([edificio] IS NULL OR LEN(LTRIM(RTRIM([edificio]))) > 0),
        CONSTRAINT [ck_horario_programacion__aula] CHECK ([aula] IS NULL OR LEN(LTRIM(RTRIM([aula]))) > 0)
    );
END
GO

IF OBJECT_ID(N'academico.docente', N'U') IS NULL
BEGIN
    CREATE TABLE [academico].[docente]
    (
        [id] int IDENTITY(1,1) NOT NULL,
        [nombre] nvarchar(200) COLLATE Modern_Spanish_100_CI_AI NOT NULL,
        [num_personal] varchar(50) COLLATE Latin1_General_100_BIN2 NULL,
        CONSTRAINT [pk_docente] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [ck_docente__nombre_no_vacio] CHECK (LEN(LTRIM(RTRIM([nombre]))) > 0),
        CONSTRAINT [ck_docente__num_personal_formato] CHECK ([num_personal] IS NULL OR ([num_personal] = UPPER(LTRIM(RTRIM([num_personal]))) AND [num_personal] NOT LIKE '%[^A-Z0-9._-]%' AND LEN(LTRIM(RTRIM([num_personal]))) > 0))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'uq_docente__num_personal' AND object_id = OBJECT_ID(N'academico.docente'))
    CREATE UNIQUE INDEX [uq_docente__num_personal] ON [academico].[docente] ([num_personal]) WHERE [num_personal] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__campus_id' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__campus_id] ON [academico].[entidad_academica] ([campus_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__area_academica_id' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__area_academica_id] ON [academico].[entidad_academica] ([area_academica_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__municipio_id' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__municipio_id] ON [academico].[entidad_academica] ([municipio_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__telefono' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__telefono] ON [academico].[entidad_academica] ([telefono]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__calle' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__calle] ON [academico].[entidad_academica] ([calle]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__numero_exterior' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__numero_exterior] ON [academico].[entidad_academica] ([numero_exterior]) WHERE [numero_exterior] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__colonia' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__colonia] ON [academico].[entidad_academica] ([colonia]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_entidad_academica__codigo_postal' AND object_id = OBJECT_ID(N'academico.entidad_academica'))
    CREATE INDEX [ix_entidad_academica__codigo_postal] ON [academico].[entidad_academica] ([codigo_postal]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_programa_educativo__sistema_educativo_id' AND object_id = OBJECT_ID(N'academico.programa_educativo'))
    CREATE INDEX [ix_programa_educativo__sistema_educativo_id] ON [academico].[programa_educativo] ([sistema_educativo_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_programa_educativo__nivel_formacion_id' AND object_id = OBJECT_ID(N'academico.programa_educativo'))
    CREATE INDEX [ix_programa_educativo__nivel_formacion_id] ON [academico].[programa_educativo] ([nivel_formacion_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_experiencia_educativa__area_formacion_id' AND object_id = OBJECT_ID(N'academico.experiencia_educativa'))
    CREATE INDEX [ix_experiencia_educativa__area_formacion_id] ON [academico].[experiencia_educativa] ([area_formacion_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_programacion_academica__experiencia_educativa_id' AND object_id = OBJECT_ID(N'academico.programacion_academica'))
    CREATE INDEX [ix_programacion_academica__experiencia_educativa_id] ON [academico].[programacion_academica] ([experiencia_educativa_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_sincronizacion_planea__periodo_inicio' AND object_id = OBJECT_ID(N'integracion.sincronizacion_planea'))
    CREATE INDEX [ix_sincronizacion_planea__periodo_inicio] ON [integracion].[sincronizacion_planea] ([periodo_escolar_id], [iniciada_en]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ux_sincronizacion_planea__periodo_en_proceso' AND object_id = OBJECT_ID(N'integracion.sincronizacion_planea'))
    CREATE UNIQUE INDEX [ux_sincronizacion_planea__periodo_en_proceso] ON [integracion].[sincronizacion_planea] ([periodo_escolar_id]) WHERE [estado] = 'EN_PROCESO';
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_horario_programacion__sincronizacion_planea_id' AND object_id = OBJECT_ID(N'academico.horario_programacion'))
    CREATE INDEX [ix_horario_programacion__sincronizacion_planea_id] ON [academico].[horario_programacion] ([sincronizacion_planea_id]);
GO
