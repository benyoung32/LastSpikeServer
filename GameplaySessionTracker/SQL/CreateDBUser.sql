CREATE USER thelastspike FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER thelastspike;
ALTER ROLE db_datawriter ADD MEMBER thelastspike;
ALTER ROLE db_ddladmin ADD MEMBER thelastspike;
GO
