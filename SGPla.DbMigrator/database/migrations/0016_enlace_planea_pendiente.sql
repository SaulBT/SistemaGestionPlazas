SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Copias PLANEA pendientes de enlace.
    La sincronización guarda todos los NRC con plan, aunque su EE o su plan de estudios
    aún no estén en el catálogo. El enlace con el catálogo se resuelve en local, sin volver
    a consultar PLANEA:
    - al sincronizar, y
    - al cargar o editar un plan de estudios.
    Para eso:
    1) se guardan los códigos de PLANEA (EE y plan) en cada copia;
    2) el enlace con la EE y el plan pasa a ser opcional (NULL = pendiente);
    3) al borrar una EE o un plan, sus copias vuelven a quedar pendientes en lugar
       de bloquear el borrado.
*/

/* 1. Códigos de PLANEA de cada copia. */
IF COL_LENGTH(N'dbo.ExperienciaEducativaPeriodo', N'codigoExperiencia') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD [codigoExperiencia] [varchar](10) NULL,   -- radoc_materia + radoc_curso
            [codigoPlan]        [varchar](50) NULL;   -- sec_programa
GO

-- Las copias registradas hasta ahora siempre están enlazadas: sus códigos son los del catálogo.
UPDATE copia
SET [codigoExperiencia] = experiencia.[codigo],
    [codigoPlan] = planEstudios.[codigoPlan]
FROM [dbo].[ExperienciaEducativaPeriodo] AS copia
INNER JOIN [dbo].[ExperienciaEducativa] AS experiencia ON experiencia.[idExperienciaEducativa] = copia.[idExperienciaEducativa]
INNER JOIN [dbo].[PlanEstudios] AS planEstudios ON planEstudios.[idPlanEstudios] = copia.[idPlanEstudios]
WHERE copia.[codigoExperiencia] IS NULL OR copia.[codigoPlan] IS NULL;
GO

IF COLUMNPROPERTY(OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'), N'codigoExperiencia', 'AllowsNull') = 1
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ALTER COLUMN [codigoExperiencia] [varchar](10) NOT NULL;
GO
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'), N'codigoPlan', 'AllowsNull') = 1
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ALTER COLUMN [codigoPlan] [varchar](50) NOT NULL;
GO

/* 2. Enlace opcional con la EE del catálogo. */
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'), N'idExperienciaEducativa', 'AllowsNull') = 0
BEGIN
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] DROP CONSTRAINT [FK_ExperienciaEducativaPeriodo_ExperienciaEducativa];
    DROP INDEX [IX_ExperienciaEducativaPeriodo_idExperienciaEducativa] ON [dbo].[ExperienciaEducativaPeriodo];

    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ALTER COLUMN [idExperienciaEducativa] [int] NULL;

    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ADD CONSTRAINT [FK_ExperienciaEducativaPeriodo_ExperienciaEducativa]
        FOREIGN KEY ([idExperienciaEducativa]) REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa])
        ON DELETE SET NULL;
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idExperienciaEducativa]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativa] ASC);
END
GO

/* 3. Enlace opcional con el plan de estudios del catálogo. */
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'), N'idPlanEstudios', 'AllowsNull') = 0
BEGIN
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] DROP CONSTRAINT [FK_ExperienciaEducativaPeriodo_PlanEstudios];
    DROP INDEX [IX_ExperienciaEducativaPeriodo_idPlanEstudios] ON [dbo].[ExperienciaEducativaPeriodo];

    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ALTER COLUMN [idPlanEstudios] [int] NULL;

    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo] ADD CONSTRAINT [FK_ExperienciaEducativaPeriodo_PlanEstudios]
        FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios])
        ON DELETE SET NULL;
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idPlanEstudios]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idPlanEstudios] ASC);
END
GO

/* 4. Búsqueda de copias por código al enlazar. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_codigos' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_codigos]
        ON [dbo].[ExperienciaEducativaPeriodo] ([codigoPlan] ASC, [codigoExperiencia] ASC)
        INCLUDE ([idRegion], [idPeriodo], [idExperienciaEducativa]);
GO
