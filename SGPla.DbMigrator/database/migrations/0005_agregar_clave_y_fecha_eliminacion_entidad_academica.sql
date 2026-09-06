SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Agrega la clave independiente y la fecha de eliminación lógica de las entidades académicas. */
IF COL_LENGTH(N'dbo.EntidadAcademica', N'clave') IS NULL
BEGIN
    ALTER TABLE [dbo].[EntidadAcademica]
        ADD [clave] [varchar](5) NULL;
END

IF COL_LENGTH(N'dbo.EntidadAcademica', N'fechaEliminacion') IS NULL
BEGIN
    ALTER TABLE [dbo].[EntidadAcademica]
        ADD [fechaEliminacion] [datetime2](7) NULL;
END
GO
