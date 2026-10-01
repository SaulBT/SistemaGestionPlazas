SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.OfertaExcluidaAviso', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OfertaExcluidaAviso](
        [idOfertaExcluidaAviso] [int] IDENTITY(1,1) NOT NULL,
        [idAviso] [int] NOT NULL,
        [idOferta] [int] NOT NULL,
        [motivo] [varchar](max) NOT NULL,
        CONSTRAINT [PK_OfertaExcluidaAviso] PRIMARY KEY CLUSTERED ([idOfertaExcluidaAviso] ASC),
        CONSTRAINT [UQ_OfertaExcluidaAviso_Oferta_Aviso] UNIQUE NONCLUSTERED ([idOferta] ASC, [idAviso] ASC),
        CONSTRAINT [FK_OfertaExcluidaAviso_Aviso] FOREIGN KEY ([idAviso]) REFERENCES [dbo].[Aviso] ([idAviso]) ON DELETE CASCADE,
        CONSTRAINT [FK_OfertaExcluidaAviso_Oferta] FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta])
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    CREATE NONCLUSTERED INDEX [IX_OfertaExcluidaAviso_idAviso]
        ON [dbo].[OfertaExcluidaAviso] ([idAviso] ASC);
END
GO
