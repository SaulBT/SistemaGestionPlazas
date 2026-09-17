SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Catálogo normalizado de regiones. */
IF OBJECT_ID(N'dbo.Region', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Region]
    (
        [id] [int] NOT NULL,
        [nombre] [varchar](100) NOT NULL,
        CONSTRAINT [PK_Region] PRIMARY KEY CLUSTERED ([id]),
        CONSTRAINT [UX_Region_nombre] UNIQUE ([nombre])
    );
END
GO

INSERT INTO [dbo].[Region] ([id], [nombre])
SELECT valores.[id], valores.[nombre]
FROM (VALUES
    (1, 'Xalapa'),
    (2, 'Veracruz'),
    (3, 'Orizaba-Córdoba'),
    (4, 'Poza Rica-Túxpan'),
    (5, 'Coatzacoalcos-Minatitlán')
) AS valores([id], [nombre])
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Region] AS region
    WHERE region.[id] = valores.[id]
);
GO

IF COL_LENGTH(N'dbo.EntidadAcademica', N'idRegion') IS NULL
BEGIN
    ALTER TABLE [dbo].[EntidadAcademica]
        ADD [idRegion] [int] NULL;
END
GO

UPDATE entidad
SET [idRegion] = region.[id]
FROM [dbo].[EntidadAcademica] AS entidad
INNER JOIN [dbo].[Region] AS region
    ON entidad.[region] = CONVERT(varchar(10), region.[id]) + '-' + region.[nombre]
    OR entidad.[region] = region.[nombre]
WHERE entidad.[idRegion] IS NULL;
GO

IF EXISTS (SELECT 1 FROM [dbo].[EntidadAcademica] WHERE [idRegion] IS NULL)
    THROW 51005, 'No se puede normalizar EntidadAcademica: existe una region no registrada.', 1;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_EntidadAcademica_idRegion'
      AND object_id = OBJECT_ID(N'dbo.EntidadAcademica')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_EntidadAcademica_idRegion]
        ON [dbo].[EntidadAcademica] ([idRegion]);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_EntidadAcademica_Region'
      AND parent_object_id = OBJECT_ID(N'dbo.EntidadAcademica')
)
BEGIN
    ALTER TABLE [dbo].[EntidadAcademica] WITH CHECK
        ADD CONSTRAINT [FK_EntidadAcademica_Region]
        FOREIGN KEY ([idRegion]) REFERENCES [dbo].[Region] ([id]);
END
GO

/* Mantiene la columna textual requerida por el MVC y REST durante la transición. */
CREATE OR ALTER TRIGGER [dbo].[TR_EntidadAcademica_SincronizarRegion]
ON [dbo].[EntidadAcademica]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE([region]) AND EXISTS
    (
        SELECT 1
        FROM inserted AS agregado
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[Region] AS region
            WHERE LTRIM(RTRIM(agregado.[region])) = CONVERT(varchar(10), region.[id]) + '-' + region.[nombre]
               OR LTRIM(RTRIM(agregado.[region])) = region.[nombre]
        )
    )
        THROW 51007, 'La region indicada no existe en el catalogo.', 1;

    IF UPDATE([region])
    BEGIN
        UPDATE entidad
        SET [idRegion] = region.[id]
        FROM [dbo].[EntidadAcademica] AS entidad
        INNER JOIN inserted AS agregado
            ON agregado.[idEntidadAcademica] = entidad.[idEntidadAcademica]
        INNER JOIN [dbo].[Region] AS region
            ON LTRIM(RTRIM(agregado.[region])) = CONVERT(varchar(10), region.[id]) + '-' + region.[nombre]
            OR LTRIM(RTRIM(agregado.[region])) = region.[nombre];
    END

    IF UPDATE([idRegion]) AND NOT UPDATE([region])
    BEGIN
        UPDATE entidad
        SET [region] = CONVERT(varchar(10), region.[id]) + '-' + region.[nombre]
        FROM [dbo].[EntidadAcademica] AS entidad
        INNER JOIN inserted AS agregado
            ON agregado.[idEntidadAcademica] = entidad.[idEntidadAcademica]
        INNER JOIN [dbo].[Region] AS region
            ON region.[id] = agregado.[idRegion];
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted AS agregado
        INNER JOIN [dbo].[EntidadAcademica] AS entidad
            ON entidad.[idEntidadAcademica] = agregado.[idEntidadAcademica]
        WHERE entidad.[idRegion] IS NULL
    )
        THROW 51006, 'La region de la entidad academica es obligatoria.', 1;
END
GO

/*
    Normaliza los catálogos que históricamente guardaban el código dentro de
    nombre. La migración conserva la columna nombre y separa únicamente los
    prefijos numéricos de cinco dígitos que puede identificar sin ambigüedad.
*/

IF COL_LENGTH(N'dbo.ProgramaEducativo', N'codigo') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProgramaEducativo]
        ADD [codigo] [varchar](5) NULL;
END
GO

/* Quitar espacios heredados antes de evaluar los prefijos y restricciones. */
UPDATE [dbo].[EntidadAcademica]
SET [clave] = NULLIF(LTRIM(RTRIM([clave])), ''),
    [nombre] = LTRIM(RTRIM([nombre]));
GO

UPDATE [dbo].[ProgramaEducativo]
SET [codigo] = NULLIF(LTRIM(RTRIM([codigo])), ''),
    [nombre] = LTRIM(RTRIM([nombre]));
GO

