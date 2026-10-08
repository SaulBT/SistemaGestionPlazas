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

-- Usuarios de desarrollo. El acceso se valida contra el LDAP institucional;
-- estas filas solo asignan el rol a cada correo.
IF NOT EXISTS (SELECT 1 FROM [dbo].[SuperUsuario] WHERE [correo] = N'zs22013663@estudiantes.uv.mx')
BEGIN
    INSERT INTO [dbo].[SuperUsuario] ([nombre], [correo])
    VALUES (N'Ian Kaleb Moctezuma Rojas', N'zs22013663@estudiantes.uv.mx');
END
GO

-- DGAA del Área Económico Administrativa (área de la Facultad de Estadística e Informática).
IF NOT EXISTS (SELECT 1 FROM [dbo].[CoordinadorDGAA] WHERE [correo] = N'zs22013696@estudiantes.uv.mx')
BEGIN
    INSERT INTO [dbo].[CoordinadorDGAA] ([idAreaAcademica], [nombre], [correo], [cargo])
    VALUES (2, N'Axel Luna', N'zs22013696@estudiantes.uv.mx', N'Coordinador de Área Académica');
END
GO

-- Coordinador de la Facultad de Estadística e Informática.
IF NOT EXISTS (SELECT 1 FROM [dbo].[CoordinadorEA] WHERE [correo] = N'zs22013630@estudiantes.uv.mx')
BEGIN
    INSERT INTO [dbo].[CoordinadorEA] ([idEntidadAcademica], [nombre], [correo], [cargo])
    VALUES (1, N'Ivan Jafeth Carballo Delgado', N'zs22013630@estudiantes.uv.mx', N'Coordinador de Entidad Académica');
END
GO

-- Avisos de ejemplo de la Facultad de Estadística e Informática (periodo vigente,
-- Art. 70 y 73): uno por estado del flujo, para poder revisar el listado de avisos.
-- No incluye archivos adjuntos ni ofertas; el acta sólo existe en el aviso con acta creada.
IF NOT EXISTS (SELECT 1 FROM [dbo].[Aviso])
BEGIN
    INSERT INTO [dbo].[Aviso]
    (
        [idEntidadAcademica], [idPeriodo], [idArticulo], [fechaCT], [fechaVacantes], [fechaCreacion],
        [requisitos], [lugar], [correo], [modalidad], [archivado], [estado], [urlPublicacion],
        [sistema], [comentarios], [fechaPublicacion]
    )
    VALUES
        (1, 5, 2, '2026-09-08', '2026-09-14', '2026-08-26',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Creado', NULL,
            N'Escolarizado', NULL, NULL),
        (1, 5, 2, '2026-09-08', '2026-09-14', '2026-08-30',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'En Revisión por DGAA', NULL,
            N'Escolarizado', NULL, NULL),
        (1, 5, 2, '2026-09-10', '2026-09-16', '2026-08-30',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Avalado por DGAA', NULL,
            N'Escolarizado', N'El aviso cumple con los requisitos y queda avalado para su firma.', NULL),
        (1, 5, 2, '2026-09-10', '2026-09-16', '2026-09-02',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Devuelto por DGAA', NULL,
            N'Escolarizado', N'Corregir los horarios de las experiencias educativas y detallar el perfil solicitado.', NULL),
        (1, 5, 2, '2026-09-15', '2026-09-21', '2026-09-03',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Firmado', NULL,
            N'Escolarizado', NULL, NULL),
        (1, 5, 2, '2026-09-15', '2026-09-21', '2026-09-04',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Publicado', N'https://www.uv.mx/personal/avisos/fei-2026-006',
            N'Escolarizado', NULL, '2026-09-18'),
        (1, 5, 2, '2026-09-15', '2026-09-21', '2026-09-04',
            N'Licenciatura en el área afín y experiencia docente mínima de dos años.',
            N'Sala de juntas de la Facultad de Estadística e Informática', N'fei@uv.mx', N'Presencial', 0, N'Acta de CT Creada', N'https://www.uv.mx/personal/avisos/fei-2026-007',
            N'Escolarizado', NULL, '2026-09-18');

    DECLARE @idAvisoActa INT = (SELECT MAX([idAviso]) FROM [dbo].[Aviso]);

    INSERT INTO [dbo].[Acta]
    ([idAviso], [folio], [lugar], [fecha], [horaInicio], [horaConclusion], [asuntosGenerales], [archivado])
    VALUES
        (@idAvisoActa, N'CT-001-26', N'Sala de juntas de la Facultad de Estadística e Informática',
            '2026-09-25', '10:00', '11:30',
            N'Revisión de las solicitudes recibidas y designación del personal académico para las experiencias educativas vacantes.', 0);
