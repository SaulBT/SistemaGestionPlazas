-- Asigna claves numéricas sin la condición de carrera de MAX(clave) + 1.
IF OBJECT_ID(N'academico.seq_area_academica_clave', N'SO') IS NULL
BEGIN
    DECLARE @siguiente_clave bigint =
        COALESCE((SELECT MAX(CONVERT(bigint, [clave])) FROM [academico].[area_academica]), 0) + 1;

    IF @siguiente_clave > 2147483647
        THROW 51000, 'No hay claves enteras disponibles para áreas académicas.', 1;

    DECLARE @crear_secuencia nvarchar(max) =
        N'CREATE SEQUENCE [academico].[seq_area_academica_clave] AS int START WITH '
        + CONVERT(nvarchar(20), @siguiente_clave)
        + N' INCREMENT BY 1 NO CYCLE;';
    EXEC sys.sp_executesql @crear_secuencia;
END;
