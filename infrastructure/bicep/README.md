# Azure deployment

`main.bicep` is a resource-group deployment for the first Azure footprint. Baseline resources are App Service, Azure SQL (Entra-only administration), Storage, Key Vault, Static Web Apps, Log Analytics, and Application Insights. Redis, Service Bus, Event Grid, Azure OpenAI, Communication Services, and Power BI Embedded are optional because enabling them can create ongoing charges and requires service-specific configuration and network integration.

Before deployment:

1. Review regional availability, SKU pricing, quotas, and data residency with the subscription owner. The production App Service and SQL SKUs incur charges.
2. Create/select an Entra security group for SQL administration and supply its object ID/display name.
3. Configure the API's Entra tenant ID and audience; replace the deployment placeholders with parameters before production.
4. Add private endpoints and private DNS for SQL, Key Vault, and Storage, plus App Service VNet integration. Public access is disabled and the starter does not yet provision this network topology; the API cannot reach those private resources until it is added.
5. Configure federated GitHub Actions credentials and authenticate Azure CLI. The workflow is `workflow_dispatch` only.

Example validation and deployment:

```bash
az bicep build --file infrastructure/bicep/main.bicep
az group create --name rg-barbercloud-dev --location brazilsouth
az deployment group what-if \
  --resource-group rg-barbercloud-dev \
  --template-file infrastructure/bicep/main.bicep \
  --parameters environment=dev sqlEntraAdminObjectId="<group-object-id>" sqlEntraAdminName="<group-name>" entraTenantId="<entra-tenant-id>" apiAudience="<api-audience>"
```

Do not run the deployment until cost, subscription, identity, quota, and network prerequisites have been reviewed. Advanced services are disabled by default (`advancedServices=false`).
