SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Sincronización con PLANEA.
    - SincronizacionPlanea: bitácora de ejecuciones.
    - ExperienciaEducativaPeriodo: copia de una EE del catálogo en un periodo,
      identificada por NRC (tabla ternaria EE x Periodo x PlanEstudios).
    - ExperienciaEducativaPeriodoHorario: horario y espacio de cada copia.
    El catálogo (ExperienciaEducativa, PlanEstudios) NO se modifica: su fuente
    de verdad es la carga de planes de estudio.
*/

/* Índice de apoyo para enlazar copias con el catálogo por código de EE. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativa_codigo' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativa'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativa_codigo]
        ON [dbo].[ExperienciaEducativa] ([codigo] ASC)
        INCLUDE ([idPlanEstudios]);
GO

/* 1. Bitácora. */
IF OBJECT_ID(N'dbo.SincronizacionPlanea', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SincronizacionPlanea]
    (
        [idSincronizacionPlanea] [int] IDENTITY(1,1) NOT NULL,
        [idPeriodo]              [int] NOT NULL,
        [fechaInicio]            [datetime2](0) NOT NULL,
        [fechaFin]               [datetime2](0) NULL,
        [estado]                 [varchar](15) NOT NULL,
        [registrosRecibidos]     [int] NULL,   -- filas de "resultado"
        [nrcRecibidos]           [int] NULL,   -- NRC distintos
        [nrcSinPlan]             [int] NULL,   -- omitidos: sec_programa nulo
        [nrcSinExperiencia]      [int] NULL,   -- omitidos: EE/plan no está en el catálogo
        [nrcExistentes]          [int] NULL,   -- omitidos: copia ya registrada
        [nrcNuevos]              [int] NULL,   -- copias registradas en esta ejecución
        [horariosInsertados]     [int] NULL,
        [advertencias]           [nvarchar](max) NULL,
        [mensajeError]           [nvarchar](max) NULL,
        CONSTRAINT [PK_SincronizacionPlanea] PRIMARY KEY CLUSTERED ([idSincronizacionPlanea] ASC),
        CONSTRAINT [FK_SincronizacionPlanea_Periodo]
            FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]),
        CONSTRAINT [CK_SincronizacionPlanea_estado] CHECK ([estado] IN
            ('EnProceso', 'Exitosa', 'SinDatos', 'Omitida', 'Fallida', 'Interrumpida'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SincronizacionPlanea_periodo_fecha' AND object_id = OBJECT_ID(N'dbo.SincronizacionPlanea'))
    CREATE NONCLUSTERED INDEX [IX_SincronizacionPlanea_periodo_fecha]
        ON [dbo].[SincronizacionPlanea] ([idPeriodo] ASC, [fechaInicio] DESC);
GO

/* 2. Copia de la EE en el periodo (NRC). */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodo]
    (
        [idExperienciaEducativaPeriodo] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativa]        [int] NOT NULL,
        [idPeriodo]                     [int] NOT NULL,
        [idPlanEstudios]                [int] NOT NULL,
        [idRegion]                      [int] NULL,
        [idSincronizacionPlanea]        [int] NOT NULL,   -- ejecución que la registró
        [nrc]                           [varchar](5)   NOT NULL,
        [titulo]                        [varchar](150) NOT NULL,   -- sec_titulo tal cual
        [campus]                        [varchar](5)   NULL,
        [nivel]                         [varchar](5)   NULL,
        [area]                          [varchar](100) NULL,
        [fechaAlta]                     [datetime2](0) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_fechaAlta] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ExperienciaEducativaPeriodo] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodo] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodo_periodo_nrc] UNIQUE NONCLUSTERED ([idPeriodo] ASC, [nrc] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_ExperienciaEducativa]
            FOREIGN KEY ([idExperienciaEducativa]) REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Periodo]
            FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_PlanEstudios]
            FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Region]
            FOREIGN KEY ([idRegion]) REFERENCES [dbo].[Region] ([id]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_SincronizacionPlanea]
            FOREIGN KEY ([idSincronizacionPlanea]) REFERENCES [dbo].[SincronizacionPlanea] ([idSincronizacionPlanea])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idExperienciaEducativa' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idExperienciaEducativa]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativa] ASC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idPlanEstudios' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idPlanEstudios]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idPlanEstudios] ASC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idSincronizacionPlanea' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idSincronizacionPlanea]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idSincronizacionPlanea] ASC);
GO

/* 3. Horario y espacio de cada copia: una fila por día. */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoHorario', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodoHorario]
    (
        [idExperienciaEducativaPeriodoHorario] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativaPeriodo]        [int] NOT NULL,
        [idHorarioPlanea]                      [int] NOT NULL,   -- rhs_id de referencia
        [dia]                                  [varchar](10)  NOT NULL,
        [horaInicio]                           [time](0) NOT NULL,
        [horaFin]                              [time](0) NOT NULL,
        [edificio]                             [varchar](50)  NULL,
        [aula]                                 [varchar](100) NULL,
        [fechaInicio]                          [date] NULL,
        [fechaFin]                             [date] NULL,
        CONSTRAINT [PK_ExperienciaEducativaPeriodoHorario] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodoHorario] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodoHorario_sesion]
            UNIQUE NONCLUSTERED ([idExperienciaEducativaPeriodo] ASC, [dia] ASC, [horaInicio] ASC, [horaFin] ASC,
                                 [edificio] ASC, [aula] ASC, [fechaInicio] ASC, [fechaFin] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodoHorario_ExperienciaEducativaPeriodo]
            FOREIGN KEY ([idExperienciaEducativaPeriodo])
            REFERENCES [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativaPeriodo]) ON DELETE CASCADE,
        CONSTRAINT [CK_ExperienciaEducativaPeriodoHorario_horas] CHECK ([horaInicio] < [horaFin])
    );
END
GO
