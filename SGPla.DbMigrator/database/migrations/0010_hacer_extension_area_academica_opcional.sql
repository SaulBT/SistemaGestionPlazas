SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Permite registrar áreas académicas sin extensión telefónica. */
ALTER TABLE [dbo].[AreaAcademica]
    ALTER COLUMN [extension] [varchar](5) NULL;