END
GO

-- Personal académico de ejemplo (con número de personal).
IF NOT EXISTS (SELECT 1 FROM [dbo].[Docente] WHERE [numeroPersonal] = '194')
BEGIN
    INSERT INTO [dbo].[Docente] ([nombre], [descripcionPerfil], [numeroPersonal], [puesto])
    VALUES
        (N'Pedro Hernández Sánchez',      N'Licenciado en Informática con posgrado en el área de la computación y experiencia en investigación aplicada.', '194', N'Investigador'),
        (N'Ana Juárez Pérez',             N'Licenciada en Ingeniería de Software con experiencia en desarrollo y pruebas de software.',                   '289', N'Docente'),
        (N'Sofía Pedraza Alarcón',        N'Licenciada en Informática con experiencia en soporte y administración de laboratorios de cómputo.',           '679', N'Técnico Académico'),
        (N'Juan Adolfo Higueras Cristal', N'Licenciado en Estadística con experiencia docente en matemáticas y probabilidad.',                            '351', N'Docente por Asignatura');

    INSERT INTO [dbo].[Grado] ([idDocente], [grado], [titulo], [ultimo])
    SELECT d.[idDocente], v.[grado], v.[titulo], v.[ultimo]
    FROM (VALUES
        ('194', N'Licenciatura', N'Informática',                                    0),
        ('194', N'Maestría',     N'Sistemas Interactivos Centrados en el Usuario',  0),
        ('194', N'Doctorado',    N'Ciencias de la Computación',                     1),
        ('289', N'Licenciatura', N'Ingeniería de Software',                         0),
        ('289', N'Maestría',     N'Ingeniería de Software',                         1),
        ('679', N'Licenciatura', N'Informática',                                    1),
        ('351', N'Licenciatura', N'Estadística',                                    0),
        ('351', N'Maestría',     N'Estadística Aplicada',                           1)
    ) v([np], [grado], [titulo], [ultimo])
    JOIN [dbo].[Docente] d ON d.[numeroPersonal] = v.[np];
END
GO

-- Personal externo (sin número de personal). Sólo puede existir uno: el índice
-- UQ_Docente_numeroPersonal no es filtrado y admite un único NULL.
IF NOT EXISTS (SELECT 1 FROM [dbo].[Docente] WHERE [numeroPersonal] IS NULL)
BEGIN
    INSERT INTO [dbo].[Docente] ([nombre], [descripcionPerfil], [numeroPersonal], [puesto])
    VALUES (N'Pedro Hernández Sánchez', N'Licenciado en informática con experiencia en programación orientada a objetos y certificación en lenguajes de programación.', NULL, NULL);

    INSERT INTO [dbo].[Grado] ([idDocente], [grado], [titulo], [ultimo])
    SELECT [idDocente], N'Licenciatura', N'Informática', 1
    FROM [dbo].[Docente] WHERE [numeroPersonal] IS NULL;
END
GO

-- Integrantes del Consejo Técnico de la Facultad de Estadística e Informática.
IF NOT EXISTS (SELECT 1 FROM [dbo].[IntegranteCT] WHERE [nombre] LIKE N'%Minerva Reyes%')
BEGIN
    INSERT INTO [dbo].[IntegranteCT] ([idEntidadAcademica], [nombre], [cargo])
    VALUES
        (1, N'Dr. Minerva Reyes Félix',          N'Secretaria Académica'),
        (1, N'Dr. Edgard Iván Benítez Guerrero', N'Consejero maestro');
END
GO

-- Ofertas del plan de Ingeniería de Software (periodo vigente, Art. 70 y 73):
-- asignadas (con docente), vacantes incluidas, una vacante excluida y dos
-- solicitudes de apertura (pendiente y rechazada).
-- Solo se carga si ya existen las EE y los docentes que referencia (se cargan desde la app);
-- en una base limpia se omite.
IF NOT EXISTS (SELECT 1 FROM [dbo].[Oferta])
   AND (SELECT COUNT(*) FROM [dbo].[ExperienciaEducativa] WHERE [idExperienciaEducativa] IN (4, 5, 6, 8, 9, 10, 20, 30, 40)) = 9
   AND (SELECT COUNT(*) FROM [dbo].[Docente] WHERE [numeroPersonal] IN ('194', '289', '679')) = 3
