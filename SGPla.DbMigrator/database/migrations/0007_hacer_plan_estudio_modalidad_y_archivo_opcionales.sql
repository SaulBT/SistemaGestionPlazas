SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Permite registrar planes de estudio sin modalidad o archivo importado. */
ALTER TABLE [dbo].[PlanEstudios]
    ALTER COLUMN [modalidad] [varchar](100) NULL;

ALTER TABLE [dbo].[PlanEstudios]
    ALTER COLUMN [idArchivoPlan] [int] NULL;
