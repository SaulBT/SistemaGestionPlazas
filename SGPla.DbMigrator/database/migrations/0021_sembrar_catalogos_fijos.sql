SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

/* Catálogos oficiales/fijos del modelo nuevo. Nunca modifica dbo.
   Los nombres de municipio corresponden al catálogo vigente de Veracruz;
   SGPLa conserva únicamente IDs internos estables, sin clave INEGI. */
IF NOT EXISTS (SELECT 1 FROM [usuarios].[rol] WHERE [id] = 1)
    INSERT INTO [usuarios].[rol] ([id], [nombre]) VALUES (1, N'Superusuario');
IF NOT EXISTS (SELECT 1 FROM [usuarios].[rol] WHERE [id] = 2)
    INSERT INTO [usuarios].[rol] ([id], [nombre]) VALUES (2, N'DGAA');
IF NOT EXISTS (SELECT 1 FROM [usuarios].[rol] WHERE [id] = 3)
    INSERT INTO [usuarios].[rol] ([id], [nombre]) VALUES (3, N'Entidad Académica');
IF EXISTS
(
    SELECT 1
    FROM (VALUES (1, N'Superusuario'), (2, N'DGAA'), (3, N'Entidad Académica')) AS esperado([id], [nombre])
    INNER JOIN [usuarios].[rol] AS rol ON rol.[id] = esperado.[id]
    WHERE rol.[nombre] <> esperado.[nombre]
)
BEGIN
    THROW 51003, N'El catálogo usuarios.rol contiene nombres incompatibles con el modelo.', 1;
END;

