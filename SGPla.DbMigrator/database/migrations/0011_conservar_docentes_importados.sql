-- Los datos del archivo no requieren un registro en el catálogo de docentes.
IF COL_LENGTH('dbo.Oferta', 'numeroPersonalImportado') IS NULL
    ALTER TABLE dbo.Oferta ADD numeroPersonalImportado nvarchar(50) NULL;
IF COL_LENGTH('dbo.Oferta', 'nombreDocenteImportado') IS NULL
    ALTER TABLE dbo.Oferta ADD nombreDocenteImportado nvarchar(300) NULL;
IF COL_LENGTH('dbo.CargaAcademica', 'numeroPersonalImportado') IS NULL
    ALTER TABLE dbo.CargaAcademica ADD numeroPersonalImportado nvarchar(50) NULL;
IF COL_LENGTH('dbo.CargaAcademica', 'nombreDocenteImportado') IS NULL
    ALTER TABLE dbo.CargaAcademica ADD nombreDocenteImportado nvarchar(300) NULL;

ALTER TABLE dbo.CargaAcademica ALTER COLUMN idDocente int NULL;
