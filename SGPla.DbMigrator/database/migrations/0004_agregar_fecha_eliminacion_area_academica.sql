SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Agrega la fecha de eliminación lógica de las áreas académicas. */
IF COL_LENGTH(N'dbo.AreaAcademica', N'fechaEliminacion') IS NULL
BEGIN
    ALTER TABLE [dbo].[AreaAcademica]
        ADD [fechaEliminacion] [datetime2](7) NULL;
END
GO