DECLARE @municipios TABLE ([nombre] nvarchar(150) NOT NULL PRIMARY KEY);
INSERT INTO @municipios ([nombre])
VALUES
    (N'Acajete'), (N'Acatlán'), (N'Acayucan'), (N'Actopan'), (N'Acula'), (N'Acultzingo'),
    (N'Camarón de Tejeda'), (N'Alpatláhuac'), (N'Alto Lucero de Gutiérrez Barrios'), (N'Altotonga'),
    (N'Alvarado'), (N'Amatitlán'), (N'Naranjos Amatlán'), (N'Amatlán de los Reyes'),
    (N'Ángel R. Cabada'), (N'La Antigua'), (N'Apazapan'), (N'Aquila'), (N'Astacinga'),
    (N'Atlahuilco'), (N'Atoyac'), (N'Atzacan'), (N'Atzalan'), (N'Tlaltetela'), (N'Ayahualulco'),
    (N'Banderilla'), (N'Benito Juárez'), (N'Boca del Río'), (N'Calcahualco'), (N'Camerino Z. Mendoza'),
    (N'Carrillo Puerto'), (N'Catemaco'), (N'Cazones de Herrera'), (N'Cerro Azul'), (N'Citlaltépetl'),
    (N'Coacoatzintla'), (N'Coahuitlán'), (N'Coatepec'), (N'Coatzacoalcos'), (N'Coatzintla'),
    (N'Coetzala'), (N'Colipa'), (N'Comapa'), (N'Córdoba'), (N'Cosamaloapan de Carpio'),
    (N'Cosautlán de Carvajal'), (N'Coscomatepec'), (N'Cosoleacaque'), (N'Cotaxtla'), (N'Coxquihui'),
    (N'Coyutla'), (N'Cuichapa'), (N'Cuitláhuac'), (N'Chacaltianguis'), (N'Chalma'), (N'Chiconamel'),
    (N'Chiconquiaco'), (N'Chicontepec'), (N'Chinameca'), (N'Chinampa de Gorostiza'), (N'Las Choapas'),
    (N'Chocamán'), (N'Chontla'), (N'Chumatlán'), (N'Emiliano Zapata'), (N'Espinal'), (N'Filomeno Mata'),
    (N'Fortín'), (N'Gutiérrez Zamora'), (N'Hidalgotitlán'), (N'Huatusco'), (N'Huayacocotla'),
    (N'Hueyapan de Ocampo'), (N'Huiloapan de Cuauhtémoc'), (N'Ignacio de la Llave'), (N'Ilamatlán'),
    (N'Isla'), (N'Ixcatepec'), (N'Ixhuacán de los Reyes'), (N'Ixhuatlán del Café'), (N'Ixhuatlancillo'),
    (N'Ixhuatlán del Sureste'), (N'Ixhuatlán de Madero'), (N'Ixmatlahuacan'), (N'Ixtaczoquitlán'),
    (N'Jalacingo'), (N'Xalapa'), (N'Jalcomulco'), (N'Jáltipan'), (N'Jamapa'), (N'Jesús Carranza'),
    (N'Xico'), (N'Jilotepec'), (N'Juan Rodríguez Clara'), (N'Juchique de Ferrer'), (N'Landero y Coss'),
    (N'Lerdo de Tejada'), (N'Magdalena'), (N'Maltrata'), (N'Manlio Fabio Altamirano'), (N'Mariano Escobedo'),
    (N'Martínez de la Torre'), (N'Mecatlán'), (N'Mecayapan'), (N'Medellín de Bravo'), (N'Miahuatlán'),
    (N'Las Minas'), (N'Minatitlán'), (N'Misantla'), (N'Mixtla de Altamirano'), (N'Moloacán'),
    (N'Naolinco'), (N'Naranjal'), (N'Nautla'), (N'Nogales'), (N'Oluta'), (N'Omealca'), (N'Orizaba'),
    (N'Otatitlán'), (N'Oteapan'), (N'Ozuluama de Mascareñas'), (N'Pajapan'), (N'Pánuco'), (N'Papantla'),
    (N'Paso del Macho'), (N'Paso de Ovejas'), (N'La Perla'), (N'Perote'), (N'Platón Sánchez'),
    (N'Playa Vicente'), (N'Poza Rica de Hidalgo'), (N'Las Vigas de Ramírez'), (N'Pueblo Viejo'),
    (N'Puente Nacional'), (N'Rafael Delgado'), (N'Rafael Lucio'), (N'Los Reyes'), (N'Río Blanco'),
    (N'Saltabarranca'), (N'San Andrés Tenejapan'), (N'San Andrés Tuxtla'), (N'San Juan Evangelista'),
    (N'Santiago Tuxtla'), (N'Sayula de Alemán'), (N'Soconusco'), (N'Sochiapa'), (N'Soledad Atzompa'),
    (N'Soledad de Doblado'), (N'Soteapan'), (N'Tamalín'), (N'Tamiahua'), (N'Tampico Alto'),
    (N'Tancoco'), (N'Tantima'), (N'Tantoyuca'), (N'Tatatila'), (N'Castillo de Teayo'), (N'Tecolutla'),
    (N'Tehuipango'), (N'Álamo Temapache'), (N'Tempoal'), (N'Tenampa'), (N'Tenochtitlán'), (N'Teocelo'),
    (N'Tepatlaxco'), (N'Tepetlán'), (N'Tepetzintla'), (N'Tequila'), (N'José Azueta'), (N'Texcatepec'),
    (N'Texhuacán'), (N'Texistepec'), (N'Tezonapa'), (N'Tierra Blanca'), (N'Tihuatlán'), (N'Tlacojalpan'),
    (N'Tlacolulan'), (N'Tlacotalpan'), (N'Tlacotepec de Mejía'), (N'Tlachichilco'), (N'Tlalixcoyan'),
    (N'Tlalnelhuayocan'), (N'Tlapacoyan'), (N'Tlaquilpa'), (N'Tlilapan'), (N'Tomatlán'), (N'Tonayán'),
    (N'Totutla'), (N'Tuxpan'), (N'Tuxtilla'), (N'Úrsulo Galván'), (N'Vega de Alatorre'), (N'Veracruz'),
    (N'Villa Aldama'), (N'Xoxocotla'), (N'Yanga'), (N'Yecuatla'), (N'Zacualpan'), (N'Zaragoza'),
    (N'Zentla'), (N'Zongolica'), (N'Zontecomatlán de López y Fuentes'), (N'Zozocolco de Hidalgo'),
    (N'Agua Dulce'), (N'El Higo'), (N'Nanchital de Lázaro Cárdenas del Río'), (N'Tres Valles'),
    (N'Carlos A. Carrillo'), (N'Tatahuicapan de Juárez'), (N'Uxpanapa'), (N'San Rafael'),
    (N'Santiago Sochiapan');

INSERT INTO [academico].[municipio] ([nombre])
SELECT m.[nombre]
FROM @municipios AS m
WHERE NOT EXISTS
(
    SELECT 1
    FROM [academico].[municipio] AS actual
    WHERE actual.[nombre] = m.[nombre] COLLATE Modern_Spanish_100_CI_AI
);

IF (SELECT COUNT(*) FROM [usuarios].[rol]) < 3
BEGIN
    THROW 51001, N'El catálogo usuarios.rol no contiene los tres roles requeridos.', 1;
END;
IF (SELECT COUNT(*) FROM [academico].[municipio]) < 212
BEGIN
    THROW 51002, N'El catálogo academico.municipio no contiene los 212 municipios de Veracruz.', 1;
END;
GO
