SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Aprobación DGAA de la programación PLANEA.
    Cada copia (NRC) queda Pendiente hasta que DGAA la aprueba (se crea su Oferta)
    o la descarta. Solo la programación aprobada es visible para la entidad académica.
*/

IF COL_LENGTH(N'dbo.ExperienciaEducativaPeriodo', N'estadoAprobacion') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD [estadoAprobacion] [varchar](10) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_estadoAprobacion] DEFAULT ('Pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ExperienciaEducativaPeriodo_estadoAprobacion')
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD CONSTRAINT [CK_ExperienciaEducativaPeriodo_estadoAprobacion]
            CHECK ([estadoAprobacion] IN ('Pendiente', 'Aprobada', 'Descartada'));
GO

IF COL_LENGTH(N'dbo.ExperienciaEducativaPeriodo', N'idOferta') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD [idOferta]      [int] NULL,
            [fechaRevision] [datetime2](0) NULL,
            [revisadoPor]   [nvarchar](150) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExperienciaEducativaPeriodo_Oferta')
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD CONSTRAINT [FK_ExperienciaEducativaPeriodo_Oferta]
            FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ExperienciaEducativaPeriodo_idOferta' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ExperienciaEducativaPeriodo_idOferta]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idOferta] ASC)
        WHERE [idOferta] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_plan_periodo_estado' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_plan_periodo_estado]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idPlanEstudios] ASC, [idPeriodo] ASC, [estadoAprobacion] ASC);
GO
