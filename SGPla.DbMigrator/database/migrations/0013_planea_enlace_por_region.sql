SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Enlace de copias PLANEA por región.
    PLANEA usa el mismo código de plan (sec_programa) en varias regiones, por ejemplo
    ISOF-14-E-CR en Xalapa, Orizaba-Córdoba y Coatzacoalcos-Minatitlán. Cada región
    registra su propio plan de estudios, así que:
    1) el código de plan pasa a ser único por programa educativo, no en todo el sistema;
    2) se eliminan las copias ya registradas cuya región no coincide con la región de
       la entidad académica dueña del plan (enlazadas antes de exigir la región).
*/

/* 1. codigoPlan único por programa educativo. */
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_PlanEstudios_codigoPlan' AND object_id = OBJECT_ID(N'dbo.PlanEstudios'))
    DROP INDEX [UQ_PlanEstudios_codigoPlan] ON [dbo].[PlanEstudios];
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_PlanEstudios_programa_codigoPlan' AND object_id = OBJECT_ID(N'dbo.PlanEstudios'))
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_PlanEstudios_programa_codigoPlan]
        ON [dbo].[PlanEstudios] ([idProgramaEducativo] ASC, [codigoPlan] ASC)
        WHERE [codigoPlan] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PlanEstudios_codigoPlan' AND object_id = OBJECT_ID(N'dbo.PlanEstudios'))
    CREATE NONCLUSTERED INDEX [IX_PlanEstudios_codigoPlan]
        ON [dbo].[PlanEstudios] ([codigoPlan] ASC)
        INCLUDE ([idProgramaEducativo])
        WHERE [codigoPlan] IS NOT NULL;
GO

/* 2. Limpieza única: copias de otra región (sus horarios se eliminan en cascada). */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo', N'U') IS NOT NULL
BEGIN
    DELETE copia
    FROM [dbo].[ExperienciaEducativaPeriodo] AS copia
    INNER JOIN [dbo].[PlanEstudios] AS planEstudios ON planEstudios.[idPlanEstudios] = copia.[idPlanEstudios]
    INNER JOIN [dbo].[ProgramaEducativo] AS programa ON programa.[idProgramaEducativo] = planEstudios.[idProgramaEducativo]
    INNER JOIN [dbo].[EntidadAcademica] AS entidad ON entidad.[idEntidadAcademica] = programa.[idEntidadAcademica]
    WHERE copia.[idRegion] IS NULL
       OR entidad.[idRegion] IS NULL
       OR copia.[idRegion] <> entidad.[idRegion];
END
GO
