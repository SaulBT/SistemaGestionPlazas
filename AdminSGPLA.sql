USE [master];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = 'admin_sgpla'
)
BEGIN
    CREATE LOGIN [admin_sgpla]
    WITH PASSWORD = 'sgpla123',
         CHECK_POLICY = OFF,
         CHECK_EXPIRATION = OFF;
END
GO

USE [GestionDePlazasBD];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = 'admin_sgpla'
)
BEGIN
    CREATE USER [admin_sgpla]
    FOR LOGIN [admin_sgpla]
    WITH DEFAULT_SCHEMA = [dbo];
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    INNER JOIN sys.database_principals r
        ON drm.role_principal_id = r.principal_id
    INNER JOIN sys.database_principals u
        ON drm.member_principal_id = u.principal_id
    WHERE r.name = 'db_owner'
      AND u.name = 'admin_sgpla'
)
BEGIN
    ALTER ROLE [db_owner] ADD MEMBER [admin_sgpla];
END
GO