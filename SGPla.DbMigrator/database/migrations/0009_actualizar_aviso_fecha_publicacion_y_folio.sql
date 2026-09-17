SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* La fecha de publicación se captura al elaborar el aviso. Se conserva NULL
   para no inventar una fecha para los avisos históricos. */
IF COL_LENGTH(N'dbo.Aviso', N'fechaPublicacion') IS NULL
BEGIN
    ALTER TABLE [dbo].[Aviso]
        ADD [fechaPublicacion] [date] NULL;
END
GO

/* El folio dejó de ser un dato del aviso y no debe conservarse como columna. */
IF COL_LENGTH(N'dbo.Aviso', N'folio') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Aviso]
        DROP COLUMN [folio];
END
GO
