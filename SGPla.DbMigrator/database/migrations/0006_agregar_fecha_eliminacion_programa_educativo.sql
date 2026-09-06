SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Agrega la fecha de eliminación lógica de los programas educativos. */
IF COL_LENGTH(N'dbo.ProgramaEducativo', N'fechaEliminacion') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProgramaEducativo]
        ADD [fechaEliminacion] [datetime2](7) NULL;
END
GO
