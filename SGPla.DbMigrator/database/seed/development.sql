-- Datos ficticios para desarrollo y pruebas funcionales.
-- Este script sólo se ejecuta cuando MIGRATOR_APPLY_DEVELOPMENT_SEED=true.

IF NOT EXISTS (SELECT 1 FROM [dbo].[AreaAcademica])
BEGIN
    SET IDENTITY_INSERT [dbo].[AreaAcademica] ON;

    INSERT INTO [dbo].[AreaAcademica]
    ([idAreaAcademica],[nombre],[telefono],[extension])
    VALUES
        (1, N'Técnica',                  N'2288421700', N'1101'),
        (2, N'Económico Administrativa', N'2288422700', N'2201'),
        (3, N'Humanidades',              N'2288423700', N'3301');

    SET IDENTITY_INSERT [dbo].[AreaAcademica] OFF;
    DBCC CHECKIDENT ('dbo.AreaAcademica', RESEED, 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Region])
BEGIN
    INSERT INTO [dbo].[Region] ([id], [nombre])
    VALUES
        (1, N'Xalapa'),
        (2, N'Veracruz'),
        (3, N'Orizaba-Córdoba'),
        (4, N'Poza Rica-Túxpan'),
        (5, N'Coatzacoalcos-Minatitlán');
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[EntidadAcademica])
BEGIN
    SET IDENTITY_INSERT [dbo].[EntidadAcademica] ON;

    INSERT INTO [dbo].[EntidadAcademica]
    (
        [idEntidadAcademica], [idAreaAcademica], [idRegion], [clave], [nombre],
        [calleNumero], [colonia], [cp], [municipio],
        [telefono], [extension], [region]
    )
    VALUES
        (1,2,1,N'11304',N'Facultad de Estadística e Informática',
            N'Av. Xalapa S/N', N'Unidad Universitaria', N'91090', N'Xalapa',
            N'2288421700', N'1110', N'1-Xalapa'),
        (2,1,1,N'11305',N'Facultad de Ingeniería Civil',
            N'Circuito Gonzalo Aguirre Beltrán S/N', N'Zona Universitaria', N'91090', N'Xalapa',
            N'2288421711', N'1111', N'1-Xalapa'),
        (3,2,1,N'22302',N'Facultad de Economía',
            N'Av. Xalapa S/N', N'Unidad Universitaria', N'91090', N'Xalapa',
            N'2288422711', N'2211', N'1-Xalapa'),
        (4,3,1,N'33301',N'Facultad de Derecho',
            N'Circuito Gonzalo Aguirre Beltrán S/N', N'Zona Universitaria', N'91090', N'Xalapa',
            N'2288423700', N'3310', N'1-Xalapa'),
        (5,3,1,N'33302',N'Facultad de Pedagogía',
            N'Francisco Moreno S/N', N'Unidad Magisterial', N'91017', N'Xalapa',
            N'2288423711', N'3311', N'1-Xalapa');

    SET IDENTITY_INSERT [dbo].[EntidadAcademica] OFF;
    DBCC CHECKIDENT ('dbo.EntidadAcademica', RESEED, 6);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Periodo])
BEGIN
    INSERT INTO [dbo].[Periodo] ([codigo]) VALUES
        ('202501'),('202551'),('202601'),('202651'),
        ('202701'),('202751'),('202801'),('202851'),
        ('202901'),('202951');
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Articulo])
BEGIN
    SET IDENTITY_INSERT [dbo].[Articulo] ON;
    INSERT INTO [dbo].[Articulo] ([idArticulo],[numero],[descripcion]) VALUES
        (1, N'70', N'personal académico adscrito que cubra el perfil requerido y cumpla con los requisitos solicitados'),
        (2, N'70 y 73', N'personal académico adscrito, al personal de la Universidad Veracruzana y público en general que cubra el perfil requerido y cumpla con los requisitos solicitados'),
        (3, N'70 y 73 a fin', N'personal académico adscrito, así como al personal de la Universidad Veracruzana y público en general que cubra los requisitos solicitados, que ostente un perfil similar o afín al requerido y');
    SET IDENTITY_INSERT [dbo].[Articulo] OFF;
    DBCC CHECKIDENT ('dbo.Articulo', RESEED, 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ProgramaEducativo])
BEGIN
    SET IDENTITY_INSERT [dbo].[ProgramaEducativo] ON;
    INSERT INTO [dbo].[ProgramaEducativo] ([idProgramaEducativo],[idEntidadAcademica],[codigo],[nombre],[campus]) VALUES
        (1,  1, N'14143', N'Estadistica',                             N'Xalapa'),
        (2,  1, N'14352', N'Ingeniería de Software',                  N'Xalapa'),
        (3,  2, N'00003', N'Ingeniería Civil',                         N'Xalapa'),
        (4,  2, N'00004', N'Ingeniería Ambiental',                     N'Xalapa'),
        (7,  3, N'00007', N'Licenciatura en Economía',                 N'Xalapa'),
        (8,  3, N'00008', N'Licenciatura en Relaciones Industriales',  N'Xalapa'),
        (9,  4, N'00009', N'Licenciatura en Derecho',                  N'Xalapa'),
        (10, 4, N'00010', N'Licenciatura en Ciencias Políticas',       N'Xalapa'),
        (11, 5, N'00011', N'Licenciatura en Pedagogía',                N'Xalapa'),
        (12, 5, N'00012', N'Licenciatura en Lengua y Literatura Hispánicas', N'Xalapa');
    SET IDENTITY_INSERT [dbo].[ProgramaEducativo] OFF;
    DBCC CHECKIDENT ('dbo.ProgramaEducativo', RESEED, 12);
END
GO
