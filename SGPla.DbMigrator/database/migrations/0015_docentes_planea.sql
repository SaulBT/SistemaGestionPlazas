SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Docentes de cada copia PLANEA (NRC).
    PLANEA devuelve una fila de "resultado" por docente del NRC con su nombre
    (radoc_nombre), número de personal (ID_TITULAR) y si imparte la EE (IND_IMPARTE).
    Los docentes cambian durante el periodo, así que cada sincronización reemplaza
    los docentes de los NRC que recibe.
*/
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoDocente', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodoDocente]
    (
        [idExperienciaEducativaPeriodoDocente] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativaPeriodo]        [int] NOT NULL,
        [numeroPersonal]                       [varchar](15)  NULL,   -- ID_TITULAR
        [nombre]                               [varchar](150) NOT NULL,   -- radoc_nombre
        [imparte]                              [bit] NULL,   -- IND_IMPARTE; NULL si PLANEA no lo indica
        CONSTRAINT [PK_ExperienciaEducativaPeriodoDocente] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodoDocente] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodoDocente_ExperienciaEducativaPeriodo]
            FOREIGN KEY ([idExperienciaEducativaPeriodo])
            REFERENCES [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativaPeriodo]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodoDocente_idExperienciaEducativaPeriodo' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoDocente'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodoDocente_idExperienciaEducativaPeriodo]
        ON [dbo].[ExperienciaEducativaPeriodoDocente] ([idExperienciaEducativaPeriodo] ASC);
GO
