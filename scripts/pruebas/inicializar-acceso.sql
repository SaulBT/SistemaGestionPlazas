-- Se ejecuta únicamente mediante exec en el servidor SQL del proyecto sgpla-pruebas.
-- No contiene USE, DELETE, TRUNCATE ni DROP. No copia datos de otra base.
SET XACT_ABORT ON;
IF DB_NAME() <> N'GestionDePlazasBD'
    THROW 50001, 'La base de destino no es la esperada.', 1;

DECLARE @correo nvarchar(255) = N'$(Administrador)';
IF LEN(@correo) < 5 OR @correo NOT LIKE N'%@%'
    THROW 50002, 'Debe proporcionar un correo institucional.', 1;

BEGIN TRANSACTION;
-- Solo crea el acceso inicial si la base todavía no tiene administrador.
IF NOT EXISTS (SELECT 1 FROM dbo.SuperUsuario WITH (UPDLOCK, HOLDLOCK))
BEGIN
    INSERT INTO dbo.SuperUsuario (nombre, correo)
    VALUES (N'Administrador de pruebas', @correo);
END;
COMMIT;
PRINT 'Acceso inicial comprobado. No se modificaron registros existentes.';