/* Entidades académicas: completar la clave desde el prefijo histórico. */
UPDATE entidad
SET [clave] = LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1)))
FROM [dbo].[EntidadAcademica] AS entidad
WHERE NULLIF(LTRIM(RTRIM([clave])), '') IS NULL
  AND CHARINDEX('-', [nombre]) > 1
  AND LEN(LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1)))) = 5
  AND LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1))) NOT LIKE '%[^0-9]%';
GO

/* Sólo se elimina el prefijo cuando coincide con la clave separada. */
UPDATE entidad
SET [nombre] = LTRIM(SUBSTRING([nombre], LEN(LTRIM(RTRIM([clave]))) + 2, 100))
FROM [dbo].[EntidadAcademica] AS entidad
WHERE NULLIF(LTRIM(RTRIM([clave])), '') IS NOT NULL
  AND [nombre] LIKE LTRIM(RTRIM([clave])) + '-%';
GO

/* Programas: usar el prefijo existente o el ID de prueba como código estable. */
UPDATE programa
SET [codigo] = LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1)))
FROM [dbo].[ProgramaEducativo] AS programa
WHERE NULLIF(LTRIM(RTRIM([codigo])), '') IS NULL
  AND CHARINDEX('-', [nombre]) > 1
  AND LEN(LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1)))) = 5
  AND LTRIM(RTRIM(LEFT([nombre], CHARINDEX('-', [nombre]) - 1))) NOT LIKE '%[^0-9]%';
GO

IF EXISTS
(
    SELECT 1
    FROM [dbo].[ProgramaEducativo]
    WHERE [codigo] IS NULL
      AND [idProgramaEducativo] > 99999
)
    THROW 51002, 'No se puede asignar un codigo automatico a un programa con ID mayor a 99999.', 1;
GO

UPDATE programa
SET [codigo] = RIGHT('00000' + CONVERT(varchar(5), [idProgramaEducativo]), 5)
FROM [dbo].[ProgramaEducativo] AS programa
WHERE NULLIF(LTRIM(RTRIM([codigo])), '') IS NULL;
GO

UPDATE programa
SET [nombre] = LTRIM(SUBSTRING([nombre], LEN(LTRIM(RTRIM([codigo]))) + 2, 100))
FROM [dbo].[ProgramaEducativo] AS programa
WHERE NULLIF(LTRIM(RTRIM([codigo])), '') IS NOT NULL
  AND [nombre] LIKE LTRIM(RTRIM([codigo])) + '-%';
GO

/* No ocultar conflictos de datos al crear las reglas de integridad. */
IF EXISTS
(
    SELECT [clave]
    FROM [dbo].[EntidadAcademica]
    WHERE [fechaEliminacion] IS NULL
      AND [clave] IS NOT NULL
    GROUP BY [clave]
    HAVING COUNT(*) > 1
)
    THROW 51000, 'No se puede normalizar EntidadAcademica: existen claves duplicadas.', 1;
GO

IF EXISTS
(
    SELECT 1
    FROM [dbo].[EntidadAcademica]
    WHERE [clave] IS NOT NULL
      AND (LEN([clave]) <> 5 OR [clave] LIKE '%[^0-9]%')
)
    THROW 51003, 'No se puede normalizar EntidadAcademica: existe una clave que no tiene cinco digitos.', 1;
GO

IF EXISTS
(
    SELECT [codigo]
    FROM [dbo].[ProgramaEducativo]
    WHERE [fechaEliminacion] IS NULL
      AND [codigo] IS NOT NULL
    GROUP BY [codigo]
    HAVING COUNT(*) > 1
)
    THROW 51001, 'No se puede normalizar ProgramaEducativo: existen codigos duplicados.', 1;
GO

IF EXISTS
(
    SELECT 1
    FROM [dbo].[ProgramaEducativo]
    WHERE [codigo] IS NOT NULL
      AND (LEN([codigo]) <> 5 OR [codigo] LIKE '%[^0-9]%')
)
    THROW 51004, 'No se puede normalizar ProgramaEducativo: existe un codigo que no tiene cinco digitos.', 1;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_EntidadAcademica_clave_numerica'
      AND parent_object_id = OBJECT_ID(N'dbo.EntidadAcademica')
)
BEGIN
    ALTER TABLE [dbo].[EntidadAcademica]
        WITH CHECK ADD CONSTRAINT [CK_EntidadAcademica_clave_numerica]
        CHECK ([clave] IS NULL OR (LEN([clave]) = 5 AND [clave] NOT LIKE '%[^0-9]%'));
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_ProgramaEducativo_codigo_numerico'
      AND parent_object_id = OBJECT_ID(N'dbo.ProgramaEducativo')
)
BEGIN
    ALTER TABLE [dbo].[ProgramaEducativo]
        WITH CHECK ADD CONSTRAINT [CK_ProgramaEducativo_codigo_numerico]
        CHECK ([codigo] IS NULL OR (LEN([codigo]) = 5 AND [codigo] NOT LIKE '%[^0-9]%'));
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_EntidadAcademica_clave_activa'
      AND object_id = OBJECT_ID(N'dbo.EntidadAcademica')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_EntidadAcademica_clave_activa]
        ON [dbo].[EntidadAcademica] ([clave])
        WHERE [fechaEliminacion] IS NULL AND [clave] IS NOT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_ProgramaEducativo_codigo_activo'
      AND object_id = OBJECT_ID(N'dbo.ProgramaEducativo')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ProgramaEducativo_codigo_activo]
        ON [dbo].[ProgramaEducativo] ([codigo])
        WHERE [fechaEliminacion] IS NULL AND [codigo] IS NOT NULL;
END
GO
