CREATE USER [app-cbn-dev] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [app-cbn-dev];
ALTER ROLE db_datawriter ADD MEMBER [app-cbn-dev];
ALTER ROLE db_ddladmin ADD MEMBER [app-cbn-dev]; --remove when migration moved to release