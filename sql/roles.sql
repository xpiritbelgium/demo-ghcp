IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '##placeholder_identity##')
BEGIN
    CREATE USER [##placeholder_identity##] FROM EXTERNAL PROVIDER;
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members rm JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id WHERE member_principal_id = USER_ID('##placeholder_identity##') AND r.name = 'db_datareader')
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [##placeholder_identity##];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members rm JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id WHERE member_principal_id = USER_ID('##placeholder_identity##') AND r.name = 'db_datawriter')
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [##placeholder_identity##];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '##placeholder_identity##')
BEGIN
    CREATE USER [##placeholder_githubprincipal##] FROM EXTERNAL PROVIDER;
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members rm JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id WHERE member_principal_id = USER_ID('##placeholder_identity##') AND r.name = 'db_ddladmin')
BEGIN
    ALTER ROLE db_ddladmin ADD MEMBER [##placeholder_githubprincipal##];
END
