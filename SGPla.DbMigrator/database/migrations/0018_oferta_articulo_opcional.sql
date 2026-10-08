SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    El artículo no se define al aprobar la programación importada de PLANEA:
    las ofertas aprobadas quedan sin artículo (se asigna después, en el aviso).
*/
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Oferta') AND name = N'idArticulo' AND is_nullable = 0)
    ALTER TABLE [dbo].[Oferta] ALTER COLUMN [idArticulo] [int] NULL;
GO