BEGIN
    DECLARE @d194 INT = (SELECT [idDocente] FROM [dbo].[Docente] WHERE [numeroPersonal] = '194');
    DECLARE @d289 INT = (SELECT [idDocente] FROM [dbo].[Docente] WHERE [numeroPersonal] = '289');
    DECLARE @d679 INT = (SELECT [idDocente] FROM [dbo].[Docente] WHERE [numeroPersonal] = '679');

    INSERT INTO [dbo].[Oferta]
    ([idDocente], [idExperienciaEducativa], [idProgramaEducativo], [idArticulo], [idPeriodo],
     [plaza], [tipoContratacion], [nrc], [incluida], [justificacion], [estadoSolicitudApertura],
     [hsm], [justificacionApertura], [tipoPlaza])
    VALUES
        (@d194, 6,  2, 2, 5, '5442', 'IPP', '42129', 0, NULL, 'Aceptada', 6, NULL, 'Definitiva'),
        (@d289, 8,  2, 2, 5, '4789', 'IOD', '58746', 0, NULL, 'Aceptada', 6, NULL, 'Temporal'),
        (@d679, 9,  2, 2, 5, '3310', 'IOD', '61207', 0, NULL, 'Aceptada', 6, NULL, 'Temporal'),
        (NULL,  20, 2, 2, 5, '3256', 'IPP', '32145', 1, NULL, 'Aceptada', 5, NULL, 'Temporal'),
        (NULL,  30, 2, 2, 5, '4120', 'IOD', '36145', 1, NULL, 'Aceptada', 4, NULL, 'Temporal'),
        (NULL,  40, 2, 2, 5, '5021', 'IPP', '47810', 1, NULL, 'Aceptada', 4, NULL, 'Definitiva'),
        (NULL,  4,  2, 2, 5, '2210', 'IOD', '29866', 0, N'Demanda insuficiente', 'Aceptada', 5, NULL, 'Temporal'),
        (NULL,  10, 2, 2, 5, NULL,   'IOD', '26050', 0, NULL, 'Pendiente', 6, N'Se requiere apertura por aumento de matrícula.', NULL),
        (NULL,  5,  2, 2, 5, NULL,   'IOD', '23118', 0, NULL, 'Rechazada', 5, N'No hay presupuesto para abrir la experiencia.', NULL);

    INSERT INTO [dbo].[Horario] ([idOferta], [dia], [horaInicio], [horaFin], [salon])
    SELECT o.[idOferta], v.[dia], v.[inicio], v.[fin], v.[salon]
    FROM (VALUES
        ('42129', N'Lunes',     '09:00', '10:59', N'FEI/AULA CDS'),
        ('42129', N'Miércoles', '09:00', '10:59', N'FEI/AULA CDS'),
        ('58746', N'Martes',    '11:00', '12:59', N'FEI/AULA 2'),
        ('61207', N'Jueves',    '13:00', '14:59', N'FEI/AULA 3'),
        ('32145', N'Lunes',     '15:00', '15:59', N'ECONEX/AULA 112'),
        ('32145', N'Martes',    '13:00', '14:59', N'ECONEX/AULA 113'),
        ('36145', N'Miércoles', '07:00', '08:59', N'FEI/AULA CDS'),
        ('47810', N'Viernes',   '11:00', '12:59', N'FEI/AULA 4')
    ) v([nrc], [dia], [inicio], [fin], [salon])
    JOIN [dbo].[Oferta] o ON o.[nrc] = v.[nrc];
END
GO

-- Vacantes incluidas en los avisos de ejemplo y horarios de recepción de documentos.
IF NOT EXISTS (SELECT 1 FROM [dbo].[OfertaAviso])
   AND EXISTS (SELECT 1 FROM [dbo].[Aviso])
   AND EXISTS (SELECT 1 FROM [dbo].[Oferta] WHERE [incluida] = 1)
BEGIN
    INSERT INTO [dbo].[OfertaAviso] ([idOferta], [idAviso])
    SELECT o.[idOferta], a.[idAviso]
    FROM [dbo].[Oferta] o CROSS JOIN [dbo].[Aviso] a
    WHERE o.[incluida] = 1;

    INSERT INTO [dbo].[Horario] ([idAviso], [dia], [horaInicio], [horaFin])
    SELECT a.[idAviso], v.[dia], v.[inicio], v.[fin]
    FROM [dbo].[Aviso] a
    CROSS JOIN (VALUES ('2026-09-14', '15:00', '17:00'), ('2026-09-15', '15:00', '17:00')) v([dia], [inicio], [fin]);
END
GO
