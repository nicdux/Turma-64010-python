-- Run as the configured Entra SQL administrator after tenant-isolation.sql.
-- Replace every placeholder from the deployment outputs/approved onboarding request.
-- This principal must be the App Service system-assigned managed identity.

CREATE USER [<api-managed-identity-display-name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<api-managed-identity-display-name>];
ALTER ROLE db_datawriter ADD MEMBER [<api-managed-identity-display-name>];
GO

DECLARE @tenant_id UNIQUEIDENTIFIER = '<saas-tenant-guid>';
DECLARE @display_name NVARCHAR(160) = N'Barbearia Avelino';

EXEC sys.sp_set_session_context @key = N'tenant_id', @value = @tenant_id;
INSERT INTO core.Tenants (tenant_id, display_name)
VALUES (@tenant_id, @display_name);
EXEC sys.sp_set_session_context @key = N'tenant_id', @value = NULL;
GO
