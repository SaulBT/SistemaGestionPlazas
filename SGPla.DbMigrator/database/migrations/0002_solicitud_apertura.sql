SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/*
    CU-09: Solicitar apertura de Experiencia Educativa.

    La solicitud de apertura es independiente de Oferta y Solicitud.
    Los límites de solicitantes se agregan como NULL para no inventar valores
    para experiencias existentes. El backend deberá impedir solicitudes para
    experiencias que aún no tengan esos límites configurados.
*/

IF COL_LENGTH(N'dbo.ExperienciaEducativa', N'cantidadMinimaSolicitantes') IS NULL
BEGIN
    ALTER TABLE [dbo].[ExperienciaEducativa]
        ADD [cantidadMinimaSolicitantes] [int] NULL;
END
GO

IF COL_LENGTH(N'dbo.ExperienciaEducativa', N'cantidadMaximaSolicitantes') IS NULL
BEGIN
    ALTER TABLE [dbo].[ExperienciaEducativa]
        ADD [cantidadMaximaSolicitantes] [int] NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_ExperienciaEducativa_LimitesSolicitantes'
      AND parent_object_id = OBJECT_ID(N'dbo.ExperienciaEducativa')
)
BEGIN
    ALTER TABLE [dbo].[ExperienciaEducativa]
        WITH CHECK ADD CONSTRAINT [CK_ExperienciaEducativa_LimitesSolicitantes]
        CHECK
        (
            ([cantidadMinimaSolicitantes] IS NULL AND [cantidadMaximaSolicitantes] IS NULL)
            OR
            (
                [cantidadMinimaSolicitantes] > 0
                AND [cantidadMaximaSolicitantes] >= [cantidadMinimaSolicitantes]
            )
        );
END
GO

IF COL_LENGTH(N'dbo.Periodo', N'fechaInicio') IS NULL
BEGIN
    ALTER TABLE [dbo].[Periodo]
        ADD [fechaInicio] [date] NULL;
END
GO

IF COL_LENGTH(N'dbo.Periodo', N'fechaFin') IS NULL
BEGIN
    ALTER TABLE [dbo].[Periodo]
        ADD [fechaFin] [date] NULL;
END
GO

/*
    Los códigos institucionales existentes usan el sufijo 01 para
    febrero-julio y 51 para agosto-enero.
*/
UPDATE periodo
SET
    [fechaInicio] = CASE
        WHEN RIGHT(LTRIM(RTRIM([codigo])), 2) = '01'
            THEN DATEFROMPARTS(CONVERT(int, LEFT(LTRIM(RTRIM([codigo])), 4)), 2, 1)
        WHEN RIGHT(LTRIM(RTRIM([codigo])), 2) = '51'
            THEN DATEFROMPARTS(CONVERT(int, LEFT(LTRIM(RTRIM([codigo])), 4)), 8, 1)
        ELSE [fechaInicio]
    END,
    [fechaFin] = CASE
        WHEN RIGHT(LTRIM(RTRIM([codigo])), 2) = '01'
            THEN EOMONTH(DATEFROMPARTS(CONVERT(int, LEFT(LTRIM(RTRIM([codigo])), 4)), 7, 1))
        WHEN RIGHT(LTRIM(RTRIM([codigo])), 2) = '51'
            THEN EOMONTH(DATEFROMPARTS(CONVERT(int, LEFT(LTRIM(RTRIM([codigo])), 4) + 1), 1, 1))
        ELSE [fechaFin]
    END
WHERE [fechaInicio] IS NULL
   OR [fechaFin] IS NULL;
GO

