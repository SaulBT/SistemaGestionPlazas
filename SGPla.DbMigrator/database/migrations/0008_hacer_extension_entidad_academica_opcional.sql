SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Permite registrar entidades académicas sin extensión telefónica. */
ALTER TABLE [dbo].[EntidadAcademica]
    ALTER COLUMN [extension] [varchar](5) NULL;
