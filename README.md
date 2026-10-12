# BARBERCLOUD CRM

BARBERCLOUD is a multi-tenant CRM foundation for barbershops. This repository currently contains an interactive dark-mode dashboard with clearly labeled demo data, a .NET API starter with Entra JWT validation and tenant-claim enforcement, Azure Bicep infrastructure, and CI workflows.

> **Deployment status:** local foundation only. No Azure resources, production data, third-party messaging, or production domain have been configured or deployed. Dashboard figures are illustrative, not connected to a database.

## Repository layout

| Path | Purpose |
| --- | --- |
| `frontend/` | Next.js, React, TypeScript dashboard |
| `backend/` | ASP.NET Core API and tests |
| `infrastructure/bicep/` | Azure infrastructure templates |
| `database/` | Database design and tenant-isolation notes |
| `devops/` | Operational configuration |
| `docs/` | Architecture, runbooks, ADRs, and roadmap |
| `tests/e2e/` | End-to-end test configuration |
| `scripts/` | Local developer utilities |
| `.github/workflows/` | CI, security scanning, and gated Azure deployment |

## Run locally

Prerequisites: Node.js 20+, .NET 9 SDK, and npm.

```bash
cd frontend
npm ci
npm run dev
```

The dashboard is available at `http://localhost:3000`. Its metrics are sample data.

```bash
cd backend
Entra__TenantId="<entra-directory-id>" \
Entra__Audience="<api-application-id-uri>" \
dotnet run --project src/BarberCloud.Api --urls http://localhost:5080
```

The API listens on `http://localhost:5080`; `/health` is public and every `/v1` route requires an Entra-issued bearer token carrying a trusted UUID `tenant_id` claim. The values above must come from an Entra app registration configured to issue that claim.

## Validate

```bash
cd frontend && npm ci && npm run lint && npm run build
cd backend && dotnet test
```

## Tenant security boundary

Production tenant identity is obtained from the validated token's `tenant_id` claim; callers cannot select a tenant with a request header. Configure `Entra__TenantId`, `Entra__Audience`, and a trusted identity application's claim transformation before exposing the API. The API rejects tokens without the tenant claim. EF Core tenant filters and write guards, SQL `SESSION_CONTEXT`, SQL row-level security DDL, and transactional audit-event persistence are implemented in the local foundation. They have not been provisioned or verified in Azure; do not store real customer data until the deployment prerequisites and production-readiness checklist are complete.

## Azure deployment

The Bicep entry point is `infrastructure/bicep/main.bicep`. It provisions a baseline API, SQL, storage, Key Vault, monitoring, VNet integration and private endpoints. Advanced paid services are explicitly gated and off by default. The supplied Entra directory is documented in `docs/deployment-prerequisites.md`. Review that checklist, obtain subscription/cost approval, and configure GitHub OIDC before deployment. The GitHub infrastructure workflow is manual and does not deploy on every push.

## Roadmap and known gaps

The API currently persists customers, barbers, services, and appointments locally/through Azure SQL, scopes queries to the validated token tenant claim, applies SQL session context for RLS, and records mutation audits. Before production: provision and verify the SQL schema/RLS, grant the managed identity database access, complete Entra/External ID flows and SaaS tenant provisioning, connect the frontend, add authorization roles and audit-retention controls, operational alerting, backups, accessibility/load/security tests, and third-party integrations. See `docs/architecture.md` and `docs/deployment-prerequisites.md`.
