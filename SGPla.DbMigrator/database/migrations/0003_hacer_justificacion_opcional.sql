SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* La justificación es opcional para la solicitud de apertura. */
IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SolicitudApertura')
      AND name = N'justificacion'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[SolicitudApertura]
        ALTER COLUMN [justificacion] [varchar](max) NULL;
END
GO