IF OBJECT_ID(N'dbo.Modalidad', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Modalidad]
    (
        [idModalidad] [int] IDENTITY(1,1) NOT NULL,
        [nombre] [varchar](100) NOT NULL,
        [activa] [bit] NOT NULL
            CONSTRAINT [DF_Modalidad_Activa] DEFAULT (1),
        CONSTRAINT [PK_Modalidad] PRIMARY KEY CLUSTERED ([idModalidad] ASC),
        CONSTRAINT [UQ_Modalidad_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
    );
END
GO

/* Conserva como opciones iniciales las modalidades ya usadas por los planes. */
INSERT INTO [dbo].[Modalidad] ([nombre])
SELECT DISTINCT LTRIM(RTRIM(planEstudios.[modalidad]))
FROM [dbo].[PlanEstudios] AS planEstudios
WHERE NULLIF(LTRIM(RTRIM(planEstudios.[modalidad])), '') IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[Modalidad] AS modalidad
      WHERE modalidad.[nombre] = LTRIM(RTRIM(planEstudios.[modalidad]))
  );
GO

IF OBJECT_ID(N'dbo.SolicitudApertura', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudApertura]
    (
        [idSolicitudApertura] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativa] [int] NOT NULL,
        [idPeriodo] [int] NOT NULL,
        [idEntidadAcademica] [int] NOT NULL,
        [idProgramaEducativo] [int] NOT NULL,
        [idPlanEstudios] [int] NOT NULL,
        [idModalidad] [int] NOT NULL,
        [seccion] [varchar](50) NOT NULL,
        [cantidadSolicitantes] [int] NOT NULL,
        [justificacion] [varchar](max) NOT NULL,
        [idArchivoOficio] [int] NOT NULL,
        [estado] [varchar](20) NOT NULL
            CONSTRAINT [DF_SolicitudApertura_Estado] DEFAULT ('Pendiente'),
        [fechaCreacion] [datetime2](0) NOT NULL
            CONSTRAINT [DF_SolicitudApertura_FechaCreacion] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_SolicitudApertura]
            PRIMARY KEY CLUSTERED ([idSolicitudApertura] ASC),
        CONSTRAINT [CK_SolicitudApertura_Estado]
            CHECK ([estado] IN ('Pendiente', 'Aceptada', 'Rechazada')),
        CONSTRAINT [CK_SolicitudApertura_CantidadSolicitantes]
            CHECK ([cantidadSolicitantes] > 0),
        CONSTRAINT [CK_SolicitudApertura_Seccion]
            CHECK (LEN(LTRIM(RTRIM([seccion]))) > 0)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_ExperienciaEducativa'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_ExperienciaEducativa]
        FOREIGN KEY ([idExperienciaEducativa])
        REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_Periodo'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_Periodo]
        FOREIGN KEY ([idPeriodo])
        REFERENCES [dbo].[Periodo] ([idPeriodo]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_EntidadAcademica'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_EntidadAcademica]
        FOREIGN KEY ([idEntidadAcademica])
        REFERENCES [dbo].[EntidadAcademica] ([idEntidadAcademica]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_ProgramaEducativo'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_ProgramaEducativo]
        FOREIGN KEY ([idProgramaEducativo])
        REFERENCES [dbo].[ProgramaEducativo] ([idProgramaEducativo]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_PlanEstudios'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_PlanEstudios]
        FOREIGN KEY ([idPlanEstudios])
        REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_Modalidad'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_Modalidad]
        FOREIGN KEY ([idModalidad])
        REFERENCES [dbo].[Modalidad] ([idModalidad]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_SolicitudApertura_ArchivoOficio'
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudApertura_ArchivoOficio]
        FOREIGN KEY ([idArchivoOficio])
        REFERENCES [dbo].[Archivo] ([idArchivo]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_SolicitudApertura_ContextoActivo'
      AND object_id = OBJECT_ID(N'dbo.SolicitudApertura')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_SolicitudApertura_ContextoActivo]
        ON [dbo].[SolicitudApertura]
        (
            [idExperienciaEducativa],
            [seccion],
            [idPeriodo],
            [idEntidadAcademica],
            [idProgramaEducativo],
            [idPlanEstudios]
        )
        WHERE [estado] <> 'Rechazada';
END
GO
