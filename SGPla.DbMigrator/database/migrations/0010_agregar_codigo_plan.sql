IF COL_LENGTH(N'dbo.PlanEstudios', N'codigoPlan') IS NULL
BEGIN
    ALTER TABLE dbo.PlanEstudios ADD codigoPlan varchar(50) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_PlanEstudios_codigoPlan' AND object_id = OBJECT_ID(N'dbo.PlanEstudios'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_PlanEstudios_codigoPlan
        ON dbo.PlanEstudios(codigoPlan)
        WHERE codigoPlan IS NOT NULL;
END
GO
