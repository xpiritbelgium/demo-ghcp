IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'app-cbn-dev')
BEGIN
    CREATE USER [app-cbn-dev] FROM EXTERNAL PROVIDER;
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('app-cbn-dev') AND role_principal_id = DATABASE_ROLE_ID('db_datareader'))
BEGIN
    ALTER ROLE db_datareader ADD MEMBER [app-cbn-dev];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('app-cbn-dev') AND role_principal_id = DATABASE_ROLE_ID('db_datawriter'))
BEGIN
    ALTER ROLE db_datawriter ADD MEMBER [app-cbn-dev];
END

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = USER_ID('app-cbn-dev') AND role_principal_id = DATABASE_ROLE_ID('db_ddladmin'))
BEGIN
    ALTER ROLE db_ddladmin ADD MEMBER [app-cbn-dev];
END
