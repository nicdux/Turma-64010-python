# Deployment prerequisites

## Entra directory supplied

The deployment target supplied for BARBERCLOUD is:

| Entra field | Value |
| --- | --- |
| Directory name | Diretório Padrão |
| Domain | `suportenicolashotmail.onmicrosoft.com` |
| Directory (tenant) ID | `094b4847-83d3-42be-b67d-c18dd6c0e29d` |

The directory ID is configured as the API's trusted token issuer and as the default tenant parameter in Bicep. It is **not** an Azure subscription ID, an API audience, a SaaS tenant ID for Barbearia Avelino, nor the object ID of the SQL administrator group.

## Required before infrastructure deployment

1. An active Azure subscription in that directory, with its subscription ID and permission to create the resource group, VNet/private endpoints, App Service, SQL, Storage, Key Vault, and Monitor resources.
2. Explicit budget/cost approval. The baseline includes paid App Service, SQL and private endpoints. `advancedServices` remains `false`; turning it on creates additional paid services.
3. An Entra security group to administer Azure SQL. Supply its **group object ID** and display name as the SQL administrator inputs. The directory ID provided above cannot be used in place of the group object ID.
4. Entra app registration and API audience (`api://...` or the registered application ID URI), plus a trusted issuance/mapping policy that emits a UUID `tenant_id` claim. The API audience is intentionally not guessed.
5. GitHub repository creation/write access. The current GitHub credential returns HTTP 403 for private repository creation; no repository or remote exists yet. After creating the repository, add `AZURE_CLIENT_ID` and `AZURE_SUBSCRIPTION_ID` as GitHub environment variables and configure the federated credential for the repository's GitHub Actions subject.
6. A GitHub deployment environment named `dev`, `staging`, or `prod`; configure required reviewers for `prod` and required CI checks/branch protection. Workflows cannot grant themselves these repository permissions.
7. After provisioning, run `database/tenant-isolation.sql` as the SQL administrator, then follow `database/bootstrap-tenant-template.sql` with the actual App Service managed-identity display name and a separate SaaS tenant UUID.
8. Complete the database login/user mapping and verify the App Service can resolve and reach the private SQL endpoint before deploying the API. The workload's system-assigned identity is not granted SQL access by Bicep alone.

## DNS and custom domains

The requested names remain `app.barbercloud.com.br`, `admin.barbercloud.com.br`, `api.barbercloud.com.br`, and `tenant.barbercloud.com.br`. No Azure custom domain target or domain-validation token exists until the corresponding app is provisioned. Do not publish speculative DNS records. After deployment, retrieve the actual Azure hostname and verification record, then add the exact GoDaddy CNAME/TXT values and HTTPS binding.

## WhatsApp and email

No Meta/WhatsApp or Communication Services account, sender verification, approved template, or credential is configured. These integrations are not activated by the infrastructure template and should be enabled only after the business supplies approved credentials through the secure secret store.
