-- Catálogos ficticios para desarrollo y pruebas del modelo nuevo.
-- Se ejecuta únicamente cuando MIGRATOR_APPLY_DEVELOPMENT_SEED=true.
-- No escribe en dbo: el modelo legacy permanece aislado para la migración paralela.

IF NOT EXISTS (SELECT 1 FROM [academico].[region])
BEGIN
    SET IDENTITY_INSERT [academico].[region] ON;
    INSERT INTO [academico].[region] ([id], [clave], [nombre]) VALUES
        (1, 1, N'Xalapa'),
        (2, 2, N'Veracruz'),
        (3, 3, N'Orizaba-Córdoba'),
        (4, 4, N'Poza Rica-Túxpan'),
        (5, 5, N'Coatzacoalcos-Minatitlán');
    SET IDENTITY_INSERT [academico].[region] OFF;
    DBCC CHECKIDENT ('academico.region', RESEED, 5);
END
GO

IF NOT EXISTS (SELECT 1 FROM [academico].[campus])
BEGIN
    INSERT INTO [academico].[campus] ([clave], [nombre], [region_id])
    SELECT v.[clave], v.[nombre], r.[id]
    FROM (VALUES
        ('XALAPA', N'Campus Xalapa', 1),
        ('VERACRUZ', N'Campus Veracruz', 2),
        ('ORIZABACORDOBA', N'Campus Orizaba-Córdoba', 3),
        ('POZARICATUXPAN', N'Campus Poza Rica-Túxpan', 4),
        ('COATZACOALCOSMINATITLAN', N'Campus Coatzacoalcos-Minatitlán', 5)
    ) AS v([clave], [nombre], [region_clave])
    INNER JOIN [academico].[region] AS r ON r.[clave] = v.[region_clave];
END
GO

IF NOT EXISTS (SELECT 1 FROM [academico].[area_academica])
    INSERT INTO [academico].[area_academica] ([clave], [nombre]) VALUES
        (1, N'Técnica'), (2, N'Económico Administrativa'), (3, N'Humanidades');
GO

IF NOT EXISTS (SELECT 1 FROM [academico].[sistema_educativo])
    INSERT INTO [academico].[sistema_educativo] ([nombre]) VALUES
        (N'Escolarizado'), (N'Virtual'), (N'Abierta');
GO

IF NOT EXISTS (SELECT 1 FROM [academico].[nivel_formacion])
    INSERT INTO [academico].[nivel_formacion] ([clave], [nombre]) VALUES
        ('TSU', N'Técnico Superior Universitario'),
        ('LIC', N'Licenciatura'),
        ('MAE', N'Maestría'),
        ('DOC', N'Doctorado');
GO

IF NOT EXISTS (SELECT 1 FROM [academico].[area_formacion])
    INSERT INTO [academico].[area_formacion] ([clave], [nombre]) VALUES
        ('AFBG', N'Área de Formación Básica General'),
        ('AFID', N'Área de Formación de Iniciación a la Disciplina'),
        ('AFD', N'Área de Formación Disciplinaria'),
        ('AFT', N'Área de Formación Terminal'),
        ('AFEL', N'Área de Formación de Elección Libre');
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[grado_academico])
    INSERT INTO [plazas].[grado_academico] ([nombre]) VALUES
        (N'Licenciatura'), (N'Maestría'), (N'Doctorado'), (N'Especialidad');
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[tratamiento_academico])
BEGIN
    INSERT INTO [plazas].[tratamiento_academico] ([nombre], [grado_academico_id])
    SELECT v.[nombre], g.[id]
    FROM (VALUES
        (N'Lic.', N'Licenciatura'), (N'Mtro.', N'Maestría'), (N'Mtra.', N'Maestría'),
        (N'Dr.', N'Doctorado'), (N'Dra.', N'Doctorado'), (N'Especialista', N'Especialidad')
    ) AS v([nombre], [grado])
    INNER JOIN [plazas].[grado_academico] AS g ON g.[nombre] = v.[grado];
END
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[articulo])
    INSERT INTO [plazas].[articulo] ([numero], [descripcion]) VALUES
        ('70', N'Personal académico adscrito que cubra el perfil requerido.'),
        ('70 Y 73', N'Personal de la Universidad Veracruzana y público en general.'),
        ('70 Y 73 A FIN', N'Perfil similar o afín al requerido.');
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[modalidad_recepcion])
    INSERT INTO [plazas].[modalidad_recepcion] ([nombre], [requiere_lugar]) VALUES
        (N'Presencial', 1), (N'Correo electrónico', 0);
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[tipo_plaza])
    INSERT INTO [plazas].[tipo_plaza] ([nombre]) VALUES
        (N'Plaza de tiempo completo'), (N'Plaza por asignatura');
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[tipo_contratacion])
    INSERT INTO [plazas].[tipo_contratacion] ([nombre]) VALUES
        (N'Base'), (N'Interino');
GO

IF NOT EXISTS (SELECT 1 FROM [plazas].[tipo_documento_aspirante])
    INSERT INTO [plazas].[tipo_documento_aspirante] ([nombre]) VALUES
        (N'Identificación oficial'), (N'CURP'), (N'RFC'), (N'Comprobante de domicilio'),
        (N'Título profesional'), (N'Cédula profesional'), (N'Currículum vitae'), (N'Constancia de situación fiscal');
GO

-- El seed puede ejecutarse después de crear la secuencia en una base vacía.
-- Reanudarla tras las claves fijas evita colisiones en la primera alta de desarrollo.
IF OBJECT_ID(N'academico.seq_area_academica_clave', N'SO') IS NOT NULL
BEGIN
    DECLARE @siguiente_clave bigint =
        COALESCE((SELECT MAX(CONVERT(bigint, [clave])) FROM [academico].[area_academica]), 0) + 1;

    IF @siguiente_clave > 2147483647
        THROW 51000, 'No hay claves enteras disponibles para áreas académicas.', 1;

    DECLARE @reiniciar_secuencia nvarchar(max) =
        N'ALTER SEQUENCE [academico].[seq_area_academica_clave] RESTART WITH '
        + CONVERT(nvarchar(20), @siguiente_clave)
        + N';';
    EXEC sys.sp_executesql @reiniciar_secuencia;
END;
GO
