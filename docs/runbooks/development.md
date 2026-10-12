# Developer runbook

## Start the dashboard

```bash
cd frontend
npm ci
npm run dev
```

Open `http://localhost:3000`. All visible business metrics are sample values.

## Start the API

Configure a development Entra directory and API audience in environment variables (do not commit them):

```bash
cd backend
Entra__TenantId="<entra-directory-id>" \
Entra__Audience="<api-application-id-uri>" \
dotnet run --project src/BarberCloud.Api --urls http://localhost:5080
```

`GET /health` does not require a token. `GET /v1/dashboard/summary` needs a valid access token with a UUID `tenant_id` claim. Swagger UI is available only in Development.

## CI and branch protection

The quality workflow builds/lints/tests the code on push and pull request. CodeQL scans C# and TypeScript. Configure GitHub branch protection separately: require pull requests, the `quality` and CodeQL checks, at least one review, and no force-push/deletion on `main` and `develop`. CI files alone cannot enforce repository rules.

## Azure provisioning

Review `infrastructure/bicep/README.md`, confirm pricing/quotas/data residency with the subscription owner, authenticate Azure CLI, and complete the private networking and Entra setup before deploying. The workflow named `Provision Azure infrastructure` is manual only and creates infrastructure, not application releases. Start with `what-if`; production requires explicit approval.

## Incident basics

1. Declare an incident lead and record UTC start time, affected environment/tenant scope, and customer impact.
2. Preserve relevant Application Insights/Log Analytics evidence with restricted access; avoid copying PII into tickets or chat.
3. For suspected cross-tenant exposure, disable the affected API access path, preserve audit evidence, involve the data controller/privacy lead, and follow the approved LGPD incident process.
4. Rotate only affected credentials through Key Vault/identity provider. Do not paste tokens or secrets into runbooks, issues, or commits.
5. Restore from a verified backup only after a recovery point and tenant scope are confirmed. Backup/restore automation and incident SLAs remain to be implemented.
