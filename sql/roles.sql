IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '##placeholder_identity##')
BEGIN
    CREATE USER [##placeholder_identity##] FROM EXTERNAL PROVIDER;
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('##placeholder_identity##') AND role_principal_id = DATABASE_ROLE_ID('db_datareader'))
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [##placeholder_identity##];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('##placeholder_identity##') AND role_principal_id = DATABASE_ROLE_ID('db_datawriter'))
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [##placeholder_identity##];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('##placeholder_identity##') AND role_principal_id = DATABASE_ROLE_ID('db_ddladmin'))
BEGIN
    ALTER ROLE db_ddladmin ADD MEMBER [##placeholder_identity##];
END
